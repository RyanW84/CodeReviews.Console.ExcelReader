using System.Data;
using Microsoft.Data.SqlClient;

namespace ExcelReader.RyanW84.Abstractions.Core;

/// <summary>
/// Interface for table existence and management operations
/// </summary>
public interface ITableManager
{
    /// <summary>
    /// Ensures a table exists in the database, creating it if necessary
    /// </summary>
    /// <param name="dataTable">DataTable containing the schema to create</param>
    /// <param name="connection">Active SQL connection to use</param>
    void EnsureTableExists(DataTable dataTable, SqlConnection connection);

    /// <summary>
    /// Ensures a table exists in the database, creating it if necessary
    /// </summary>
    /// <param name="dataTable">DataTable containing the schema to create</param>
    void EnsureTableExists(DataTable dataTable);
}