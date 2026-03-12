using CrewRedTestTask.Domain.Entities;

namespace CrewRedTestTask.Application.Interfaces;

public interface ITaxiTripImportService
{
	Task<ImportResult> RunAsync(CancellationToken cancellationToken = default);
}