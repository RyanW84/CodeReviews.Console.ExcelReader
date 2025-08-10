using System.Data;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using ExcelReader.RyanW84.Abstractions.Services;
using ExcelReader.RyanW84.Abstractions.Common;
using ExcelReader.RyanW84.Helpers;

namespace ExcelReader.RyanW84.Services;

/// <summary>
/// Simplified CSV service using CsvHelper for efficient CSV operations.
/// Follows Single Responsibility Principle by focusing only on reading CSV files as DataTable.
/// Uses existing file path service and validation for consistency.
/// </summary>
public class CsvService(
    INotificationService notificationService,
    IFilePathService filePathService,
    IFilePathValidation filePathValidation
) : ICsvService
{
    private readonly INotificationService _notificationService =
        notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    private readonly IFilePathService _filePathService =
        filePathService ?? throw new ArgumentNullException(nameof(filePathService));
    private readonly IFilePathValidation _filePathValidation = 
        filePathValidation ?? throw new ArgumentNullException(nameof(filePathValidation));

    private static readonly CsvConfiguration DefaultConfig = new(CultureInfo.InvariantCulture)
    {
        HasHeaderRecord = true,
        MissingFieldFound = null, // Ignore missing fields
        HeaderValidated = null, // Don't validate headers
        BadDataFound = null, // Handle bad data gracefully
        TrimOptions = TrimOptions.Trim,
        DetectDelimiter = true // Auto-detect delimiter
    };

    /// <summary>
    /// Reads CSV file and converts directly to DataTable using CsvHelper.
    /// Uses existing file path service for path resolution and validation.
    /// </summary>
    /// <param name="filePath">Path to the CSV file</param>
    /// <param name="tableName">Optional table name for the DataTable</param>
    /// <returns>DataTable containing CSV data</returns>
    public async Task<DataTable> ReadCsvAsDataTableAsync(string filePath, string? tableName = null)
    {
        try
        {
            // Validate the file path using existing validation service
            if (!_filePathValidation.ValidateFilePath(filePath))
            {
                throw new FilePathValidationException($"File validation failed for: {filePath}");
            }

            _notificationService.ShowInfo($"Reading CSV file: {Path.GetFileName(filePath)}");

            var dataTable = new DataTable(tableName ?? "CsvImport");

            // Use CsvHelper for efficient CSV reading
            using var reader = new StringReader(await File.ReadAllTextAsync(filePath));
            using var csv = new CsvReader(reader, DefaultConfig);

            // Read header and create columns
            if (await csv.ReadAsync())
            {
                csv.ReadHeader();
                if (csv.HeaderRecord != null)
                {
                    foreach (var header in csv.HeaderRecord)
                    {
                        var columnName = string.IsNullOrWhiteSpace(header) ? "Unknown" : header.Trim();
                        
                        // Ensure unique column names
                        if (dataTable.Columns.Contains(columnName))
                        {
                            var counter = 1;
                            var originalName = columnName;
                            while (dataTable.Columns.Contains(columnName))
                            {
                                columnName = $"{originalName}_{counter}";
                                counter++;
                            }
                        }
                        
                        dataTable.Columns.Add(columnName);
                    }
                }
            }

            // Read data rows using CsvHelper
            while (await csv.ReadAsync())
            {
                var row = dataTable.NewRow();
                
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    var fieldValue = csv.GetField(i);
                    row[i] = string.IsNullOrWhiteSpace(fieldValue) ? DBNull.Value : fieldValue.Trim();
                }
                
                dataTable.Rows.Add(row);
            }

            _notificationService.ShowSuccess(
                $"Successfully read CSV: {dataTable.Rows.Count} rows, {dataTable.Columns.Count} columns"
            );

            return dataTable;
        }
        catch (FilePathValidationException)
        {
            throw; // Re-throw validation exceptions
        }
        catch (Exception ex)
        {
            var fileName = Path.GetFileName(filePath);
            var errorMessage = $"Error reading {fileName}: {ex.Message}";
            _notificationService.ShowError(errorMessage);
            throw new InvalidOperationException(errorMessage, ex);
        }
    }
}
