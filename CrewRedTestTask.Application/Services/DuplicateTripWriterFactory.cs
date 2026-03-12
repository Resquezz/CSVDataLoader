using CrewRedTestTask.Application.Interfaces;
using CrewRedTestTask.Application.Configuration;

namespace CrewRedTestTask.Application.Services;

public sealed class DuplicateTripWriterFactory : IDuplicateTripWriterFactory
{
	private static readonly string[] DuplicateCsvHeader =
	[
		"tpep_pickup_datetime",
		"tpep_dropoff_datetime",
		"passenger_count",
		"trip_distance",
		"store_and_fwd_flag",
		"PULocationID",
		"DOLocationID",
		"fare_amount",
		"tip_amount"
	];

	private readonly CliOptions _options;

	public DuplicateTripWriterFactory(CliOptions options)
	{
		_options = options;
	}

	public async Task<IDuplicateTripWriter> CreateAsync(CancellationToken cancellationToken = default)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(_options.DuplicatesOutputPath) ?? AppContext.BaseDirectory);

		var writer = new StreamWriter(_options.DuplicatesOutputPath, false);
		await writer.WriteLineAsync(ToCsv(DuplicateCsvHeader));
		cancellationToken.ThrowIfCancellationRequested();

		return new DuplicateTripWriter(_options.DuplicatesOutputPath, writer);
	}

	private static string ToCsv(IEnumerable<string> values)
	{
		return string.Join(',', values.Select(static value =>
		{
			var escaped = value.Replace("\"", "\"\"", StringComparison.Ordinal);
			return escaped.Contains(',') || escaped.Contains('"')
				? $"\"{escaped}\""
				: escaped;
		}));
	}

	private sealed class DuplicateTripWriter : IDuplicateTripWriter
	{
		private readonly StreamWriter _writer;

		public DuplicateTripWriter(string outputPath, StreamWriter writer)
		{
			OutputPath = outputPath;
			_writer = writer;
		}

		public string OutputPath { get; }

		public Task WriteAsync(string[] values, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return _writer.WriteLineAsync(ToCsv(values));
		}

		public ValueTask DisposeAsync()
		{
			return _writer.DisposeAsync();
		}
	}
}