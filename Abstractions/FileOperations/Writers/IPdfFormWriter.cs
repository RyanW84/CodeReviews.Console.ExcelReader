namespace ExcelReader.RyanW84.Abstractions.FileOperations.Writers;

/// <summary>
/// Interface for PDF form writing operations
/// </summary>
public interface IPdfFormWriter
{
    Task WriteFormFieldsAsync(string filePath, Dictionary<string, string> fieldValues);
    void WriteFormFields(string filePath, Dictionary<string, string> fieldValues);
}