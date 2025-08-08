using System.Data;
using ExcelReader.RyanW84.Abstractions.Data.DatabaseServices;
using ExcelReader.RyanW84.Abstractions.Services;
using ExcelReader.RyanW84.Models;
using ExcelReader.RyanW84.Helpers;

namespace ExcelReader.RyanW84.Controller;

public class ExcelBeginnerController(
	IExcelBeginnerService excelBeginnerService ,
	IExcelReaderDbContext dbContext ,
	INotificationService notificationService) : DataImportControllerBase(dbContext, notificationService)
{
    private readonly IExcelBeginnerService _excelBeginnerService = excelBeginnerService ?? throw new ArgumentNullException(nameof(excelBeginnerService));

	public async Task AddDataFromExcel()
    {
        await ExecuteDomainImportAsync(
            _excelBeginnerService,
            "Excel",
            service => service.ReadFromExcel(),
            ConvertDataTableToModels,
            async (dbContext, models) => dbContext.ExcelBeginner.AddRange(models)
        );
    }

    private List<ExcelBeginner> ConvertDataTableToModels(DataTable dataTable) =>
		[.. dataTable.Rows
            .Cast<DataRow>()
            .Select(row => new ExcelBeginner
            {
                Name = row.GetStringValue("Name"),
                Age = row.GetIntValue("age"),
                Sex = row.GetStringValue("sex"),
                Colour = row.GetStringValue("colour"),
                Height = row.GetStringValue("height"),
            })
            .Where(model => !string.IsNullOrWhiteSpace(model.Name))];
}
