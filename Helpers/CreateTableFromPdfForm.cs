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

        // Build CREATE TABLE statement with proper sanitization
        var columnDefs = new List<string>();
        foreach (DataColumn col in dataTable.Columns)
        {
            var sanitizedColumnName = SqlDataTypeMapper.SanitizeColumnName(col.ColumnName);
            columnDefs.Add($"[{sanitizedColumnName}] NVARCHAR(MAX)");
        }

        var sanitizedTableName = SqlDataTypeMapper.SanitizeTableName(dataTable.TableName);
        var createTableSql =
            $"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{sanitizedTableName}') " +
            $"CREATE TABLE [{sanitizedTableName}] ({string.Join(", ", columnDefs)})";

        using (var command = new SqlCommand(createTableSql, connection))
        {
            await command.ExecuteNonQueryAsync();
        }

        // Bulk copy the data
        using var bulkCopy = new SqlBulkCopy(connection);
        bulkCopy.DestinationTableName = sanitizedTableName;
        await bulkCopy.WriteToServerAsync(dataTable);
    }

    public void CreateTableFromPdfFormDataSync(DataTable dataTable)
    {
        // Synchronous version that calls the async method and waits for the result
        CreateTableFromPdfFormData(dataTable).GetAwaiter().GetResult();
    }
}
