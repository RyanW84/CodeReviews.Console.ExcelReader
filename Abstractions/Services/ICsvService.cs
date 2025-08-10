using System.Data;

namespace ExcelReader.RyanW84.Abstractions.Services;

/// <summary>
/// Unified CSV service interface following Interface Segregation Principle.
/// Provides comprehensive CSV operations while maintaining focused responsibilities.
/// </summary>
public interface ICsvService
{
    /// <summary>
    /// Reads CSV file directly as DataTable.
    /// Optimized for database operations and data manipulation.
    /// </summary>
    /// <param name="filePath">Path to the CSV file</param>
    /// <param name="tableName">Optional table name for the DataTable</param>
    /// <returns>DataTable containing CSV data</returns>
    Task<DataTable> ReadCsvAsDataTableAsync(string filePath, string? tableName = null);
}
