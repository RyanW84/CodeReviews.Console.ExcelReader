namespace ExcelReader.RyanW84.Abstractions.Data.DatabaseServices;

/// <summary>
/// Interface for Excel database operations
/// </summary>
public interface IExcelDatabaseService
{
    Task WriteAsync(Dictionary<string, string> fieldValues);
}
