using ExcelReader.RyanW84.Data;
using ExcelReader.RyanW84.Services;
using ExcelReader.RyanW84.Helpers;
using ExcelReader.RyanW84.Abstractions.Services;
using ExcelReader.RyanW84.Abstractions.FileOperations.Readers;
using ExcelReader.RyanW84.Abstractions.Data.TableCreators;
using ExcelReader.RyanW84.Abstractions.Data.DatabaseServices;

namespace ExcelReader.RyanW84.Controller;

public class PdfTableController(
	IExcelReaderDbContext dbContext ,
	IPdfTableReader readFromPdf ,
	ICsvTableCreator createTableFromCSV ,
	INotificationService notificationService) : DataImportControllerBase(dbContext, notificationService)
{
    private readonly IPdfTableReader _readFromPdf = readFromPdf ?? throw new ArgumentNullException(nameof(readFromPdf));
    private readonly ICsvTableCreator _createTableFromCSV = createTableFromCSV ?? throw new ArgumentNullException(nameof(createTableFromCSV));

	public async Task AddDataFromPdf()
    {
        await ExecuteOperationAsync(async () =>
        {
            NotificationService.ShowInfo("Starting PDF import...");
            var pdfData = await _readFromPdf.ReadPdfFileAsync();
            var dataTable = await _readFromPdf.ConvertToDataTableAsync(pdfData);
            NotificationService.ShowInfo($"Read {dataTable.Rows.Count} Rows from PDF file.");
            NotificationService.ShowInfo($"Read {dataTable.Columns.Count} Columns from PDF file.");

            dataTable.TableName = "PdfImport";
            await _createTableFromCSV.CreateTableFromCsvDataAsync(dataTable);
            await SaveChangesAsync();
            NotificationService.ShowSuccess("PDF import complete.");
        }, "PDF import");
    }
}
