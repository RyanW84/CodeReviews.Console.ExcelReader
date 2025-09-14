namespace ExcelReader.RyanW84;

/// <summary>
/// Custom exception for file reading operations
/// </summary>
public class FileReaderException : Exception
{
    public string? FilePath { get; }
    public string? Operation { get; }

    public FileReaderException(string message) : base(message)
    {
    }

    public FileReaderException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public FileReaderException(string message, string filePath, string operation)
        : base(message)
    {
        FilePath = filePath;
        Operation = operation;
    }

    public FileReaderException(string message, string filePath, string operation, Exception innerException)
        : base(message, innerException)
    {
        FilePath = filePath;
        Operation = operation;
    }
}
