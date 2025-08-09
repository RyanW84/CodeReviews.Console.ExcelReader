using ExcelReader.RyanW84.Models;

using System.ComponentModel.DataAnnotations;

namespace ExcelReader.RyanW84.Abstractions.Services;

/// <summary>
/// Enhanced CSV metadata service leveraging existing validation infrastructure
/// </summary>
public interface ICsvMetadataService
{
    /// <summary>
    /// Gets metadata with built-in validation using existing validators
    /// </summary>
    Task<CsvMetadata> GetMetadataAsync(string filePath);
    
    /// <summary>
    /// Validates CSV structure using existing validation methods
    /// </summary>
    Task<ValidationResult> ValidateCsvStructureAsync(string filePath);
    
    /// <summary>
    /// Infers SQL data types for CSV columns using SqlDataTypeMapper
    /// </summary>
    Task<Dictionary<string, string>> InferSqlDataTypesAsync(string filePath);
    
    /// <summary>
    /// Validates CSV data quality using existing field validators
    /// </summary>
   
}