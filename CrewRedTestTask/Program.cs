using CrewRedTestTask.Application.Configuration;
using CrewRedTestTask.Application.Interfaces;
using CrewRedTestTask.Configuration;
using Microsoft.Extensions.DependencyInjection;

var options = CliOptions.Parse(args);

if (!File.Exists(options.InputPath))
{
	Console.Error.WriteLine($"Input file not found: {options.InputPath}");
	return 1;
}

var services = new ServiceCollection();
services.AddTaxiTripImporting(options);

await using var serviceProvider = services.BuildServiceProvider();
var importer = serviceProvider.GetRequiredService<ITaxiTripImportService>();
var result = await importer.RunAsync();

Console.WriteLine($"Rows inserted: {result.InsertedRows}");
Console.WriteLine($"Duplicate rows removed: {result.DuplicateRows}");
Console.WriteLine($"Invalid rows skipped: {result.InvalidRows}");
Console.WriteLine($"Duplicates written to: {result.DuplicatesFilePath}");

return 0;
