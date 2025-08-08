// Abstractions\IPdfFormDatabaseService.cs
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExcelReader.RyanW84.Abstractions.Data.DatabaseServices
{
    public interface IPdfFormDatabaseService
    {
        Task WriteAsync(Dictionary<string, string> fieldValues);
    }
}