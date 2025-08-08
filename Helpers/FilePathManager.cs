using ExcelReader.RyanW84.Abstractions.Services;

using Spectre.Console;

using FileType = ExcelReader.RyanW84.Abstractions.Common.FileType;

namespace ExcelReader.RyanW84.Helpers;

/// <summary>
/// File path service implementation following SOLID principles
/// </summary>
public class FilePathManager : IFilePathService
{
    private static readonly Dictionary<FileType, string> DefaultPaths = new()
    {
        { FileType.Excel, @"C:\Users\Ryanw\OneDrive\Documents\GitHub\Excel-Reader\Data\ExcelDynamic.xlsx" },
        { FileType.PDF, @"C:\Users\Ryanw\OneDrive\Documents\GitHub\Excel-Reader\Data\FillablePDF.pdf" },
        { FileType.CSV, @"C:\Users\Ryanw\OneDrive\Documents\GitHub\Excel-Reader\Data\ExcelCSV.csv" }
    };

    /// <summary>
    /// Gets a file path with user interaction and validation
    /// </summary>
    /// <param name="fileType">The type of file to select</param>
    /// <param name="customDefault">Custom default path (optional)</param>
    /// <returns>Validated file path</returns>
    /// <exception cref="FilePathValidationException">Thrown when file path validation fails</exception>
    public string GetFilePath(FileType fileType, string? customDefault = null)
    {
        try
        {
            var defaultPath = customDefault ?? GetDefaultPath(fileType);
            var fileTypeName = GetFileTypeName(fileType);

            var useExisting = AnsiConsole.Confirm(
                $"Use existing {fileTypeName} file path? [green]{defaultPath}[/]"
            );

            var filePath = useExisting
                ? defaultPath
                : AnsiConsole.Ask<string>($"Enter the path to the {fileTypeName} file:", defaultPath);

            ValidateFilePath(filePath, fileType);
            return filePath;
        }
        catch (FilePathValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new FilePathValidationException($"Unexpected error while getting file path: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Validates a file path with comprehensive checks
    /// </summary>
    /// <param name="filePath">The file path to validate</param>
    /// <param name="fileType">The expected file type</param>
    /// <exception cref="FilePathValidationException">Thrown when validation fails</exception>
    public void ValidateFilePath(string filePath, FileType fileType)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new FilePathValidationException("File path cannot be empty or null.");
        }

        if (!File.Exists(filePath))
        {
            throw new FilePathValidationException($"File not found: {filePath}",
                new FileNotFoundException($"The file '{filePath}' was not found."));
        }

        try
        {
            using var fileStream = File.OpenRead(filePath);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new FilePathValidationException($"Access denied to file: {filePath}. Please check file permissions.", ex);
        }
        catch (IOException ex)
        {
            throw new FilePathValidationException($"I/O error accessing file: {filePath}. The file may be in use by another application.", ex);
        }
    }

    /// <summary>
    /// Checks if a file path is valid without throwing exceptions
    /// </summary>
    /// <param name="filePath">The file path to check</param>
    /// <returns>True if valid, false otherwise</returns>
    public bool IsValidPath(string filePath)
    {
        try
        {
            ValidateFilePath(filePath, FileType.Generic);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string GetDefaultPath(FileType fileType)
    {
        return DefaultPaths.TryGetValue(fileType, out var path) ? path : string.Empty;
    }

    private static string GetFileTypeName(FileType fileType)
    {
        return fileType switch
        {
            FileType.Excel => "Excel",
            FileType.PDF => "PDF",
            FileType.CSV => "CSV",
            FileType.Generic => "file",
            _ => "file"
        };
    }
}

public class FilePathValidationException : Exception
{
    public FilePathValidationException(string message) : base(message) { }
    public FilePathValidationException(string message, Exception innerException) : base(message, innerException) { }
}