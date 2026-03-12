using System.Globalization;
using CrewRedTestTask.Application.Interfaces;
using CrewRedTestTask.Domain.Entities;
using Microsoft.VisualBasic.FileIO;

namespace CrewRedTestTask.Application.Services;

public sealed class CsvTaxiTripParser : ICsvTaxiTripParser
{
	private const int CsvColumnCount = 18;
	private static readonly TimeSpan EstUtcOffset = TimeSpan.FromHours(-5);
	private static readonly string[] ExpectedHeaders =
	[
		"VendorID",
		"tpep_pickup_datetime",
		"tpep_dropoff_datetime",
		"passenger_count",
		"trip_distance",
		"RatecodeID",
		"store_and_fwd_flag",
		"PULocationID",
		"DOLocationID",
		"payment_type",
		"fare_amount",
		"extra",
		"mta_tax",
		"tip_amount",
		"tolls_amount",
		"improvement_surcharge",
		"total_amount",
		"congestion_surcharge"
	];

	public TextFieldParser CreateParser(string inputPath)
	{
		var parser = new TextFieldParser(inputPath)
		{
			TextFieldType = FieldType.Delimited,
			HasFieldsEnclosedInQuotes = true,
			TrimWhiteSpace = false
		};

		parser.SetDelimiters(",");
		return parser;
	}

	public bool HasExpectedHeader(string[]? header)
	{
		return header is not null && ExpectedHeaders.SequenceEqual(header.Select(static value => value.Trim()));
	}

	public bool TryParse(string[] rawFields, out TaxiTripRecord record)
	{
		if (rawFields.Length != CsvColumnCount)
		{
			record = default;
			return false;
		}

		var fields = rawFields.Select(static value => value.Trim()).ToArray();

		if (!TryParseInputDate(fields[1], out var pickupLocal) || !TryParseInputDate(fields[2], out var dropoffLocal))
		{
			record = default;
			return false;
		}

		if (!byte.TryParse(fields[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out var passengerCount) ||
			!decimal.TryParse(fields[4], NumberStyles.Number, CultureInfo.InvariantCulture, out var tripDistance) ||
			!short.TryParse(fields[7], NumberStyles.Integer, CultureInfo.InvariantCulture, out var pickupLocationId) ||
			!short.TryParse(fields[8], NumberStyles.Integer, CultureInfo.InvariantCulture, out var dropoffLocationId) ||
			!decimal.TryParse(fields[10], NumberStyles.Number, CultureInfo.InvariantCulture, out var fareAmount) ||
			!decimal.TryParse(fields[13], NumberStyles.Number, CultureInfo.InvariantCulture, out var tipAmount))
		{
			record = default;
			return false;
		}

		var normalizedStoreAndFwd = NormalizeStoreAndForwardFlag(fields[6]);
		if (normalizedStoreAndFwd is null)
		{
			record = default;
			return false;
		}

		var pickupUtc = new DateTimeOffset(DateTime.SpecifyKind(pickupLocal, DateTimeKind.Unspecified), EstUtcOffset).UtcDateTime;
		var dropoffUtc = new DateTimeOffset(DateTime.SpecifyKind(dropoffLocal, DateTimeKind.Unspecified), EstUtcOffset).UtcDateTime;

		record = new TaxiTripRecord(
			pickupUtc,
			dropoffUtc,
			passengerCount,
			decimal.Round(tripDistance, 2, MidpointRounding.AwayFromZero),
			normalizedStoreAndFwd,
			pickupLocationId,
			dropoffLocationId,
			decimal.Round(fareAmount, 2, MidpointRounding.AwayFromZero),
			decimal.Round(tipAmount, 2, MidpointRounding.AwayFromZero));

		return true;
	}

	private static bool TryParseInputDate(string value, out DateTime parsed)
	{
		return DateTime.TryParseExact(
			value,
			"MM/dd/yyyy hh:mm:ss tt",
			CultureInfo.InvariantCulture,
			DateTimeStyles.AllowWhiteSpaces,
			out parsed);
	}

	private static string? NormalizeStoreAndForwardFlag(string value)
	{
		return value.ToUpperInvariant() switch
		{
			"N" => "No",
			"Y" => "Yes",
			"NO" => "No",
			"YES" => "Yes",
			_ => null
		};
	}
}