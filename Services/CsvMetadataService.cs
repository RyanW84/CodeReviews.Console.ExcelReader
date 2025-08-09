using ExcelReader.RyanW84.Abstractions.Common;
using ExcelReader.RyanW84.Abstractions.Services;
using ExcelReader.RyanW84.Models;
using ExcelReader.RyanW84.Helpers;
using System.ComponentModel.DataAnnotations;

namespace ExcelReader.RyanW84.Services;

/// <summary>
/// Handles CSV metadata extraction.
/// Follows Single Responsibility Principle.
/// </summary>
public class CsvMetadataService(ICsvParser csvParser, INotificationService notificationService) : ICsvMetadataService
{
    private readonly ICsvParser _csvParser = csvParser ?? throw new ArgumentNullException(nameof(csvParser));
    private readonly INotificationService _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));

    public async Task<CsvMetadata> GetMetadataAsync(string filePath)
    {
        try
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(stream);

            var headerLine = await reader.ReadLineAsync();
            if (string.IsNullOrEmpty(headerLine))
                return new CsvMetadata();

            var headers = _csvParser.ParseLine(headerLine);
            var rowCount = 0;

            while (await reader.ReadLineAsync() != null)
                rowCount++;

            var fileInfo = new FileInfo(filePath);
            var metadata = new CsvMetadata
            {
                FileName = fileInfo.Name,
                FilePath = filePath,
                FileSize = fileInfo.Length,
                ColumnCount = headers.Length,
                DataRowCount = rowCount,
                Headers = headers,
                LastModified = fileInfo.LastWriteTime,
                AnalyzedAt = DateTime.UtcNow
            };

            // Basic validation
            if (rowCount == 0)
                metadata.ValidationErrors.Add("No data rows found");

            if (headers.Length == 0)
                metadata.ValidationErrors.Add("No columns found");

            if (headers.Any(string.IsNullOrWhiteSpace))
                metadata.ValidationWarnings.Add("Some column headers are empty");

            return metadata;
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Error reading CSV metadata: {ex.Message}");
            return new CsvMetadata();
        }
    }

    public async Task<ValidationResult> ValidateCsvStructureAsync(string filePath)
    {
        try
        {
            var metadata = await GetMetadataAsync(filePath);

            if (metadata.ValidationErrors.Count != 0)
            {
                return new ValidationResult(string.Join("; ", metadata.ValidationErrors));
            }

            if (metadata.ValidationWarnings.Count != 0)
            {
                _notificationService.ShowWarning($"CSV validation warnings: {string.Join("; ", metadata.ValidationWarnings)}");
            }

            _notificationService.ShowSuccess("CSV structure validation passed");
            return ValidationResult.Success;
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error validating CSV structure: {ex.Message}";
            _notificationService.ShowError(errorMessage);
            return new ValidationResult(errorMessage);
        }
    }

    public async Task<Dictionary<string, string>> InferSqlDataTypesAsync(string filePath)
    {
        try
        {
            var metadata = await GetMetadataAsync(filePath);
            var dataTypes = new Dictionary<string, string>();

            // Read a sample of rows to infer data types
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(stream);

            // Skip header
            await reader.ReadLineAsync();

            var sampleRows = new List<string[]>();
            var sampleCount = Math.Min(100, metadata.DataRowCount); // Sample first 100 rows or all rows if less

            for (int i = 0; i < sampleCount; i++)
            {
                var line = await reader.ReadLineAsync();
                if (line == null) break;
                sampleRows.Add(_csvParser.ParseLine(line));
            }

            // Analyze each column
            for (int columnIndex = 0; columnIndex < metadata.Headers.Length; columnIndex++)
            {
                var columnName = SqlDataTypeMapper.SanitizeColumnName(metadata.Headers[columnIndex]);
                var inferredType = InferColumnDataType(sampleRows, columnIndex);
                dataTypes[columnName] = SqlDataTypeMapper.GetSqlDataType(inferredType);
            }

            _notificationService.ShowSuccess($"Inferred SQL data types for {dataTypes.Count} columns");
            return dataTypes;
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Error inferring SQL data types: {ex.Message}");
            return [];
        }
    }

    private static Type InferColumnDataType(List<string[]> sampleRows, int columnIndex)
    {
        var nonEmptyValues = sampleRows
            .Where(row => columnIndex < row.Length)
            .Select(row => row[columnIndex])
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToList();

        if (nonEmptyValues.Count == 0)
            return typeof(string);

        // Try to infer type based on sample values
        var allIntegers = nonEmptyValues.All(value => int.TryParse(value, out _));
        if (allIntegers)
            return typeof(int);

        var allLongs = nonEmptyValues.All(value => long.TryParse(value, out _));
        if (allLongs)
            return typeof(long);

        var allDecimals = nonEmptyValues.All(value => decimal.TryParse(value, out _));
        if (allDecimals)
            return typeof(decimal);

        var allDoubles = nonEmptyValues.All(value => double.TryParse(value, out _));
        if (allDoubles)
            return typeof(double);

        var allBooleans = nonEmptyValues.All(value => bool.TryParse(value, out _));
        if (allBooleans)
            return typeof(bool);

        var allDates = nonEmptyValues.All(value => DateTime.TryParse(value, out _));
        if (allDates)
            return typeof(DateTime);

        var allGuids = nonEmptyValues.All(value => Guid.TryParse(value, out _));
        if (allGuids)
            return typeof(Guid);

        // Default to string
        return typeof(string);
    }
}
