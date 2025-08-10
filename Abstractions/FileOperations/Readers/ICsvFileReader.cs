using System.Data;

namespace ExcelReader.RyanW84.Abstractions.FileOperations.Readers;

/// <summary>
/// Simplified CSV file reader interface focused exclusively on DataTable operations.
/// Eliminates unnecessary data type conversions and follows Single Responsibility Principle.
/// </summary>
public interface ICsvFileReader
{
    /// <summary>
    /// Reads CSV file using the configured default path and returns as DataTable.
    /// </summary>
    /// <returns>DataTable containing CSV data</returns>
    Task<DataTable> ReadCsvFileAsync();

    /// <summary>
    /// Reads CSV file from a custom file path and returns as DataTable.
    /// </summary>
    /// <param name="customFilePath">Custom path to the CSV file</param>
    /// <returns>DataTable containing CSV data</returns>
    Task<DataTable> ReadCsvFileAsync(string customFilePath);
}
