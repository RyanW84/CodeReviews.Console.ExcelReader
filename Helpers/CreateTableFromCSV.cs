using System.Data;
using ExcelReader.RyanW84.Abstractions.Data.TableCreators;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ExcelReader.RyanW84.Helpers;

/// <summary>
/// CSV table creator implementation using shared SQL utilities
/// </summary>
public class CreateTableFromCSV(IConfiguration configuration) : ICsvTableCreator
{
    private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

	public async Task CreateTableFromCsvDataAsync(DataTable dataTable)
    {
        ArgumentNullException.ThrowIfNull(dataTable);

        var connectionString = _configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection string not found");

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        // Use shared utility for consistent table creation
        var createTableSql = SqlDataTypeMapper.BuildCreateTableStatement(
            dataTable.TableName,
            dataTable.Columns
        );

        await using var command = new SqlCommand(createTableSql, connection);
        await command.ExecuteNonQueryAsync();

        // Bulk copy the data using sanitized table name
        using var bulkCopy = new SqlBulkCopy(connection);
        bulkCopy.DestinationTableName = SqlDataTypeMapper.SanitizeTableName(dataTable.TableName);
        await bulkCopy.WriteToServerAsync(dataTable);
    }
}
