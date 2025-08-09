using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExcelReader.RyanW84.Abstractions.Data.DatabaseServices
{
    public interface IPdfWriterDatabaseService
    {
        Task WriteAsync(Dictionary<string, string> fieldValues);
    }
}