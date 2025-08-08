using System.Data;

using ExcelReader.RyanW84.Abstractions.Base;

namespace ExcelReader.RyanW84.Abstractions.FileOperations.Readers;

/// <summary>
/// Interface for Excel file reading operations
/// </summary>
public interface IExcelReader : IFileReader<DataTable>
{
    /// <summary>
    /// Gets available worksheet names in the Excel file
    /// </summary>
    /// <param name="filePath">Path to the Excel file</param>
    /// <returns>List of worksheet names</returns>
    Task<IEnumerable<string>> GetWorksheetsAsync(string filePath);
}