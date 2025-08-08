using ExcelReader.RyanW84.Abstractions.Services;
using Spectre.Console;

namespace ExcelReader.RyanW84.Helpers;

/// <summary>
/// Notification service implementation following SOLID principles
/// </summary>
public class UserNotifier : INotificationService
{
    public void ShowSuccess(string message) => AnsiConsole.MarkupLine($"[green]{message}[/]");

    public void ShowError(string message) => AnsiConsole.MarkupLine($"[red]{message}[/]");

    public void ShowWarning(string message) => AnsiConsole.MarkupLine($"[yellow]{message}[/]");

    public void ShowInfo(string message) => AnsiConsole.MarkupLine(message);
}
