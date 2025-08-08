using System.Data;

using ExcelReader.RyanW84.Abstractions.Data.TableCreators;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ExcelReader.RyanW84.Helpers;

public class CreateTableFromPdfForm : IPdfFormTableCreator
{
    private readonly IConfiguration _configuration;

    public CreateTableFromPdfForm(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task CreateTableFromPdfFormData(DataTable dataTable)
    {
        // Create SQL table and insert data
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        // Build CREATE TABLE statement
        var columnDefs = new List<string>();
        foreach (DataColumn col in dataTable.Columns)
        {
            columnDefs.Add($"[{col.ColumnName}] NVARCHAR(MAX)");
        }
        var createTableSql =
            $"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{dataTable.TableName}') " +
            $"CREATE TABLE [{dataTable.TableName}] ({string.Join(", " , columnDefs)})";

        using (var command = new SqlCommand(createTableSql , connection))
        {
            await command.ExecuteNonQueryAsync();
        }

        // Bulk copy the data
        using var bulkCopy = new SqlBulkCopy(connection);
        bulkCopy.DestinationTableName = dataTable.TableName;
        await bulkCopy.WriteToServerAsync(dataTable);
    }

    public void CreateTableFromPdfFormDataSync(DataTable dataTable)
    {
        // Synchronous version that calls the async method and waits for the result
        CreateTableFromPdfFormData(dataTable).GetAwaiter().GetResult();
    }
}
