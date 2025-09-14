using System.Data;
using System.Text;

namespace ExcelReader.RyanW84.Helpers;

/// <summary>
/// Utility class for SQL data type mapping and name sanitization
/// </summary>
public static class SqlDataTypeMapper
{
    private static readonly char[] InvalidChars = [' ', '\n', '\r', '\t', ',', '.', '/', '\\', '[', ']', '(', ')', '{', '}'];

    /// <summary>
    /// Maps .NET types to SQL Server data types
    /// </summary>
    public static string GetSqlDataType(Type type)
    {
        return type.Name.ToLowerInvariant() switch
        {
            "string" => "NVARCHAR(MAX)",
            "int32" => "INT",
            "int64" => "BIGINT",
            "decimal" => "DECIMAL(18,2)",
            "double" => "FLOAT",
            "datetime" => "DATETIME2",
            "datetimeoffset" => "DATETIMEOFFSET",
            "boolean" => "BIT",
            "guid" => "UNIQUEIDENTIFIER",
            _ => "NVARCHAR(MAX)",
        };
    }

    /// <summary>
    /// Sanitizes column names for SQL Server compatibility
    /// </summary>
    public static string SanitizeColumnName(string columnName)
    {
        if (string.IsNullOrWhiteSpace(columnName))
            return "Col_Unknown";

        var sanitized = InvalidChars.Aggregate(columnName.Trim(), (current, c) => current.Replace(c, '_'));

        // Ensure the name starts with a letter or underscore
        if (!char.IsLetter(sanitized[0]) && sanitized[0] != '_')
        {
            sanitized = $"Col_{sanitized}";
        }

        // Limit length to SQL Server identifier limit (128 chars)
        if (sanitized.Length > 128)
        {
            sanitized = sanitized.Substring(0, 128);
        }

        return sanitized;
    }

    /// <summary>
    /// Sanitizes table names for SQL Server compatibility
    /// </summary>
    public static string SanitizeTableName(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
            return "Table_Unknown";

        var sanitized = InvalidChars.Aggregate(tableName.Trim(), (current, c) => current.Replace(c, '_'));

        // Ensure the name starts with a letter or underscore
        if (!char.IsLetter(sanitized[0]) && sanitized[0] != '_')
        {
            sanitized = $"Table_{sanitized}";
        }

        // Limit length to SQL Server identifier limit (128 chars)
        if (sanitized.Length > 128)
        {
            sanitized = sanitized.Substring(0, 128);
        }

        return sanitized;
    }

    /// <summary>
    /// Builds a CREATE TABLE SQL statement
    /// </summary>
    public static string BuildCreateTableStatement(string tableName, DataColumnCollection columns)
    {
        var sanitizedTableName = SanitizeTableName(tableName);
        var sb = new StringBuilder($"CREATE TABLE [{sanitizedTableName}] (\n");

        for (int i = 0; i < columns.Count; i++)
        {
            var column = columns[i];
            var sanitizedColumnName = SanitizeColumnName(column.ColumnName);
            var sqlDataType = GetSqlDataType(column.DataType);
            sb.Append($"[{sanitizedColumnName}] {sqlDataType}");

            if (i < columns.Count - 1)
                sb.Append(",\n");
        }

        sb.Append("\n)");
        return sb.ToString();
    }
}
