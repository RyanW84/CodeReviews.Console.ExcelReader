using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using ExcelReader.RyanW84.Abstractions.Data.DatabaseServices;
using ExcelReader.RyanW84.Data.Models;

namespace ExcelReader.RyanW84.Data
{
    public class ExcelReaderDbContext(DbContextOptions<ExcelReaderDbContext> options) : DbContext(options), IExcelReaderDbContext
    {
		public DbSet<ExcelBeginner> ExcelBeginner { get; set; }

        // Implement IExcelReaderDbContext methods
        public void EnsureDeleted()
        {
            Database.EnsureDeleted();
        }

        public void EnsureCreated()
        {
            Database.EnsureCreated();
        }

        public static ILoggerFactory GetLoggerFactory()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.AddFilter((category, level) =>
                    category == DbLoggerCategory.Database.Command.Name
                    && level == LogLevel.Information
                );
            });
            return loggerFactory;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<ExcelBeginner>()
                .HasData(
                    new List<ExcelBeginner>
                    {
                        new() { Id = 1, Name = "Bob Gates", Age = 67, Sex = "Male", Colour = "White", Height = "F507" },
                        new() { Id = 2, Name = "Jodie Cousins", Age = 28, Sex = "Female", Colour = "Black", Height = "F500" },
                        new() { Id = 3, Name = "Joseph Smith", Age = 30, Sex = "Male", Colour = "Asian", Height = "F600" }
                    }
                );
        }
    }

    public class ExcelDbContextFactory : IDesignTimeDbContextFactory<ExcelReaderDbContext>
    {
        public ExcelReaderDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ExcelReaderDbContext>();
            optionsBuilder.UseSqlServer(
                @"Server=(localdb)\MSSQLlocaldb; Database=ExcelReader; Integrated Security=True; MultipleActiveResultSets=True;")
                .EnableSensitiveDataLogging()
                .UseLoggerFactory(ExcelReaderDbContext.GetLoggerFactory());

            return new ExcelReaderDbContext(optionsBuilder.Options);
        }
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddExcelReaderDbContext(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<ExcelReaderDbContext>(options =>
                options.UseSqlServer(connectionString)
                       .UseLoggerFactory(ExcelReaderDbContext.GetLoggerFactory())
                       .EnableSensitiveDataLogging());

            return services;
        }
    }
}
