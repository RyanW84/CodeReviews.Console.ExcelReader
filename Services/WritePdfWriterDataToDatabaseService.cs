using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExcelReader.RyanW84.Helpers;

using Microsoft.Extensions.Configuration;
using ExcelReader.RyanW84.Abstractions.Data.TableCreators;
using ExcelReader.RyanW84.Abstractions.Data.DatabaseServices;

namespace ExcelReader.RyanW84.Services;

public class WritePdfWriterDataToDatabaseService(IConfiguration configuration , IPdfFormTableCreator createTableFromPdfWriter) : IPdfWriterDatabaseService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly IPdfFormTableCreator _createTableFromPdfWriter = createTableFromPdfWriter;

	public async Task WriteAsync(Dictionary<string, string> fieldValues)
    {
        // Use a specific table name for PDF writer data with timestamp to ensure uniqueness
        var tableName = $"PdfWriterData_{DateTime.Now:yyyyMMdd_HHmmss}";
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
        await _createTableFromPdfWriter.CreateTableFromPdfFormData(dataTable);
    }
}