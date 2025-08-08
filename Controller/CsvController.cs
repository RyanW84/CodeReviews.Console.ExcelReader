using System.Data;
using ExcelReader.RyanW84.Abstractions.Core;
using ExcelReader.RyanW84.Abstractions.Data.DatabaseServices;
using ExcelReader.RyanW84.Abstractions.Data.TableCreators;
using ExcelReader.RyanW84.Abstractions.FileOperations.Readers;
using ExcelReader.RyanW84.Abstractions.Services;

namespace ExcelReader.RyanW84.Controller;

public class CsvController(
	IExcelReaderDbContext dbContext ,
	ICsvFileReader csvFileReader ,
	ICsvTableCreator createTableFromCSV ,
	INotificationService notificationService ,
	IDataConverter<List<string[]> , DataTable> csvDataConverter
	) : DataImportControllerBase(dbContext, notificationService)
{
    private readonly ICsvFileReader _csvFileReader = csvFileReader ?? throw new ArgumentNullException(nameof(csvFileReader));
    private readonly ICsvTableCreator _createTableFromCSV =
			createTableFromCSV ?? throw new ArgumentNullException(nameof(createTableFromCSV));
    private readonly IDataConverter<List<string[]>, DataTable> _csvDataConverter =
			csvDataConverter ?? throw new ArgumentNullException(nameof(csvDataConverter));

	public async Task AddDataFromCsv()
    {
        await ExecuteTableImportAsync(
            _csvFileReader,
            _createTableFromCSV,
            "CSV",
            async reader =>
            {
                var csvData = await reader.ReadCsvFile();
                return await _csvDataConverter.ConvertAsync(csvData);
            },
            (creator, dataTable) => creator.CreateTableFromCsvDataAsync(dataTable),
            "CsvImport"
        );
    }
}
