using System.Data;
using ExcelReader.RyanW84.Abstractions.Services;

namespace ExcelReader.RyanW84.Services
{
    public class RecordSelectionService(INotificationService userNotifier) : IRecordSelectionService
    {
        private readonly INotificationService _userNotifier = userNotifier;

		public async Task<int> SelectRecordForUpdateAsync(DataTable dataTable)
        {
            return await Task.Run(() => SelectRecordForUpdate(dataTable));
        }

        public int SelectRecordForUpdate(DataTable dataTable)
        {
            if (dataTable == null || dataTable.Rows.Count == 0)
            {
                _userNotifier.ShowError("No records available for selection.");
                return -1;
            }

            Console.WriteLine("\n=== Select Record to Update ===");
            Console.WriteLine($"Found {dataTable.Rows.Count} records:");
            Console.WriteLine();

            // Display all records with index numbers
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                Console.WriteLine($"Record {i + 1}:");
                var row = dataTable.Rows[i];
                foreach (DataColumn column in dataTable.Columns)
                {
                    var value = row[column.ColumnName]?.ToString() ?? "NULL";
                    Console.WriteLine($"  {column.ColumnName}: {value}");
                }
                Console.WriteLine();
            }

            while (true)
            {
                Console.Write($"Select record number (1-{dataTable.Rows.Count}) or 0 to cancel: ");
                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                if (int.TryParse(input, out int selection))
                {
                    if (selection == 0)
                    {
                        Console.WriteLine("Selection cancelled.");
                        return -1;
                    }

                    if (selection >= 1 && selection <= dataTable.Rows.Count)
                    {
                        Console.WriteLine($"Selected record {selection}");
                        return selection - 1; // Convert to 0-based index
                    }
                }

                Console.WriteLine($"Invalid selection. Please enter a number between 1 and {dataTable.Rows.Count}, or 0 to cancel.");
            }
        }
    }
}