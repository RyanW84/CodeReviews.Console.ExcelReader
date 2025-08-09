using System.Data;
using ExcelReader.RyanW84.Abstractions.Data.DatabaseServices;
using ExcelReader.RyanW84.Abstractions.Services;
using ExcelReader.RyanW84.Helpers;
using ExcelReader.RyanW84.Data.Models;

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
                Age = row.GetIntValue("Age"),
                Sex = row.GetStringValue("Sex"),
                Colour = row.GetStringValue("Colour"),
                Height = row.GetStringValue("Height"),
            })
            .Where(model => !string.IsNullOrWhiteSpace(model.Name))];
}
