using CrewRedTestTask.Domain.Entities;

namespace CrewRedTestTask.Domain.Interfaces;

public interface ITaxiTripRepository
{
	Task InitializeAsync(CancellationToken cancellationToken = default);
	Task<int> BulkInsertAsync(IReadOnlyCollection<TaxiTripRecord> records, CancellationToken cancellationToken = default);
}