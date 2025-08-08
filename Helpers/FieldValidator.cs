// Ignore Spelling: Validator

using System.Globalization;

namespace ExcelReader.RyanW84.Helpers;

/// <summary>
/// Unified field validator for both Excel and PDF field validation.
/// Consolidates duplicate functionality from ExcelFieldValidator and PdfFieldValidator.
/// </summary>
public static class FieldValidator
{
    /// <summary>
    /// Validates if a date string is in the correct dd-MM-yyyy format
    /// </summary>
    /// <param name="date">The date string to validate</param>
    /// <returns>True if the date is valid, false otherwise</returns>
    public static bool IsValidDate(string date)
    {
        return DateTime.TryParseExact(
            date,
            "dd-MM-yyyy",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out _
        );
    }

    /// <summary>
    /// Calculates age based on date of birth string
    /// </summary>
    /// <param name="dobValue">Date of birth in dd-MM-yyyy format</param>
    /// <returns>Calculated age in years, or null if invalid DOB</returns>
    public static int? CalculateAge(string? dobValue)
    {
        if (!string.IsNullOrEmpty(dobValue) && DateTime.TryParseExact(
                dobValue,
                "dd-MM-yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var dob
            ))
        {
            var today = DateTime.Today;
            var age = today.Year - dob.Year;
            if (dob > today.AddYears(-age))
                age--;
            return age;
        }
        return null;
    }
}