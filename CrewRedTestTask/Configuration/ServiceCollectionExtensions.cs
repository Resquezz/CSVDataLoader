using CrewRedTestTask.Application.Configuration;
using CrewRedTestTask.Application.Interfaces;
using CrewRedTestTask.Application.Services;
using CrewRedTestTask.Infrastructure.Data;
using CrewRedTestTask.Infrastructure.Repositories;
using CrewRedTestTask.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CrewRedTestTask.Configuration;

internal static class ServiceCollectionExtensions
{
	public static IServiceCollection AddTaxiTripImporting(this IServiceCollection services, CliOptions options)
	{
		services.AddSingleton(options);
		services.AddDbContextFactory<TaxiTripsDbContext>(builder => builder.UseSqlServer(options.ConnectionString));
		services.AddScoped<ICsvTaxiTripParser, CsvTaxiTripParser>();
		services.AddScoped<IDuplicateTripWriterFactory, DuplicateTripWriterFactory>();
		services.AddScoped<ITaxiTripRepository, TaxiTripRepository>();
		services.AddScoped<ITaxiTripImportService, TaxiTripImportService>();

		return services;
	}
}