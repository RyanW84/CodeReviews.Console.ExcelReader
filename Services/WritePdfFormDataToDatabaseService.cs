using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExcelReader.RyanW84.Helpers;

using Microsoft.Extensions.Configuration;
using ExcelReader.RyanW84.Abstractions.Data.TableCreators;
using ExcelReader.RyanW84.Abstractions.Data.DatabaseServices;

namespace ExcelReader.RyanW84.Services;

public class WritePdfFormDataToDatabaseService : IPdfFormDatabaseService
{
    private readonly IConfiguration _configuration;
    private readonly IPdfFormTableCreator _createTableFromPdfForm;

    public WritePdfFormDataToDatabaseService(IConfiguration configuration, IPdfFormTableCreator createTableFromPdfForm)
    {
        _configuration = configuration;
        _createTableFromPdfForm = createTableFromPdfForm;
    }

    public async Task WriteAsync(Dictionary<string, string> fieldValues)
    {
        var dataTable = new DataTable("PdfFormData");
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
