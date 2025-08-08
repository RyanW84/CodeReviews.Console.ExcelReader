using System.Data;

using ExcelReader.RyanW84.Abstractions.Services;

namespace ExcelReader.RyanW84.Helpers;

/// <summary>
/// Provides a method to convert a dictionary of string/object pairs to a DataTable.
/// </summary>
public class DictionaryToDataTableConverter : IDataTableService
{
    /// <summary>
    /// Converts a Dictionary<string, object> to a DataTable with one row.
    /// </summary>
    /// <param name="dictionary">The dictionary to convert.</param>
    /// <returns>A DataTable with columns from the dictionary keys and a single row of values.</returns>
    public DataTable Convert(Dictionary<string, object> dictionary)
    {
        var dataTable = new DataTable();
        foreach (var key in dictionary.Keys)
        {
            dataTable.Columns.Add(key);
        }

        var row = dataTable.NewRow();

        foreach (var kvp in dictionary)
        {
            row[kvp.Key] = kvp.Value ?? DBNull.Value;
        }

        dataTable.Rows.Add(row);
        return dataTable;
    }
}
