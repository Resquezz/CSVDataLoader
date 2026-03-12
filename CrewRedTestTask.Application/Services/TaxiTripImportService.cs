using CrewRedTestTask.Application.Interfaces;
using CrewRedTestTask.Application.Configuration;
using CrewRedTestTask.Domain.Entities;
using CrewRedTestTask.Domain.Interfaces;
using Microsoft.VisualBasic.FileIO;

namespace CrewRedTestTask.Application.Services;

public sealed class TaxiTripImportService : ITaxiTripImportService
{
	private const int BatchSize = 10_000;
	private readonly CliOptions _options;
	private readonly ICsvTaxiTripParser _csvTaxiTripParser;
	private readonly IDuplicateTripWriterFactory _duplicateTripWriterFactory;
	private readonly ITaxiTripRepository _taxiTripRepository;

	public TaxiTripImportService(
		CliOptions options,
		ICsvTaxiTripParser csvTaxiTripParser,
		IDuplicateTripWriterFactory duplicateTripWriterFactory,
		ITaxiTripRepository taxiTripRepository)
	{
		_options = options;
		_csvTaxiTripParser = csvTaxiTripParser;
		_duplicateTripWriterFactory = duplicateTripWriterFactory;
		_taxiTripRepository = taxiTripRepository;
	}

	public async Task<ImportResult> RunAsync(CancellationToken cancellationToken = default)
	{
		await _taxiTripRepository.InitializeAsync(cancellationToken);

		var dedupeKeys = new HashSet<TripDeduplicationKey>();
		var batch = new List<TaxiTripRecord>(BatchSize);
		var insertedRows = 0;
		var duplicateRows = 0;
		var invalidRows = 0;

		await using var duplicateWriter = await _duplicateTripWriterFactory.CreateAsync(cancellationToken);
		using var parser = _csvTaxiTripParser.CreateParser(_options.InputPath);

		var header = parser.ReadFields();
		if (!_csvTaxiTripParser.HasExpectedHeader(header))
		{
			throw new InvalidOperationException("Unexpected CSV header. The importer only supports the assessment input format.");
		}

		while (!parser.EndOfData)
		{
			cancellationToken.ThrowIfCancellationRequested();

			string[]? fields;
			try
			{
				fields = parser.ReadFields();
			}
			catch (MalformedLineException)
			{
				invalidRows++;
				continue;
			}

			if (fields is null)
			{
				continue;
			}

			if (!_csvTaxiTripParser.TryParse(fields, out var record))
			{
				invalidRows++;
				continue;
			}

			var dedupeKey = new TripDeduplicationKey(record.PickupDateUtc, record.DropoffDateUtc, record.PassengerCount);
			if (!dedupeKeys.Add(dedupeKey))
			{
				duplicateRows++;
				await duplicateWriter.WriteAsync(record.ToDuplicateRow(), cancellationToken);
				continue;
			}

			batch.Add(record);
			if (batch.Count < BatchSize)
			{
				continue;
			}

			insertedRows += await FlushBatchAsync(batch, cancellationToken);
		}

		insertedRows += await FlushBatchAsync(batch, cancellationToken);

		return new ImportResult(insertedRows, duplicateRows, invalidRows, duplicateWriter.OutputPath);
	}

	private async Task<int> FlushBatchAsync(List<TaxiTripRecord> batch, CancellationToken cancellationToken)
	{
		if (batch.Count == 0)
		{
			return 0;
		}

		var insertedRows = await _taxiTripRepository.BulkInsertAsync(batch, cancellationToken);
		batch.Clear();
		return insertedRows;
	}
}