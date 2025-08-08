// Abstractions\IExcelTableCreator.cs
using System.Data;

namespace ExcelReader.RyanW84.Abstractions.Data.TableCreators
{
    public interface IExcelTableCreator
    {
        void CreateTableFromExcel(DataTable dataTable);
    }
}