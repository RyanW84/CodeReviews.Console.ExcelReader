using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExcelReader.RyanW84.Helpers;

using Microsoft.Extensions.Configuration;
using ExcelReader.RyanW84.Abstractions.Data.TableCreators;
using ExcelReader.RyanW84.Abstractions.Data.DatabaseServices;

namespace ExcelReader.RyanW84.Services;

public class WritePdfFormDataToDatabaseService(IConfiguration configuration , IPdfFormTableCreator createTableFromPdfForm) : IPdfFormDatabaseService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly IPdfFormTableCreator _createTableFromPdfForm = createTableFromPdfForm;

	public async Task WriteAsync(Dictionary<string, string> fieldValues)
    {
        // Use a specific table name for PDF form data with timestamp to ensure uniqueness
        var tableName = $"PdfFormData_{DateTime.Now:yyyyMMdd_HHmmss}";
        var dataTable = new DataTable(tableName);
        
        foreach (var key in fieldValues.Keys)
        {
            dataTable.Columns.Add(key);
        }
        var row = dataTable.NewRow();
        foreach (var kvp in fieldValues)
        {
            row[kvp.Key] = kvp.Value ?? string.Empty;
        }
        dataTable.Rows.Add(row);

        // Offload the database operation to a background thread
        await _createTableFromPdfForm.CreateTableFromPdfFormData(dataTable);
    }
}
