using System.Data;
using ExcelReader.RyanW84.Abstractions.Core;
using ExcelReader.RyanW84.Abstractions.Services;
using ExcelReader.RyanW84.Helpers;

namespace ExcelReader.RyanW84.Services;

/// <summary>
/// Converts CSV data to DataTable.
/// Follows Single Responsibility Principle.
/// </summary>
public class CsvToDataTableConverter(INotificationService notificationService) : IDataConverter<List<string[]>, DataTable>
{
    private readonly INotificationService _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    private const string DefaultTableName = "CsvImport";

	public DataTable Convert(List<string[]> csvData)
    {
        var dataTable = new DataTable(DefaultTableName);

        try
        {
            if (!IsValidCsvData(csvData))
            {
                _notificationService.ShowWarning("Invalid CSV data provided");
                return dataTable;
            }

            BuildColumns(dataTable, csvData[0]);
            PopulateRows(dataTable, csvData.Skip(1));

            _notificationService.ShowInfo($"Converted CSV to DataTable: {dataTable.Rows.Count} rows, {dataTable.Columns.Count} columns");
            return dataTable;
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Error converting CSV data: {ex.Message}");
            throw new InvalidOperationException("CSV to DataTable conversion failed", ex);
        }
    }

    public async Task<DataTable> ConvertAsync(List<string[]> csvData)
    {
        return await Task.Run(() => Convert(csvData));
    }

    private static bool IsValidCsvData(List<string[]> csvData)
    {
        return csvData != null && csvData.Count >= 2;
    }

    private void BuildColumns(DataTable dataTable, string[] headers)
    {
        var columnNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var header in headers)
        {
            var cleanName = SqlDataTypeMapper.SanitizeColumnName(
                string.IsNullOrWhiteSpace(header) ? "Unknown_Column" : header.Trim());

            var uniqueName = EnsureUniqueColumnName(cleanName, columnNames);
            columnNames.Add(uniqueName);
            dataTable.Columns.Add(uniqueName);
        }
    }

    private static string EnsureUniqueColumnName(string baseName, HashSet<string> existingNames)
    {
        var uniqueName = baseName;
        var counter = 1;
        
        while (existingNames.Contains(uniqueName))
        {
            uniqueName = $"{baseName}_{counter++}";
        }
        
        return uniqueName;
    }

    private static void PopulateRows(DataTable dataTable, IEnumerable<string[]> dataRows)
    {
        foreach (var sourceRow in dataRows)
        {
            var rowData = new object[dataTable.Columns.Count];
            
            for (int j = 0; j < dataTable.Columns.Count; j++)
            {
                if (j < sourceRow.Length)
                {
                    var cellValue = sourceRow[j]?.Trim() ?? string.Empty;
                    rowData[j] = string.IsNullOrEmpty(cellValue) ? DBNull.Value : cellValue;
                }
                else
                {
                    rowData[j] = DBNull.Value;
                }
            }
            
            dataTable.Rows.Add(rowData);
        }
    }
}