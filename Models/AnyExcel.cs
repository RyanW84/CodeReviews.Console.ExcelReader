using System.Data;

namespace ExcelReader.RyanW84.Models;

/// <summary>
/// Represents a wrapper for a DataTable, used for Excel data transport.
/// </summary>
public class AnyExcel
{
    /// <summary>
    /// Gets or sets the DataTable containing Excel data.
    /// </summary>
    public DataTable DataTable { get; set; } = new();
}