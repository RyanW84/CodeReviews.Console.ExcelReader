using System.Data;

namespace ExcelReader.RyanW84.Helpers;

public static class DataRowExtensions
{
    public static string GetStringValue(this DataRow row, string columnName)
    {
        return row[columnName]?.ToString()?.Trim() ?? string.Empty;
    }

    public static int GetIntValue(this DataRow row, string columnName)
    {
        var value = row[columnName]?.ToString()?.Trim();
        return int.TryParse(value, out var result) ? result : 0;
    }

    public static T? GetValue<T>(this DataRow row, string columnName)
        where T : struct
    {
        var value = row[columnName]?.ToString()?.Trim();
        if (string.IsNullOrEmpty(value))
            return null;

        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return null;
        }
    }
}
