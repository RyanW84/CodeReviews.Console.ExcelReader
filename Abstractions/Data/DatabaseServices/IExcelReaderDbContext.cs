using ExcelReader.RyanW84.Data.Models;

using Microsoft.EntityFrameworkCore;

using System.Data;

namespace ExcelReader.RyanW84.Abstractions.Data.DatabaseServices
{
    public interface IExcelReaderDbContext
    {
        DbSet<ExcelBeginner> ExcelBeginner { get; set; }
        
        void EnsureDeleted();
        void EnsureCreated();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}