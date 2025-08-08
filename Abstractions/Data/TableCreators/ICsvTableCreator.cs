// Abstractions\ICsvTableCreator.cs
using System.Data;
using System.Threading.Tasks;

namespace ExcelReader.RyanW84.Abstractions.Data.TableCreators
{
    /// <summary>
    /// Interface for CSV table creation operations
    /// </summary>
    public interface ICsvTableCreator
    {
        /// <summary>
        /// Asynchronously creates a table from CSV data
        /// </summary>
        /// <param name="dataTable">DataTable containing CSV data</param>
        /// <returns>Task representing the async operation</returns>
        Task CreateTableFromCsvDataAsync(DataTable dataTable);
    }
}