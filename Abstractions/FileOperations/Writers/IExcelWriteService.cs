namespace ExcelReader.RyanW84.Abstractions.FileOperations.Writers;

/// <summary>
/// Interface for Excel writing operations
/// </summary>
public interface IExcelWriteService
{
    void WriteFieldsToExcel(string filePath, Dictionary<string, string> fieldValues);
}