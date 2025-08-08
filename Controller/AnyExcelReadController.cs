using ExcelReader.RyanW84.Abstractions.Data.DatabaseServices;
using ExcelReader.RyanW84.Abstractions.Data.TableCreators;
using ExcelReader.RyanW84.Abstractions.FileOperations.Readers;
using ExcelReader.RyanW84.Abstractions.Services;

namespace ExcelReader.RyanW84.Controller;

public class AnyExcelReadController(
    IExcelReaderDbContext dbContext,
    IAnyExcelReader anyExcelReader,
    IExcelTableCreator createTableFromExcel,
    INotificationService notificationService
) : DataImportControllerBase(dbContext, notificationService)
{
    private readonly IAnyExcelReader _anyExcelReader =
        anyExcelReader ?? throw new ArgumentNullException(nameof(anyExcelReader));
    private readonly IExcelTableCreator _createTableFromExcel =
        createTableFromExcel ?? throw new ArgumentNullException(nameof(createTableFromExcel));

    public async Task AddDynamicDataFromExcel()
    {
        await ExecuteOperationAsync(
            async () =>
            {
                NotificationService.ShowInfo("Starting dynamic Excel import...");
                var dataTable = await _anyExcelReader.ReadFromExcelAsync();
                _createTableFromExcel.CreateTableFromExcel(dataTable);
                await SaveChangesAsync();
                NotificationService.ShowSuccess("Dynamic Excel import complete.");
            },
            "Dynamic Excel import"
        );
    }
}
