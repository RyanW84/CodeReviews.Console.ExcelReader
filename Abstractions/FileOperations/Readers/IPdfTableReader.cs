using System.Data;

namespace ExcelReader.RyanW84.Abstractions.FileOperations.Readers;

/// <summary>
/// Interface for PDF table reading operations
/// </summary>
public interface IPdfTableReader 
{ 
    Task<List<string[]>> ReadPdfFileAsync();
    List<string[]> ReadPdfFile();
    
    /// <summary>
    /// Converts the PDF data (list of string arrays) to a DataTable
    /// </summary>
    /// <param name="pdfData">The PDF data as list of string arrays</param>
    /// <returns>A DataTable containing the PDF data</returns>
    Task<DataTable> ConvertToDataTableAsync(List<string[]> pdfData);
}