using System.Data;
using OfficeOpenXml;

namespace ExcelReader.RyanW84.Services;

public static class ExcelBeginnerService
{
    public static ExcelPackage ExcelPackage()
    {
        const string filePath =
            @"C:\Users\Ryanw\OneDrive\Documents\GitHub\Excel-Reader\Data\ExcelBeginner.xlsx";
        var file = new FileInfo(filePath);

        // Check if the file exists
        if (!file.Exists)
            throw new FileNotFoundException($"Excel file not found at path: {filePath}");

        OfficeOpenXml.ExcelPackage.License.SetNonCommercialPersonal("Ryan Weavers");
        var package = new ExcelPackage(file);
        var worksheet = package.Workbook.Worksheets["test"];

        if (worksheet == null)
            throw new Exception("Worksheet 'test' not found in the Excel file");
        return (package);
    }

    public static DataTable ReadFromExcel(ExcelPackage excelPackage, bool hasHeader = true)
    {
        var worksheet = excelPackage.Workbook.Worksheets["test"];
        // Get the dimensions of the worksheet
        var rowCount = worksheet.Dimension.End.Row;
        var colCount = worksheet.Dimension.End.Column;

        var excelAsTable = new DataTable();
        foreach (var firstRowCell in worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column])
            //Get column details
            if (!string.IsNullOrEmpty(firstRowCell.Text))
            {
                var firstColumn = $"Column {firstRowCell.Start.Column}";
                excelAsTable.Columns.Add(hasHeader ? firstRowCell.Text : firstColumn);
            }

        var startRow = hasHeader ? 2 : 1;
        //Get row details
        for (var rowNum = startRow; rowNum <= worksheet.Dimension.End.Row; rowNum++)
        {
            var wsRow = worksheet.Cells[rowNum, 1, rowNum, excelAsTable.Columns.Count];
            var row = excelAsTable.Rows.Add();
            foreach (var cell in wsRow)
                row[cell.Start.Column - 1] = cell.Text;
        }

        return excelAsTable;
    }
}
