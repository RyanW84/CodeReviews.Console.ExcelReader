namespace ExcelReader.RyanW84.Abstractions.FileOperations.Readers;

/// <summary>
/// Interface for CSV file reading operations
/// </summary>
public interface ICsvFileReader
{
    Task<List<string[]>> ReadCsvFile();
}