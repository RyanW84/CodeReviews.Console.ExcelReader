using System.Threading.Tasks;

namespace ExcelReader.RyanW84.Abstractions.Common
{
    public interface IFilePathValidation
    {
        bool ValidateFilePath(string filePath);
        bool FileExists(string filePath);
        bool HasReadPermission(string filePath);
        bool HasWritePermission(string filePath);
        bool IsValidExtension(string filePath, params string[] validExtensions);
        
        // Async versions
        Task<bool> ValidateFilePathAsync(string filePath);
        Task<bool> FileExistsAsync(string filePath);
        Task<bool> HasReadPermissionAsync(string filePath);
        Task<bool> HasWritePermissionAsync(string filePath);
        Task<bool> IsValidExtensionAsync(string filePath, params string[] validExtensions);
    }
}