using System.Data;
using ExcelReader.RyanW84.Models;

namespace ExcelReader.RyanW84.Abstractions.Services;

/// <summary>
/// Unified CSV service interface using CsvHelper for simplicity
/// </summary>
public interface ICsvService
{
    /// <summary>
    /// Reads CSV file as array of string arrays (raw data)
    /// </summary>
    Task<List<string[]>> ReadCsvAsArraysAsync(string filePath);
    
    /// <summary>
    /// Reads CSV file directly as DataTable
    /// </summary>
    Task<DataTable> ReadCsvAsDataTableAsync(string filePath, string? tableName = null);
    
    /// <summary>
    /// Reads CSV file as strongly-typed objects
    /// </summary>
    Task<List<T>> ReadCsvAsObjectsAsync<T>(string filePath);
    
    /// <summary>
    /// Writes DataTable to CSV file
    /// </summary>
    Task WriteCsvFromDataTableAsync(DataTable dataTable, string filePath);
    
    /// <summary>
    /// Gets CSV file metadata
    /// </summary>
    Task<CsvMetadata> GetMetadataAsync(string filePath);
}