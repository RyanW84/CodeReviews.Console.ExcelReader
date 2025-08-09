using System.Data;

namespace ExcelReader.RyanW84.Abstractions.Data.DatabaseServices
{
    public interface IExcelUpdateService
    {
        /// <summary>
        /// Updates a specific record in the database based on row index
        /// </summary>
        /// <param name="fieldValues">Updated field values</param>
        /// <param name="originalDataTable">Original data table for context</param>
        /// <param name="rowIndex">Index of the row to update</param>
        Task UpdateRecordAsync(Dictionary<string, string> fieldValues, DataTable originalDataTable, int rowIndex);
    }
}