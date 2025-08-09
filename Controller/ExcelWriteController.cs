using System.Data;
using ExcelReader.RyanW84.Helpers;
using ExcelReader.RyanW84.Services;
using ExcelReader.RyanW84.UserInterface;
using ExcelReader.RyanW84.Abstractions.Services;
using ExcelReader.RyanW84.Abstractions.FileOperations.Readers;
using ExcelReader.RyanW84.Abstractions.FileOperations.Writers;
using ExcelReader.RyanW84.Abstractions.Common;
using ExcelReader.RyanW84.Abstractions.Core;
using ExcelReader.RyanW84.Abstractions.Data.DatabaseServices;

namespace ExcelReader.RyanW84.Controller;

public class ExcelWriteController(
    IExcelWriteService writeToExcelService,
    IAnyExcelReader anyExcelRead,
    IFieldInputService fieldInputUi,
    IExcelDatabaseService writeUpdatedExcelDataToDatabase,
    IExcelUpdateService excelUpdateService,
    IRecordSelectionService recordSelectionService,
    INotificationService userNotifier,
    IFilePathService filePathManager
)
{
    private readonly IExcelWriteService _writeToExcelService = writeToExcelService;
    private readonly IAnyExcelReader _anyExcelRead = anyExcelRead;
    private readonly IFieldInputService _fieldInputUi = fieldInputUi;
    private readonly IExcelDatabaseService _writeUpdatedExcelDataToDatabase = writeUpdatedExcelDataToDatabase;
    private readonly IExcelUpdateService _excelUpdateService = excelUpdateService;
    private readonly IRecordSelectionService _recordSelectionService = recordSelectionService;
    private readonly INotificationService _userNotifier = userNotifier;
    private readonly IFilePathService _filePathManager = filePathManager;

    // Orchestrator method for all steps
    public async Task UpdateExcelAndDatabaseAsync()
    {
        try
        {
            // 1. Get file path from user via FilePathManager
            // Use a custom default path for Excel files
            var customDefault = @"C:\Users\Ryanw\OneDrive\Documents\GitHub\Excel-Reader\Data\ExcelDynamic.xlsx";
            var filePath = _filePathManager.GetFilePath(FileType.Excel, customDefault);

            // 2. Get existing data from Excel using the obtained file path
            var table = await _anyExcelRead.ReadFromExcelAsync(filePath);
            if (table == null || table.Rows.Count == 0)
            {
                _userNotifier.ShowError("No data found in the Excel file.");
                return;
            }

            // 3. Ask user to select which record to update
            var selectedRowIndex = await _recordSelectionService.SelectRecordForUpdateAsync(table);
            if (selectedRowIndex == -1)
            {
                _userNotifier.ShowInfo("Update operation cancelled by user.");
                return;
            }

            // 4. Get existing field values from the selected row
            var existingFields = new Dictionary<string, string>();
            var selectedRow = table.Rows[selectedRowIndex];
            foreach (DataColumn col in table.Columns)
            {
                existingFields[col.ColumnName] = selectedRow[col.ColumnName]?.ToString() ?? string.Empty;
            }

            // 5. Update field values interactively using unified UI
            var updatedFields = await _fieldInputUi.GatherUpdatedFieldsAsync(existingFields, FileType.Excel);

            // 6. Update the Excel file (update the specific row)
            await UpdateExcelFileAsync(filePath, table, selectedRowIndex, updatedFields);

            // 7. Update the database record using the update service
            await _excelUpdateService.UpdateRecordAsync(updatedFields, table, selectedRowIndex);

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

    private async Task UpdateExcelFileAsync(string filePath, DataTable table, int rowIndex, Dictionary<string, string> updatedFields)
    {
        // Update the DataTable with new values
        var rowToUpdate = table.Rows[rowIndex];
        foreach (var kvp in updatedFields)
        {
            if (table.Columns.Contains(kvp.Key))
            {
                rowToUpdate[kvp.Key] = kvp.Value ?? string.Empty;
            }
        }

        // Since IExcelWriteService only has WriteFieldsToExcel method, 
        // we need to create a simpler approach or extend the interface
        // For now, let's write the updated fields as a simple update
        await Task.Run(() => _writeToExcelService.WriteFieldsToExcel(filePath, updatedFields));
    }

    // Keep synchronous version for backward compatibility
    public void UpdateExcelAndDatabase()
    {
        UpdateExcelAndDatabaseAsync().GetAwaiter().GetResult();
    }
}
