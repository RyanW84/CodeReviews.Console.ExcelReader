using System.Data;
using ExcelReader.RyanW84.Helpers;
using OfficeOpenXml;
using ExcelReader.RyanW84.Abstractions.Services;
using ExcelReader.RyanW84.Abstractions.FileOperations.Readers;
using ExcelReader.RyanW84.Abstractions.Common;

namespace ExcelReader.RyanW84.Services;

public class AnyExcelRead(IFilePathService filePathManager, INotificationService userNotifier) : IAnyExcelReader
{
    private readonly IFilePathService _filePathManager = filePathManager;
    private readonly INotificationService _userNotifier = userNotifier;

    // Original methods for backward compatibility
    public async Task<DataTable> ReadFromExcelAsync()
    {
        string filePath;
        try
        {
            var customDefault = @"C:\Users\Ryanw\OneDrive\Documents\GitHub\CodeReviews.Console.ExcelReader\Data\ExcelDynamic.Xlsx";
            filePath = _filePathManager.GetFilePath(FileType.Excel, customDefault);
        }
        catch (FilePathValidationException ex)
        {
            _userNotifier.ShowError($"Excel file path error: {ex.Message}");
            return new DataTable();
        }

        return await ReadFromExcelAsync(filePath);
    }

    public DataTable ReadFromExcel()
    {
        return ReadFromExcelAsync().GetAwaiter().GetResult();
    }

    // New method - uses provided file path
    public async Task<DataTable> ReadFromExcelAsync(string filePath)
    {
        try
        {
            _filePathManager.ValidateFilePath(filePath, FileType.Excel);
        }
        catch (FilePathValidationException ex)
        {
            _userNotifier.ShowError($"Excel file path error: {ex.Message}");
            return new DataTable();
        }

        return await Task.Run(() =>
        {
            ExcelPackage.License.SetNonCommercialPersonal("Ryan Weavers");

            using var package = new ExcelPackage(new FileInfo(filePath));
            ExcelWorksheet worksheet = package.Workbook.Worksheets[0]; // Get first worksheet

            var dataTable = new DataTable();

            // Add columns with type detection
            for (var columns = 1; columns <= worksheet.Dimension.Columns; columns++)
            {
                var readColumn = dataTable.Columns.Add(worksheet.Cells[1, columns].Text);
                var typeDetected = false;
                _userNotifier.ShowInfo($"Column added: {readColumn.ColumnName}");

                foreach (var cell in worksheet.Cells[2, columns, worksheet.Dimension.Rows, columns])
                    if (!string.IsNullOrEmpty(cell.Text))
                    {
                        if (DateTime.TryParse(cell.Text, out var dateValue))
                        {
                            readColumn.DataType = typeof(string); // Store as string in dd-MM-yyyy format
                            typeDetected = true;
                            break;
                        }

                        if (double.TryParse(cell.Text, out _))
                        {
                            readColumn.DataType = typeof(double);
                            typeDetected = true;
                            break;
                        }

                        if (int.TryParse(cell.Text, out _))
                        {
                            readColumn.DataType = typeof(int);
                            typeDetected = true;
                        }
                        else if (float.TryParse(cell.Text, out _))
                        {
                            readColumn.DataType = typeof(float);
                            typeDetected = true;
                        }
                        else if (bool.TryParse(cell.Text, out _))
                        {
                            readColumn.DataType = typeof(bool);
                            typeDetected = true;
                        }
                    }

                // If no other type was detected, set it to string
                if (typeDetected is not true)
                {
                    readColumn.DataType = typeof(string);
                }
            }

            // Populate DataTable with Excel data
            for (var row = 2; row <= worksheet.Dimension.Rows; row++)
            {
                var dataRow = dataTable.NewRow();
                for (var col = 1; col <= worksheet.Dimension.Columns; col++)
                {
                    var cellValue = worksheet.Cells[row, col].Text;
                    var colType = dataTable.Columns[col - 1].DataType;
                    if (colType == typeof(string) && DateTime.TryParse(cellValue, out var dateValue))
                    {
                        dataRow[col - 1] = dateValue.ToString("dd-MM-yyyy"); // Store as date-only string
                    }
                    else
                    {
                        dataRow[col - 1] = Convert.ChangeType(cellValue, colType);
                    }
                }

                dataTable.Rows.Add(dataRow);
            }

            dataTable.TableName = worksheet.Name;
            return dataTable;
        });
    }

  
    // New synchronous overload that accepts file path
    public DataTable ReadFromExcel(string filePath)
    {
        return ReadFromExcelAsync(filePath).GetAwaiter().GetResult();
    }
}
