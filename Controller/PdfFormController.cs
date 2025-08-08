// Ignore Spelling: Pdf

using ExcelReader.RyanW84.Abstractions.Common;
using ExcelReader.RyanW84.Abstractions.Data.DatabaseServices;
using ExcelReader.RyanW84.Abstractions.FileOperations.Readers;
using ExcelReader.RyanW84.Abstractions.Services;
using ExcelReader.RyanW84.Helpers;

namespace ExcelReader.RyanW84.Controller;

/// <summary>
/// Controller for importing PDF form data (read-only operations)
/// </summary>
public class PdfFormController(
	IPdfFormReader readFromPdfForm ,
	IPdfFormDatabaseService writePdfFormDataToDatabaseService ,
	IFilePathService filePathManager ,
	IExcelReaderDbContext dbContext ,
	INotificationService notificationService) : DataImportControllerBase(dbContext, notificationService)
{
    private readonly IPdfFormReader _readFromPdfForm = readFromPdfForm ?? throw new ArgumentNullException(nameof(readFromPdfForm));
    private readonly IPdfFormDatabaseService _writePdfFormDataToDatabaseService = writePdfFormDataToDatabaseService ?? throw new ArgumentNullException(nameof(writePdfFormDataToDatabaseService));
    private readonly IFilePathService _filePathManager = filePathManager ?? throw new ArgumentNullException(nameof(filePathManager));

	/// <summary>
	/// Imports PDF form data to database (read-only operation)
	/// </summary>
	public async Task ImportDataFromPdfForm()
    {
        await ExecuteOperationAsync(async () =>
        {
            // 1. Get the file path using FilePathManager
            var customDefault = @"C:\Users\Ryanw\OneDrive\Documents\GitHub\Excel-Reader\Data\FillablePDF.pdf";
            var filePath = _filePathManager.GetFilePath(FileType.PDF, customDefault);

            // 2. Read existing fields from PDF (read-only)
            NotificationService.ShowInfo("Reading PDF form fields...");
            var fields = await _readFromPdfForm.ReadFormFieldsAsync(filePath);
            
            if (fields.Count == 0)
            {
                NotificationService.ShowError("No form fields found or file not found.");
                return;
            }

            NotificationService.ShowInfo($"Found {fields.Count} form fields in PDF.");

            // 3. Import data directly to database without modification
            NotificationService.ShowInfo("Importing PDF form data to database...");
            await _writePdfFormDataToDatabaseService.WriteAsync(fields);
            await SaveChangesAsync();

            NotificationService.ShowSuccess($"PDF form data imported successfully! Imported {fields.Count} fields.");
        }, "PDF form import");
    }
}
