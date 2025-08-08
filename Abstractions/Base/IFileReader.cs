using System.Threading.Tasks;

namespace ExcelReader.RyanW84.Abstractions.Base;

/// <summary>
/// Interface for file reading operations - follows Interface Segregation Principle
/// </summary>
/// <typeparam name="T">The type of data to read</typeparam>
public interface IFileReader<T>
{
    /// <summary>
    /// Reads data from a file asynchronously
    /// </summary>
    /// <param name="filePath">Path to the file to read</param>
    /// <returns>Data read from the file</returns>
    Task<T> ReadAsync(string filePath);
    
    /// <summary>
    /// Validates if the file can be read
    /// </summary>
    /// <param name="filePath">Path to the file to validate</param>
    /// <returns>True if file can be read, false otherwise</returns>
    bool CanRead(string filePath);
}