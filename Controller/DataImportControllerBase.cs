using System.Data;
using ExcelReader.RyanW84.Abstractions.Data.DatabaseServices;
using ExcelReader.RyanW84.Abstractions.Services;

namespace ExcelReader.RyanW84.Controller;

public abstract class DataImportControllerBase(
	IExcelReaderDbContext dbContext ,
	INotificationService notificationService) : BaseController(notificationService)
{
    protected readonly IExcelReaderDbContext DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

	protected async Task<bool> SaveChangesAsync()
    {
        return await ExecuteOperationAsync(async () =>
        {
            await DbContext.SaveChangesAsync();
        }, "Save changes to database");
    }

    /// <summary>
    /// Generic domain model import workflow
    /// </summary>
    protected async Task<bool> ExecuteDomainImportAsync<TService, T>(
        TService service,
        string importType,
        Func<TService, DataTable?> readOperation,
        Func<DataTable, List<T>> convertOperation,
        Func<IExcelReaderDbContext, List<T>, Task> saveOperation) where T : class
    {
        return await ExecuteOperationAsync(async () =>
        {
            // 1. Read data
            var dataTable = readOperation(service);
            if (dataTable == null || dataTable.Rows.Count == 0)
            {
                NotificationService.ShowError($"No data found in the {importType} file.");
                return;
            }

            // 2. Convert to domain models
            var domainModels = convertOperation(dataTable);
            if (domainModels.Count == 0)
            {
                NotificationService.ShowError("No valid data rows found to import.");
                return;
            }

            // 3. Save to database
            await saveOperation(DbContext, domainModels);
            await DbContext.SaveChangesAsync();

            NotificationService.ShowSuccess($"{importType} import complete. Imported {domainModels.Count} records.");
        }, $"{importType} import");
    }

    /// <summary>
    /// Generic table creation import workflow
    /// </summary>
    protected async Task<bool> ExecuteTableImportAsync<TReader, TCreator>(
        TReader reader,
        TCreator tableCreator,
        string importType,
        Func<TReader, Task<DataTable>> readOperation,
        Func<TCreator, DataTable, Task> createTableOperation,
        string? customTableName = null)
    {
        return await ExecuteOperationAsync(async () =>
        {
            NotificationService.ShowInfo($"Starting {importType} import...");
            
            var dataTable = await readOperation(reader);
            if (dataTable == null || dataTable.Rows.Count == 0)
                throw new InvalidOperationException($"No data found in the {importType} file.");

            NotificationService.ShowInfo($"Read {dataTable.Rows.Count} rows and {dataTable.Columns.Count} columns from file.");

            if (!string.IsNullOrEmpty(customTableName))
                dataTable.TableName = customTableName;

            await createTableOperation(tableCreator, dataTable);
            await SaveChangesAsync();

            NotificationService.ShowSuccess($"{importType} import complete.");
        }, $"{importType} import");
    }
}