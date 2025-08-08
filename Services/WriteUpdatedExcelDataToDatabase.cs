// Ignore Spelling: dict

using ExcelReader.RyanW84.Abstractions.Data.DatabaseServices;
using ExcelReader.RyanW84.Abstractions.Data.TableCreators;
using ExcelReader.RyanW84.Abstractions.Services;
using ExcelReader.RyanW84.Data;

namespace ExcelReader.RyanW84.Services;

public class WriteUpdatedExcelDataToDatabase(
	IDataTableService dictToTableConverter ,
	IExcelTableCreator createTableFromAnyExcel ,
	ExcelReaderDbContext dbContext
) : IExcelDatabaseService
{
	private readonly IDataTableService _dictToTableConverter = dictToTableConverter;
	private readonly IExcelTableCreator _createTableFromAnyExcel = createTableFromAnyExcel;
	private readonly ExcelReaderDbContext _dbContext = dbContext;

	public async Task WriteAsync(Dictionary<string , string> fieldValues)
	{
		// Convert string dictionary to object dictionary for the converter
		var objDict = new Dictionary<string , object>(fieldValues.Count);
		foreach (var kvp in fieldValues)
		{
			objDict[kvp.Key] = kvp.Value;
		}

		// Convert to DataTable
		var dataTable = _dictToTableConverter.Convert(objDict);
		dataTable.TableName = "ExcelData";

		// Use the existing service to create and populate the table
		_createTableFromAnyExcel.CreateTableFromExcel(dataTable);
		await _dbContext.SaveChangesAsync();
	}

	// Keep synchronous version for backward compatibility during transition
	public void Write(Dictionary<string , string> fieldValues)
	{
		WriteAsync(fieldValues).GetAwaiter().GetResult();
	}
}
