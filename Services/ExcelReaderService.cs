using System.Data;

using ExcelReader.RyanW84.Abstractions.Common;
using ExcelReader.RyanW84.Abstractions.FileOperations.Readers;
using ExcelReader.RyanW84.Abstractions.Services;

using OfficeOpenXml;

namespace ExcelReader.RyanW84.Services;

/// <summary>
/// Excel file reader service implementing IExcelReader interface following SOLID principles
/// </summary>
public class ExcelReaderService(
    IFilePathService filePathService,
    INotificationService notificationService
) : IExcelReader
{
    private readonly IFilePathService _filePathService =
        filePathService ?? throw new ArgumentNullException(nameof(filePathService));

    private readonly INotificationService _notificationService =
        notificationService ?? throw new ArgumentNullException(nameof(notificationService));

    public async Task<DataTable> ReadAsync(string filePath)
    {
        try
        {
            _filePathService.ValidateFilePath(filePath, FileType.Excel);

            return await Task.Run(() =>
            {
                using var package = CreateExcelPackage(filePath);
                return ReadDataFromWorksheet(package);
            });
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Error reading Excel file: {ex.Message}");
            throw;
        }
    }

    public bool CanRead(string filePath)
    {
        return _filePathService.IsValidPath(filePath)
            && (
                Path.GetExtension(filePath).Equals(".xlsx", StringComparison.OrdinalIgnoreCase)
                || Path.GetExtension(filePath).Equals(".xls", StringComparison.OrdinalIgnoreCase)
            );
    }

	// In GetWorksheetsAsync, also replace the obsolete property:
	public async Task<IEnumerable<string>> GetWorksheetsAsync(string filePath)
	{
		try
		{
			_filePathService.ValidateFilePath(filePath , FileType.Excel);

			return await Task.Run(( ) =>
			{
				var worksheetNames = new List<string>();
				var file = new FileInfo(filePath);

				if (!file.Exists)
					throw new FileNotFoundException($"Excel file not found at path: {filePath}");

				ExcelPackage.License.SetNonCommercialPersonal("Ryan Weavers");

				using var package = new ExcelPackage(file);

				foreach (var worksheet in package.Workbook.Worksheets)
				{
					worksheetNames.Add(worksheet.Name);
				}

				return worksheetNames;
			});
		}
		catch (Exception ex)
		{
			_notificationService.ShowError($"Error getting worksheets: {ex.Message}");
			throw;
		}
	}

	// Remove all usages of the obsolete LicenseContext property.
	// EPPlus 8+ requires license acceptance via ExcelPackage.License.Accepted = true;
	// Set this once before using any ExcelPackage functionality, ideally at application startup.
	// If you must set it here, do so before creating any ExcelPackage instance.

	private static ExcelPackage CreateExcelPackage(string filePath)
	{
		var file = new FileInfo(filePath);

		if (!file.Exists)
			throw new FileNotFoundException($"Excel file not found at path: {filePath}");

		ExcelPackage.License.SetNonCommercialPersonal("Ryan Weavers"); // Accept the license as required by EPPlus 8+

		var package = new ExcelPackage(file);

		if (package.Workbook.Worksheets.Count == 0)
			throw new InvalidOperationException("No worksheets found in the Excel file");

		return package;
	}

    private static DataTable ReadDataFromWorksheet(
        ExcelPackage excelPackage,
        bool hasHeader = true,
        string? worksheetName = null
    )
    {
        // Get the first worksheet or the specified one
        var worksheet =
            (
                string.IsNullOrEmpty(worksheetName)
                    ? excelPackage.Workbook.Worksheets.FirstOrDefault()
                    : excelPackage.Workbook.Worksheets[worksheetName]
            )
            ?? throw new InvalidOperationException(
                $"Worksheet '{worksheetName ?? "default"}' not found in the Excel file"
            );
        if (worksheet.Dimension == null)
            return new DataTable(); // Return empty DataTable for empty worksheet

        var excelAsTable = new DataTable();

        // Add columns
        for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
        {
            var headerCell = worksheet.Cells[1, col];
            var columnName =
                hasHeader && !string.IsNullOrEmpty(headerCell.Text)
                    ? headerCell.Text
                    : $"Column{col}";
            excelAsTable.Columns.Add(columnName);
        }

        // Add data rows
        var startRow = hasHeader ? 2 : 1;
        for (var rowNum = startRow; rowNum <= worksheet.Dimension.End.Row; rowNum++)
        {
            var row = excelAsTable.NewRow();
            for (int col = 1; col <= excelAsTable.Columns.Count; col++)
            {
                var cellValue = worksheet.Cells[rowNum, col].Text;
                row[col - 1] = cellValue;
            }
            excelAsTable.Rows.Add(row);
        }

        return excelAsTable;
    }
}
