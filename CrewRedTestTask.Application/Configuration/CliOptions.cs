namespace CrewRedTestTask.Application.Configuration;

public sealed record CliOptions(
	string InputPath,
	string DuplicatesOutputPath,
	string ConnectionString,
	string DestinationTable)
{
	public static CliOptions Parse(string[] args)
	{
		var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		for (var index = 0; index < args.Length; index++)
		{
			var current = args[index];
			if (!current.StartsWith("--", StringComparison.Ordinal))
			{
				continue;
			}

			var key = current[2..];
			if (index + 1 >= args.Length)
			{
				throw new ArgumentException($"Missing value for argument --{key}");
			}

			values[key] = args[++index];
		}

		var inputPath = GetValue(values, "input", Path.Combine(Environment.CurrentDirectory, "sample-cab-data.csv"));
		var duplicatesOutputPath = GetValue(values, "duplicates-output", Path.Combine(Environment.CurrentDirectory, "duplicates.csv"));
		var destinationTable = GetValue(values, "table", "dbo.TaxiTrips");

			var connectionString = GetValue(
				values,
				"connection-string",
				Environment.GetEnvironmentVariable("CREWRED_SQL_CONNECTION") ?? string.Empty
			);

			if (string.IsNullOrWhiteSpace(connectionString))
			{
				throw new InvalidOperationException("No connection string provided. Set the --connection-string option or the CREWRED_SQL_CONNECTION environment variable.");
			}

		return new CliOptions(
			Path.GetFullPath(inputPath),
			Path.GetFullPath(duplicatesOutputPath),
			connectionString,
			destinationTable);
	}

	private static string GetValue(IReadOnlyDictionary<string, string> values, string key, string defaultValue)
	{
		return values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
			? value
			: defaultValue;
	}
}