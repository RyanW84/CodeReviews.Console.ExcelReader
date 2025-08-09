using System.Data;

namespace ExcelReader.RyanW84.Abstractions.Services
{
    public interface IRecordSelectionService
    {
        /// <summary>
        /// Displays available records and allows user to select one for updating
        /// </summary>
        /// <param name="dataTable">DataTable containing all records</param>
        /// <returns>The selected row index, or -1 if cancelled</returns>
        Task<int> SelectRecordForUpdateAsync(DataTable dataTable);
        
        /// <summary>
        /// Synchronous version of record selection
        /// </summary>
        int SelectRecordForUpdate(DataTable dataTable);
    }
}