// Services\ReadFromPdf.cs
using System.Data;
using ExcelReader.RyanW84.Helpers;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.Text.RegularExpressions;
using ExcelReader.RyanW84.Abstractions.Services;
using ExcelReader.RyanW84.Abstractions.FileOperations.Readers;
using ExcelReader.RyanW84.Abstractions.Common;

namespace ExcelReader.RyanW84.Abstractions;

public class ReadFromPdf : IPdfTableReader
{
	private readonly IFilePathService _filePathManager;
	private readonly INotificationService _userNotifier;

	public ReadFromPdf(IFilePathService filePathManager , INotificationService userNotifier)
	{
		_filePathManager = filePathManager;
		_userNotifier = userNotifier;
	}

	public async Task<List<string[]>> ReadPdfFileAsync( )
	{
		string filePath;
		try
		{
			var customDefault = @"C:\Users\Ryanw\OneDrive\Documents\GitHub\CodeReviews.Console.ExcelReader\Data\TablePDF.pdf";
			filePath = _filePathManager.GetFilePath(FileType.PDF , customDefault);
		}
		catch (FilePathValidationException ex)
		{
			_userNotifier.ShowError($"PDF file path error: {ex.Message}");
			return [];
		}

		return await Task.Run(( ) =>
		{
			_userNotifier.ShowInfo($"Opening {filePath}");
			var result = new List<string[]>();

			try
			{
				using var pdfReader = new PdfReader(filePath);
				using var pdfDoc = new PdfDocument(pdfReader);

				for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
				{
					_userNotifier.ShowInfo($"Processing page {i} of {pdfDoc.GetNumberOfPages()}");
					var page = pdfDoc.GetPage(i);
					var strategy = new SimpleTextExtractionStrategy();
					var text = PdfTextExtractor.GetTextFromPage(page , strategy);

					// Split the text into lines and process each one
					var lines = text.Split('\n' , StringSplitOptions.RemoveEmptyEntries);

					// Process each line and try to identify tables
					foreach (var line in lines)
					{
						var trimmedLine = line.Trim();
						if (string.IsNullOrWhiteSpace(trimmedLine))
							continue;

						// Try to detect table rows by looking for patterns
						// This is a simple approach - a more sophisticated approach would use regex or a PDF table extraction library
						var cells = SplitLineIntoCells(trimmedLine);

						if (cells.Length > 1) // Only add if we have at least two cells (likely a table row)
						{
							result.Add(cells);
						}
					}
				}

				_userNotifier.ShowInfo($"Extracted {result.Count} rows from PDF");
			}
			catch (Exception ex)
			{
				_userNotifier.ShowError($"Error reading PDF: {ex.Message}");
			}

			return result;
		});
	}

	private string[] SplitLineIntoCells(string line)
	{
		// This is a simplified approach to split a line into cells
		// Try multiple methods to identify the most likely table structure

		// Method 1: Split by multiple spaces (common in PDFs with fixed-width tables)
		var multiSpaceSplit = Regex.Split(line , @"\s{2,}").Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
		if (multiSpaceSplit.Length > 1)
			return multiSpaceSplit;

		// Method 2: Split by tab characters
		var tabSplit = line.Split('\t' , StringSplitOptions.RemoveEmptyEntries);
		if (tabSplit.Length > 1)
			return tabSplit;

		// Method 3: Look for patterns like delimiters (| or , or ;)
		foreach (var delimiter in new[] { '|' , ',' , ';' })
		{
			if (line.Contains(delimiter))
			{
				return line.Split(delimiter , StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
			}
		}

		// Fall back to basic whitespace splitting if we can't identify a clear structure
		return line.Split(new[] { ' ' } , StringSplitOptions.RemoveEmptyEntries);
	}

	public List<string[]> ReadPdfFile( )
	{
		return ReadPdfFileAsync().GetAwaiter().GetResult();
	}

	public async Task<DataTable> ConvertToDataTableAsync(List<string[]> pdfData)
	{
		return await Task.Run(() =>
		{
			var dataTable = new DataTable();

			if (pdfData == null || pdfData.Count == 0)
				return dataTable;

			// Determine the maximum number of columns in the data
			var maxColumns = pdfData.Max(row => row.Length);

			// Add columns to the data table
			for (int i = 0; i < maxColumns; i++)
			{
				// Try to use the first row as headers if it has the same number of columns
				var columnName = pdfData.Count > 0 && pdfData[0].Length > i && !string.IsNullOrWhiteSpace(pdfData[0][i])
					? pdfData[0][i]
					: $"Column{i + 1}";

				// Make sure column names are unique
				if (dataTable.Columns.Contains(columnName))
					columnName = $"{columnName}_{i}";

				dataTable.Columns.Add(columnName);
			}

			// Add data rows, starting from the second row if the first was used for headers
			var startRow = pdfData.Count > 1 && pdfData[2].Length == maxColumns ? 1 : 0;

			for (int rowIndex = startRow; rowIndex < pdfData.Count; rowIndex++)
			{
				var row = pdfData[rowIndex];
				var dataRow = dataTable.NewRow();

				for (int colIndex = 0; colIndex < row.Length; colIndex++)
				{
					dataRow[colIndex] = row[colIndex];
				}

				dataTable.Rows.Add(dataRow);
			}

			return dataTable;
		});
	}

	public DataTable ConvertToDataTable(List<string[]> pdfData)
	{
		return ConvertToDataTableAsync(pdfData).GetAwaiter().GetResult();
	}
}