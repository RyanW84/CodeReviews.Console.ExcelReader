using System.Data;
using ExcelReader.RyanW84.Abstractions.Core;
using ExcelReader.RyanW84.Abstractions.Data.DatabaseServices;
using ExcelReader.RyanW84.Abstractions.Data.TableCreators;
using ExcelReader.RyanW84.Abstractions.FileOperations.Readers;
using ExcelReader.RyanW84.Abstractions.Services;
using ExcelReader.RyanW84.Helpers;

namespace ExcelReader.RyanW84.Controller;

/// <summary>
/// Simplified CSV controller focused exclusively on DataTable operations.
/// Removes unnecessary data converters and complex workflows.
/// </summary>
public class CsvController(
    IExcelReaderDbContext dbContext,
    ICsvService csvService,
    ICsvTableCreator createTableFromCSV,
    INotificationService notificationService,
    IFilePathService filePathService,
    ICsvFileReader csvFileReader
) : DataImportControllerBase(dbContext, notificationService)
{
    private readonly ICsvService _csvService = csvService ?? throw new ArgumentNullException(nameof(csvService));
    private readonly ICsvFileReader _csvFileReader = csvFileReader ?? throw new ArgumentNullException(nameof(csvFileReader));
    private readonly ICsvTableCreator _createTableFromCSV = createTableFromCSV ?? throw new ArgumentNullException(nameof(createTableFromCSV));
    private readonly IFilePathService _filePathService = filePathService ?? throw new ArgumentNullException(nameof(filePathService));

    /// <summary>
    /// Simple CSV import using DataTable exclusively.
    /// </summary>
    public async Task ImportCsvAsync()
    {
        await ExecuteOperationAsync(async () =>
        {
            NotificationService.ShowInfo("Starting CSV import...");

            // Direct DataTable reading - no conversions needed
            var dataTable = await _csvFileReader.ReadCsvFileAsync();
            
            if (dataTable.Rows.Count == 0)
            {
                NotificationService.ShowWarning("CSV file contains no data to import.");
                return;
            }

            // Validate data quality
            ValidateDataQuality(dataTable);

            // Create table and save
            await _createTableFromCSV.CreateTableFromCsvDataAsync(dataTable);
            await SaveChangesAsync();

            NotificationService.ShowSuccess($"CSV import completed: {dataTable.Rows.Count} records imported.");
        }, "CSV import");
    }

    /// <summary>
    /// CSV import with custom file path and validation.
    /// </summary>
    public async Task ImportCsvAsync(string filePath)
    {
        await ExecuteOperationAsync(async () =>
        {
            ValidateNotNullOrEmpty(filePath, nameof(filePath));
            
            NotificationService.ShowInfo("Starting validated CSV import...");

            // Direct DataTable reading with custom path
            var dataTable = await _csvFileReader.ReadCsvFileAsync(filePath);
            
            if (dataTable.Rows.Count == 0)
            {
                NotificationService.ShowWarning("CSV file contains no data to import.");
                return;
            }

            // Validate data quality
            ValidateDataQuality(dataTable);

            // Set appropriate table name
            dataTable.TableName = $"CsvImport_{Path.GetFileNameWithoutExtension(filePath)}";

            // Create table and save
            await _createTableFromCSV.CreateTableFromCsvDataAsync(dataTable);
            await SaveChangesAsync();

            NotificationService.ShowSuccess($"CSV import completed: {dataTable.Rows.Count} records validated and imported.");
        }, "validated CSV import");
    }

    /// <summary>
    /// Batch CSV processing using DataTable exclusively.
    /// </summary>
    public async Task ProcessMultipleCsvFilesAsync(string[] filePaths)
    {
        ValidateNotNull(filePaths, nameof(filePaths));

        await ExecuteOperationAsync(async () =>
        {
            var successCount = 0;
            var totalFiles = filePaths.Length;

            NotificationService.ShowInfo($"Starting batch processing of {totalFiles} CSV files...");

            foreach (var filePath in filePaths)
            {
                try
                {
                    var dataTable = await _csvFileReader.ReadCsvFileAsync(filePath);
                    if (dataTable.Rows.Count > 0)
                    {
                        dataTable.TableName = $"CsvImport_{Path.GetFileNameWithoutExtension(filePath)}";
                        await _createTableFromCSV.CreateTableFromCsvDataAsync(dataTable);
                        successCount++;
                    }
                }
                catch (Exception ex)
                {
                    NotificationService.ShowWarning($"Failed to process {filePath}: {ex.Message}");
                }
            }

            await SaveChangesAsync();
            NotificationService.ShowSuccess($"Batch processing complete: {successCount}/{totalFiles} files processed successfully.");
        }, "batch CSV processing");
    }

    /// <summary>
    /// Validates DataTable data quality.
    /// </summary>
    private void ValidateDataQuality(DataTable dataTable)
    {
        if (dataTable.Rows.Count == 0)
            throw new InvalidOperationException("CSV file contains no data rows.");

        if (dataTable.Columns.Count == 0)
            throw new InvalidOperationException("CSV file contains no columns.");

        // Count empty rows
        var emptyRows = dataTable.Rows.Cast<DataRow>()
            .Count(row => row.ItemArray.All(field => field == DBNull.Value || string.IsNullOrWhiteSpace(field?.ToString())));

        if (emptyRows > 0)
            NotificationService.ShowWarning($"Found {emptyRows} empty rows that will be processed.");
    }

    /// <summary>
    /// Gets the default CSV file path using the file path service.
    /// </summary>
    private string GetDefaultCsvPath()
    {
        return _filePathService.GetFilePath(Abstractions.Common.FileType.CSV, 
            Path.Combine("Data", "ExcelCSV.csv"));
    }
}
