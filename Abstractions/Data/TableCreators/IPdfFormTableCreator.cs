// Abstractions\IPdfFormTableCreator.cs
using System.Data;
using System.Threading.Tasks;

namespace ExcelReader.RyanW84.Abstractions.Data.TableCreators
{
    public interface IPdfFormTableCreator
    {
        Task CreateTableFromPdfFormData(DataTable dataTable);
    }
}