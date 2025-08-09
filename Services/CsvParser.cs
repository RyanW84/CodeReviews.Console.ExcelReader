using System.Text;
using ExcelReader.RyanW84.Abstractions.Services;

namespace ExcelReader.RyanW84.Services;

/// <summary>
/// Handles CSV parsing logic.
/// Follows Single Responsibility Principle.
/// </summary>
public class CsvParser(INotificationService notificationService) : ICsvParser
{
    private readonly INotificationService _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    private const char DefaultDelimiter = ',';

	public async Task<List<string[]>> ParseFileAsync(string filePath)
    {
        var csvData = new List<string[]>();

        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

        string? line;
        var lineNumber = 0;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            lineNumber++;

            if (string.IsNullOrWhiteSpace(line))
            {
                _notificationService.ShowWarning($"Skipping empty line {lineNumber}");
                continue;
            }

            try
            {
                var rowData = ParseLine(line);
                csvData.Add(rowData);
            }
            catch (Exception ex)
            {
                _notificationService.ShowWarning($"Error parsing line {lineNumber}: {ex.Message}");
            }
        }

        return csvData;
    }

    public string[] ParseLine(string line)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++; // Skip next quote
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == DefaultDelimiter && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        result.Add(current.ToString());
        return [.. result];
    }
}