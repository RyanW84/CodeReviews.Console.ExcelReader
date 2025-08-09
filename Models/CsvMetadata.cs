using CsvHelper.Configuration.Attributes;

namespace ExcelReader.RyanW84.Models;

/// <summary>
/// Simplified CSV metadata using CsvHelper capabilities
/// </summary>
public class CsvMetadata
{
	public string FileName { get; set; } = string.Empty;
	public string FilePath { get; set; } = string.Empty;
	public long FileSize { get; set; }
	public int ColumnCount { get; set; }
	public int DataRowCount { get; set; }
	public string[] Headers { get; set; } = [];
	public DateTime LastModified { get; set; }
	public DateTime AnalyzedAt { get; set; } = DateTime.Now;
	public List<string> ValidationErrors { get; set; } = [];
	public List<string> ValidationWarnings { get; set; } = [];

	public string FormattedFileSize => FileSize switch
	{
		< 1024 => $"{FileSize} B",
		< 1024 * 1024 => $"{FileSize / 1024.0:F1} KB",
		< 1024 * 1024 * 1024 => $"{FileSize / (1024.0 * 1024):F1} MB",
		_ => $"{FileSize / (1024.0 * 1024 * 1024):F1} GB"
	};

	public override string ToString( )
	{
		return $"CSV: {FileName} ({DataRowCount} rows, {ColumnCount} cols, {FormattedFileSize})";
	}
}

/// <summary>
/// Dynamic CSV record for unknown structure files
/// </summary>
public class DynamicCsvRecord
{
	public Dictionary<string , string> Fields { get; set; } = [];
}