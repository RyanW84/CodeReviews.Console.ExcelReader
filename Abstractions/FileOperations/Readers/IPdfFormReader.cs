namespace ExcelReader.RyanW84.Abstractions.FileOperations.Readers;

/// <summary>
/// Interface for PDF form reading operations
/// </summary>
public interface IPdfFormReader
{
    Task<Dictionary<string, string>> ReadFormFieldsAsync(string filePath);
    Dictionary<string, string> ReadFormFields(string filePath);
}