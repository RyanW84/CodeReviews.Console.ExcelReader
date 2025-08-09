using System.Data;
using ExcelReader.RyanW84.Abstractions.Common;
using ExcelReader.RyanW84.Abstractions.Core;
using ExcelReader.RyanW84.Abstractions.FileOperations.Readers;
using ExcelReader.RyanW84.Abstractions.Services;

namespace ExcelReader.RyanW84.Services;

/// <summary>
/// CSV file reader focused solely on reading CSV files.
/// Follows Single Responsibility Principle.
/// </summary>
public class CsvFileReader(
	IFilePathService filePathManager ,
	INotificationService userNotifier ,
	ICsvParser csvParser
	) : ICsvFileReader
{
    private readonly IFilePathService _filePathManager =
			filePathManager ?? throw new ArgumentNullException(nameof(filePathManager));
    private readonly INotificationService _userNotifier = userNotifier ?? throw new ArgumentNullException(nameof(userNotifier));
    private readonly ICsvParser _csvParser = csvParser ?? throw new ArgumentNullException(nameof(csvParser));

	public async Task<List<string[]>> ReadCsvFile()
    {
        try
        {
            var filePath = _filePathManager.GetFilePath(FileType.CSV, GetDefaultPath());
            if (string.IsNullOrEmpty(filePath))
                return [];

            _userNotifier.ShowInfo($"Opening CSV file: {Path.GetFileName(filePath)}");

            return await _csvParser.ParseFileAsync(filePath);
        }
        catch (Exception ex)
        {
            _userNotifier.ShowError($"Failed to read CSV file: {ex.Message}");
            throw;
        }
    }

    private static string GetDefaultPath() =>
        Path.Combine("Data", "ExcelCSV.CSV");
}
