using ExcelReader.RyanW84.Abstractions.Common;
using ExcelReader.RyanW84.Abstractions.Services;

namespace ExcelReader.RyanW84.Services
{
    public class FilePathValidationService(INotificationService notificationService)
        : IFilePathValidation
    {
        private readonly INotificationService _notificationService = notificationService;

        public bool ValidateFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                _notificationService.ShowError("File path cannot be empty.");
                return false;
            }

            if (!FileExists(filePath))
            {
                _notificationService.ShowError($"File does not exist: {filePath}");
                return false;
            }

            if (!HasReadPermission(filePath))
            {
                _notificationService.ShowError($"No read permission for file: {filePath}");
                return false;
            }

            return true;
        }

        public bool FileExists(string filePath)
        {
            return !string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath);
        }

        public bool HasReadPermission(string filePath)
        {
            if (!FileExists(filePath))
                return false;

            try
            {
                using var fs = File.OpenRead(filePath);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool HasWritePermission(string filePath)
        {
            if (!FileExists(filePath))
                return false;

            try
            {
                using var fs = File.OpenWrite(filePath);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool IsValidExtension(string filePath, params string[] validExtensions)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            var extension = Path.GetExtension(filePath)?.ToLowerInvariant();
            return !string.IsNullOrEmpty(extension)
                && validExtensions.Any(e =>
                    e.Equals(extension, StringComparison.InvariantCultureIgnoreCase)
                );
        }

        // Async implementations
        public Task<bool> ValidateFilePathAsync(string filePath)
        {
            return Task.FromResult(ValidateFilePath(filePath));
        }

        public Task<bool> FileExistsAsync(string filePath)
        {
            return Task.FromResult(FileExists(filePath));
        }

        public Task<bool> HasReadPermissionAsync(string filePath)
        {
            return Task.FromResult(HasReadPermission(filePath));
        }

        public Task<bool> HasWritePermissionAsync(string filePath)
        {
            return Task.FromResult(HasWritePermission(filePath));
        }

        public Task<bool> IsValidExtensionAsync(string filePath, params string[] validExtensions)
        {
            return Task.FromResult(IsValidExtension(filePath, validExtensions));
        }
    }
}
