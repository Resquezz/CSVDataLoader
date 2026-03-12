namespace CrewRedTestTask.Application.Interfaces;

public interface IDuplicateTripWriterFactory
{
	Task<IDuplicateTripWriter> CreateAsync(CancellationToken cancellationToken = default);
}

public interface IDuplicateTripWriter : IAsyncDisposable
{
	string OutputPath { get; }
	Task WriteAsync(string[] values, CancellationToken cancellationToken = default);
}