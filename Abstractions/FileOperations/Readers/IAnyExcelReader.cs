using System.Data;

namespace ExcelReader.RyanW84.Abstractions.FileOperations.Readers;

/// <summary>
/// Interface for any Excel reading operations
/// </summary>
public interface IAnyExcelReader
{
    // Original methods for backward compatibility
    Task<DataTable> ReadFromExcelAsync();
    DataTable ReadFromExcel();
    
    // New overloads that accept file path
    Task<DataTable> ReadFromExcelAsync(string filePath);
    DataTable ReadFromExcel(string filePath);
}
