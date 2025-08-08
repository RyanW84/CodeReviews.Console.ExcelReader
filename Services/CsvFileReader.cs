using ExcelReader.RyanW84.Abstractions.Common;
using ExcelReader.RyanW84.Abstractions.FileOperations.Readers;
using ExcelReader.RyanW84.Abstractions.Services;

namespace ExcelReader.RyanW84.Services
{
    public class CsvFileReader : ICsvFileReader
    {
        private readonly IFilePathService _filePathService;
        private readonly INotificationService _notificationService;

        public CsvFileReader(IFilePathService filePathService, INotificationService notificationService)
        {
            _filePathService = filePathService;
            _notificationService = notificationService;
        }

        public async Task<List<string[]>> ReadCsvFile()
        {
            var filePath = _filePathService.GetFilePath(FileType.CSV);

            if (string.IsNullOrEmpty(filePath))
            {
                _notificationService.ShowError("No file selected.");
                return new List<string[]>();
            }

            try
            {
                var lines = await File.ReadAllLinesAsync(filePath);
                var result = new List<string[]>();

                foreach (var line in lines)
                {
                    // Simple CSV parsing - splits by comma
                    // For more complex CSV parsing, consider using a CSV library like CsvHelper
                    var values = line.Split(',');
                    result.Add(values);
                }

                _notificationService.ShowInfo($"Successfully read {result.Count} rows from CSV file.");
                return result;
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Error reading CSV file: {ex.Message}");
                return new List<string[]>();
            }
        }
    }
}