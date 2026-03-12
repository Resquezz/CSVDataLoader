namespace CrewRedTestTask.Domain.Entities;

public readonly record struct ImportResult(int InsertedRows, int DuplicateRows, int InvalidRows, string DuplicatesFilePath);
