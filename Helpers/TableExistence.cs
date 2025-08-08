using System.Data;
using ExcelReader.RyanW84.Abstractions.Core;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ExcelReader.RyanW84.Helpers;

/// <summary>
/// Service for managing table existence and creation in SQL Server
/// </summary>
public class TableExistence(IConfiguration configuration) : ITableManager
{
    private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

	public void EnsureTableExists(DataTable dataTable, SqlConnection connection)
    {
        ArgumentNullException.ThrowIfNull(dataTable);
        ArgumentNullException.ThrowIfNull(connection);

        var sanitizedTableName = SqlDataTypeMapper.SanitizeTableName(dataTable.TableName);
        
        // Check if table exists using modern SQL Server approach
        if (TableExists(sanitizedTableName, connection))
            return;

        // Create table using shared utility
        var createTableSql = SqlDataTypeMapper.BuildCreateTableStatement(
            dataTable.TableName, 
            dataTable.Columns
        );

        using var command = new SqlCommand(createTableSql, connection);
        command.ExecuteNonQuery();
    }

    public void EnsureTableExists(DataTable dataTable)
    {
        ArgumentNullException.ThrowIfNull(dataTable);

        var connectionString = _configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection string not found in configuration");

        using var connection = new SqlConnection(connectionString);
        connection.Open();
        EnsureTableExists(dataTable, connection);
    }

    // Implement async methods
    public async Task EnsureTableExistsAsync(DataTable dataTable, SqlConnection connection)
    {
        ArgumentNullException.ThrowIfNull(dataTable);
        ArgumentNullException.ThrowIfNull(connection);

        var sanitizedTableName = SqlDataTypeMapper.SanitizeTableName(dataTable.TableName);
        
        // Check if table exists using modern SQL Server approach
        if (await TableExistsAsync(sanitizedTableName, connection))
            return;

        // Create table using shared utility
        var createTableSql = SqlDataTypeMapper.BuildCreateTableStatement(
            dataTable.TableName, 
            dataTable.Columns
        );

        await using var command = new SqlCommand(createTableSql, connection);
        await command.ExecuteNonQueryAsync();
    }

    public async Task EnsureTableExistsAsync(DataTable dataTable)
    {
        ArgumentNullException.ThrowIfNull(dataTable);

        var connectionString = _configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection string not found in configuration");

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await EnsureTableExistsAsync(dataTable, connection);
    }

    private static bool TableExists(string tableName, SqlConnection connection)
    {
        const string checkTableSql = """
            SELECT COUNT(1) 
            FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_NAME = @tableName 
            AND TABLE_TYPE = 'BASE TABLE'
            """;

        using var command = new SqlCommand(checkTableSql, connection);
        command.Parameters.AddWithValue("@tableName", tableName);
        
        var result = command.ExecuteScalar();
        return Convert.ToInt32(result) > 0;
    }

    private static async Task<bool> TableExistsAsync(string tableName, SqlConnection connection)
    {
        const string checkTableSql = """
            SELECT COUNT(1) 
            FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_NAME = @tableName 
            AND TABLE_TYPE = 'BASE TABLE'
            """;

        await using var command = new SqlCommand(checkTableSql, connection);
        command.Parameters.AddWithValue("@tableName", tableName);
        
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }
}