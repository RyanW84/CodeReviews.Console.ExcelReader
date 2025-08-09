using System.Data;
using ExcelReader.RyanW84.Abstractions.Data.DatabaseServices;
using ExcelReader.RyanW84.Abstractions.Data.TableCreators;
using Microsoft.Extensions.Configuration;

namespace ExcelReader.RyanW84.Services
{
    public class ExcelUpdateService(IConfiguration configuration , IExcelTableCreator tableCreator) : IExcelUpdateService
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly IExcelTableCreator _tableCreator = tableCreator;

		public async Task UpdateRecordAsync(Dictionary<string, string> fieldValues, DataTable originalDataTable, int rowIndex)
        {
            if (originalDataTable == null || rowIndex < 0 || rowIndex >= originalDataTable.Rows.Count)
            {
                throw new ArgumentException("Invalid data table or row index");
            }

            // Create a new DataTable with updated values
            var updatedDataTable = originalDataTable.Copy();
            var rowToUpdate = updatedDataTable.Rows[rowIndex];

            // Update the specific row with new values
            foreach (var kvp in fieldValues)
            {
                if (updatedDataTable.Columns.Contains(kvp.Key))
                {
                    rowToUpdate[kvp.Key] = kvp.Value ?? string.Empty;
                }
            }

            // Use timestamp-based table name to track updates
            updatedDataTable.TableName = $"ExcelData_Updated_{DateTime.Now:yyyyMMdd_HHmmss}";

            // Save the entire updated table to database 
            await Task.Run(() => _tableCreator.CreateTableFromExcel(updatedDataTable));
        }
    }
}