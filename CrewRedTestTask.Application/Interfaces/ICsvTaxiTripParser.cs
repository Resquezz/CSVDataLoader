using CrewRedTestTask.Domain.Entities;
using Microsoft.VisualBasic.FileIO;

namespace CrewRedTestTask.Application.Interfaces;

public interface ICsvTaxiTripParser
{
	TextFieldParser CreateParser(string inputPath);
	bool HasExpectedHeader(string[]? header);
	bool TryParse(string[] rawFields, out TaxiTripRecord record);
}