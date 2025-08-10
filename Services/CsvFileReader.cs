using ExcelReader.RyanW84.Abstractions.FileOperations.Readers;
using ExcelReader.RyanW84.Abstractions.Services;
using ExcelReader.RyanW84.Abstractions.Common;
using System.Data;

namespace ExcelReader.RyanW84.Services;

/// <summary>
/// Simplified CSV file reader focused exclusively on DataTable operations.
/// Eliminates unnecessary conversions and delegates all parsing to ICsvService.
/// </summary>
public class CsvFileReader(
    IFilePathService filePathService,
    INotificationService notificationService,
    ICsvService csvService
) : ICsvFileReader
{
    private readonly IFilePathService _filePathService =
        filePathService ?? throw new ArgumentNullException(nameof(filePathService));
    private readonly INotificationService _notificationService =
        notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    private readonly ICsvService _csvService =
        csvService ?? throw new ArgumentNullException(nameof(csvService));

    /// <summary>
    /// Reads CSV file using the configured default path and returns as DataTable.
    /// </summary>
    public async Task<DataTable> ReadCsvFileAsync()
    {
        var filePath = GetCsvFilePath();
        return await ReadCsvFileAsync(filePath);
    }

    /// <summary>
    /// Reads CSV file from a custom file path and returns as DataTable.
    /// </summary>
    public async Task<DataTable> ReadCsvFileAsync(string customFilePath)
    {
        try
        {
            if (string.IsNullOrEmpty(customFilePath))
            {
                _notificationService.ShowWarning("No CSV file path provided or selected.");
                return new DataTable();
            }

            _notificationService.ShowInfo($"Reading CSV file: {Path.GetFileName(customFilePath)}");

            // Direct delegation to ICsvService - no unnecessary conversions
            var dataTable = await _csvService.ReadCsvAsDataTableAsync(customFilePath, "CsvImport");

            _notificationService.ShowSuccess(
                $"Successfully loaded CSV: {dataTable.Rows.Count} rows, {dataTable.Columns.Count} columns"
            );

            return dataTable;
        }
        catch (Exception ex)
        {
            var errorMessage = $"Failed to read CSV file: {ex.Message}";
            _notificationService.ShowError(errorMessage);
            throw new InvalidOperationException(errorMessage, ex);
        }
    }

    /// <summary>
    /// Gets the CSV file path using the configured file path service.
    /// </summary>
    private string GetCsvFilePath()
    {
        const string defaultCsvPath =
            @"C:\Users\Ryanw\OneDrive\Documents\GitHub\CodeReviews.Console.ExcelReader\Data\ExcelCSV.csv";

        try
        {
            return _filePathService.GetFilePath(FileType.CSV, defaultCsvPath);
        }
        catch (Exception ex)
        {
            _notificationService.ShowWarning(
                $"Error getting CSV file path: {ex.Message}. Using default path."
            );
            return defaultCsvPath;
        }
    }
}
