using System.Data;
using ExcelReader.RyanW84.Abstractions.Core;
using ExcelReader.RyanW84.Abstractions.Data.DatabaseServices;
using ExcelReader.RyanW84.Abstractions.Data.TableCreators;
using ExcelReader.RyanW84.Abstractions.FileOperations.Readers;
using ExcelReader.RyanW84.Abstractions.Services;

namespace ExcelReader.RyanW84.Controller;

/// <summary>
/// CSV controller following SOLID principles with clear separation of concerns.
/// Acts as an orchestrator for CSV operations, delegating specific tasks to appropriate services.
/// </summary>
public class CsvController(
    IExcelReaderDbContext dbContext,
    ICsvService csvService,
    ICsvTableCreator createTableFromCSV,
    INotificationService notificationService,
    IFilePathService filePathService,
    ICsvFileReader csvFileReader,
    IDataConverter<List<string[]>, DataTable> csvDataConverter
) : DataImportControllerBase(dbContext, notificationService)
{
    private readonly ICsvService _csvService = csvService ?? throw new ArgumentNullException(nameof(csvService));
    private readonly ICsvFileReader _csvFileReader = csvFileReader ?? throw new ArgumentNullException(nameof(csvFileReader));
    private readonly ICsvTableCreator _createTableFromCSV = createTableFromCSV ?? throw new ArgumentNullException(nameof(createTableFromCSV));
    private readonly IDataConverter<List<string[]>, DataTable> _csvDataConverter = csvDataConverter ?? throw new ArgumentNullException(nameof(csvDataConverter));
    private readonly IFilePathService _filePathService = filePathService ?? throw new ArgumentNullException(nameof(filePathService));

    /// <summary>
    /// Imports CSV data using the template method pattern from base class.
    /// This method demonstrates the Open/Closed principle - behavior is extended without modifying base functionality.
    /// </summary>
    public async Task AddDataFromCsv()
    {
        await ExecuteTableImportAsync(
            _csvFileReader,
            _createTableFromCSV,
            "CSV",
            ConvertCsvDataToDataTable,
            (creator, dataTable) => creator.CreateTableFromCsvDataAsync(dataTable),
            "CsvImport"
        );
    }

    /// <summary>
    /// Advanced CSV import with custom file path and validation.
    /// Demonstrates extensibility through composition rather than inheritance.
    /// </summary>
    public async Task ImportCsvWithValidation(string? customFilePath = null)
    {
        await ExecuteOperationAsync(async () =>
        {
            ValidateNotNullOrEmpty(customFilePath ?? "default", nameof(customFilePath));
            
            NotificationService.ShowInfo("Starting validated CSV import...");

            // Use ICsvService directly for more control
            var filePath = customFilePath ?? GetDefaultCsvPath();
            var dataTable = await _csvService.ReadCsvAsDataTableAsync(filePath, "CsvImport");
            
            // Validate data quality
            ValidateDataQuality(dataTable);

            // Create table and save
            await _createTableFromCSV.CreateTableFromCsvDataAsync(dataTable);
            await SaveChangesAsync();

            NotificationService.ShowSuccess($"CSV import completed with {dataTable.Rows.Count} records validated and imported.");
        }, "validated CSV import");
    }

    /// <summary>
    /// Batch CSV processing - demonstrates how the controller can orchestrate complex workflows.
    /// </summary>
    public async Task ProcessMultipleCsvFiles(string[] filePaths)
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
                    var dataTable = await _csvService.ReadCsvAsDataTableAsync(filePath);
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
    /// Converts raw CSV data to DataTable using the injected converter.
    /// This private method encapsulates the conversion logic and maintains single responsibility.
    /// </summary>
    private async Task<DataTable> ConvertCsvDataToDataTable(ICsvFileReader reader)
    {
        var csvData = await reader.ReadCsvFile();
        return await _csvDataConverter.ConvertAsync(csvData);
    }

    /// <summary>
    /// Validates data quality - demonstrates how business rules can be encapsulated.
    /// </summary>
    private void ValidateDataQuality(DataTable dataTable)
    {
        if (dataTable.Rows.Count == 0)
            throw new InvalidOperationException("CSV file contains no data rows.");

        if (dataTable.Columns.Count == 0)
            throw new InvalidOperationException("CSV file contains no columns.");

        // Additional business rule validations can be added here
        var emptyRows = dataTable.Rows.Cast<DataRow>()
            .Count(row => row.ItemArray.All(field => field == DBNull.Value || string.IsNullOrWhiteSpace(field?.ToString())));

        if (emptyRows > 0)
            NotificationService.ShowWarning($"Found {emptyRows} empty rows that will be skipped.");
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
