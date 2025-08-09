using System.Data;
using ExcelReader.RyanW84.Helpers;
using ExcelReader.RyanW84.Services;
using ExcelReader.RyanW84.UserInterface;
using ExcelReader.RyanW84.Abstractions.Services;
using ExcelReader.RyanW84.Abstractions.FileOperations.Readers;
using ExcelReader.RyanW84.Abstractions.FileOperations.Writers;
using ExcelReader.RyanW84.Abstractions.Common;
using ExcelReader.RyanW84.Abstractions.Data.DatabaseServices;

namespace ExcelReader.RyanW84.Controller;

public class ExcelWriteController(
    IExcelWriteService writeToExcelService,
    IAnyExcelReader anyExcelRead,
    IFieldInputService fieldInputUi,
    IExcelDatabaseService writeUpdatedExcelDataToDatabase,
    INotificationService userNotifier,
    IFilePathService filePathManager
)
{
    private readonly IExcelWriteService _writeToExcelService = writeToExcelService;
    private readonly IAnyExcelReader _anyExcelRead = anyExcelRead;
    private readonly IFieldInputService _fieldInputUi = fieldInputUi;
    private readonly IExcelDatabaseService _writeUpdatedExcelDataToDatabase =
        writeUpdatedExcelDataToDatabase;
    private readonly INotificationService _userNotifier = userNotifier;
    private readonly IFilePathService _filePathManager = filePathManager;

    // Orchestrator method for all steps
    public async Task UpdateExcelAndDatabaseAsync()
    {
        try
        {
            // 1. Get file path from user via FilePathManager
            // Use a custom default path for Excel files
            var customDefault = @"C:\Users\Ryanw\OneDrive\Documents\GitHub\CodeReviews.Console.ExcelReader\Data\ExcelDynamic.Xlsx";
            var filePath = _filePathManager.GetFilePath(FileType.Excel,customDefault);

            // 2. Get existing field values from Excel using the obtained file path
            var table = await _anyExcelRead.ReadFromExcelAsync(filePath);
            if (table == null || table.Rows.Count == 0)
            {
                _userNotifier.ShowError("No data found in the Excel file.");
                return;
            }
            var existingFields = new Dictionary<string, string>();
            foreach (DataColumn col in table.Columns)
            {
                existingFields[col.ColumnName] =
                    table.Rows[0][col.ColumnName]?.ToString() ?? string.Empty;
            }

            // 3. Update field values interactively using unified UI
            // Async usage:
            var updatedFields = await _fieldInputUi.GatherUpdatedFieldsAsync(existingFields, FileType.Excel);

            // 4. Write updated fields to Excel
            _writeToExcelService.WriteFieldsToExcel(filePath, updatedFields);

            // 5. Write updated fields to database
            await _writeUpdatedExcelDataToDatabase.WriteAsync(updatedFields);

            _userNotifier.ShowSuccess("Excel file and database updated successfully!");
        }
        catch (FilePathValidationException ex)
        {
            _userNotifier.ShowError($"File path error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _userNotifier.ShowError($"An unexpected error occurred: {ex.Message}");
        }
    }

    // Keep synchronous version for backward compatibility
    public void UpdateExcelAndDatabase()
    {
        UpdateExcelAndDatabaseAsync().GetAwaiter().GetResult();
    }
}
