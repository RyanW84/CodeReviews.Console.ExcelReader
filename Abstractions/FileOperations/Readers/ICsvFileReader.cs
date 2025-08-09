using System.Data;

namespace ExcelReader.RyanW84.Abstractions.FileOperations.Readers;

/// <summary>
/// Interface for CSV file reading operations following Interface Segregation Principle.
/// Provides focused methods for CSV file reading while maintaining extensibility.
/// </summary>
public interface ICsvFileReader
{
    /// <summary>
    /// Reads CSV file using the configured default path and returns raw string arrays.
    /// </summary>
    /// <returns>List of string arrays representing CSV rows</returns>
    Task<List<string[]>> ReadCsvFile();

    /// <summary>
    /// Reads CSV file from a custom file path and returns raw string arrays.
    /// Extends functionality without modifying the base interface contract.
    /// </summary>
    /// <param name="customFilePath">Custom path to the CSV file</param>
    /// <returns>List of string arrays representing CSV rows</returns>
    Task<List<string[]>> ReadCsvFileAsync(string customFilePath);

    /// <summary>
    /// Reads CSV file and returns as DataTable for immediate database operations.
    /// Convenience method for common data processing scenarios.
    /// </summary>
    /// <param name="customFilePath">Optional custom file path; uses default if not provided</param>
    /// <returns>DataTable containing CSV data</returns>
    Task<DataTable> ReadCsvAsDataTableAsync(string? customFilePath = null);
}