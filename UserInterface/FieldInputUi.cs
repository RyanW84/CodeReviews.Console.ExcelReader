using ExcelReader.RyanW84.Abstractions.Common;
using ExcelReader.RyanW84.Abstractions.Services;
using ExcelReader.RyanW84.Helpers;

using Spectre.Console;

namespace ExcelReader.RyanW84.UserInterface;

/// <summary>
/// Unified field input UI for both Excel and PDF form field editing.
/// Consolidates common functionality while supporting format-specific features.
/// </summary>
public class FieldInputUi : IFieldInputService
{
    private readonly INotificationService _notificationService;

    public FieldInputUi(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// Async method to gather updated field values from user input.
    /// Works for both Excel and PDF scenarios.
    /// </summary>
    /// <param name="existingFields">Dictionary of current field values</param>
    /// <param name="fileType">Type of file being processed (for display messages)</param>
    /// <returns>Dictionary of updated field values</returns>
    public async Task<Dictionary<string, string>> GatherUpdatedFieldsAsync(
        Dictionary<string, string> existingFields,
        FileType fileType = FileType.Generic)
    {
        return await Task.Run(() => GatherUpdatedFields(existingFields, fileType));
    }

    /// <summary>
    /// Generic method to gather updated field values from user input.
    /// Works for both Excel and PDF scenarios.
    /// </summary>
    /// <param name="existingFields">Dictionary of current field values</param>
    /// <param name="fileType">Type of file being processed (for display messages)</param>
    /// <returns>Dictionary of updated field values</returns>
    public Dictionary<string, string> GatherUpdatedFields(
        Dictionary<string, string> existingFields,
        FileType fileType = FileType.Generic)
    {
        var fieldValues = new Dictionary<string, string>();
        string? dobValue = existingFields.TryGetValue("DOB", out string? value) ? value : null;
        string? ageFieldName = null;

        var fileTypeName = fileType switch
        {
            FileType.Excel => "Excel",
            FileType.PDF => "PDF form",
            _ => "form"
        };

        AnsiConsole.MarkupLine($"[yellow]Review and update {fileTypeName} fields:[/]");

        foreach (var (fieldName, currentValue) in existingFields)
        {
            string newValue = currentValue;

            if (!AnsiConsole.Confirm(
                $"Field: [green]{fieldName}[/] | Current Value: [yellow]{currentValue}[/] | Update?"))
            {
                fieldValues[fieldName] = newValue;
                continue;
            }

            newValue = fieldName.ToLowerInvariant() switch
            {
                "name" => PromptForName(currentValue),
                "surname" => PromptForSurname(currentValue),
                "age" => HandleAgeField(ref ageFieldName, fieldName, currentValue),
                "sex" => PromptForSex(),
                "colour" => PromptForColour(),
                "wanted" => PromptForWanted(), // PDF-specific field
                _ when fieldName.Contains("dob", StringComparison.OrdinalIgnoreCase) =>
                    dobValue = PromptForDob(currentValue),
                _ => PromptForGeneric(currentValue, fieldName),
            };

            fieldValues[fieldName] = newValue ?? string.Empty;
        }

        // Recalculate age from DOB if possible
        RecalculateAge(fieldValues, ageFieldName, dobValue, existingFields);

        return fieldValues;
    }

    /// <summary>
    /// Async version of GetFilePath
    /// </summary>
    public async Task<string> GetFilePathAsync(string defaultPath, FileType fileType = FileType.Generic)
    {
        return await Task.Run(() => GetFilePath(defaultPath, fileType));
    }

    /// <summary>
    /// Gets file path from user with appropriate prompt based on file type
    /// </summary>
    public string GetFilePath(string defaultPath, FileType fileType = FileType.Generic)
    {
        var promptText = fileType switch
        {
            FileType.Excel => "\nEnter the path to the Excel file (or press Enter for default):",
            FileType.PDF => "\nEnter the path to the PDF form file (or press Enter for default):",
            _ => "\nEnter the file path (or press Enter for default):"
        };

        return AnsiConsole.Ask<string>(promptText, defaultPath);
    }

    private void RecalculateAge(
        Dictionary<string, string> fieldValues,
        string? ageFieldName,
        string? dobValue,
        Dictionary<string, string> existingFields)
    {
        ageFieldName ??= existingFields.Keys.FirstOrDefault(k =>
            k.Equals("age", StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(ageFieldName) && dobValue != null)
        {
            int? calculatedAge = FieldValidator.CalculateAge(dobValue);
            if (calculatedAge.HasValue)
            {
                fieldValues[ageFieldName] = calculatedAge.Value.ToString();
                _notificationService.ShowSuccess($"Calculated age from DOB: {calculatedAge}");
            }
        }
    }

    private string HandleAgeField(ref string? ageFieldName, string fieldName, string currentValue)
    {
        _notificationService.ShowInfo("Age is autocalculated");
        ageFieldName = fieldName;
        return currentValue;
    }

    private string PromptForName(string currentValue) =>
        AnsiConsole.Prompt(
            new TextPrompt<string>("Enter the updated Name:")
                .DefaultValue(currentValue)
                .AllowEmpty()
        );

    private string PromptForSurname(string currentValue) =>
        AnsiConsole.Prompt(
            new TextPrompt<string>("Enter the updated Surname:")
                .DefaultValue(currentValue)
                .AllowEmpty()
        );

    private string PromptForDob(string currentValue) =>
        AnsiConsole.Prompt(
            new TextPrompt<string>("Enter Date of Birth (dd-MM-yyyy):")
                .DefaultValue(currentValue)
                .Validate(date =>
                    FieldValidator.IsValidDate(date)
                        ? ValidationResult.Success()
                        : ValidationResult.Error("Invalid date format. Use dd-MM-yyyy.")
                )
        );

    private string PromptForSex() =>
        AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select sex:")
                .AddChoices("Male", "Female", "Other")
        );

    private string PromptForColour() =>
        AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select colour:")
                .AddChoices("White", "Black", "Asian", "African", "Other")
        );

    private string PromptForWanted() =>
        AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Is wanted?")
                .AddChoices("Yes", "No")
        );

    private string PromptForGeneric(string currentValue, string fieldName) =>
        AnsiConsole.Prompt(
            new TextPrompt<string>($"Enter the updated value for {fieldName}:")
                .DefaultValue(currentValue)
                .AllowEmpty()
        );
}