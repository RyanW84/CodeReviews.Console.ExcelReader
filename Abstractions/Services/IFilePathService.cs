using ExcelReader.RyanW84.Abstractions.Common;

namespace ExcelReader.RyanW84.Abstractions.Services;

/// <summary>
/// Interface for file path management
/// </summary>
public interface IFilePathService
{
    string GetFilePath(FileType fileType, string? customDefault = null);
    void ValidateFilePath(string filePath, FileType fileType);
    bool IsValidPath(string filePath);
}