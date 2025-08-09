namespace ExcelReader.RyanW84.Abstractions.Services;

public interface ICsvParser
{
    Task<List<string[]>> ParseFileAsync(string filePath);
    string[] ParseLine(string line);
}