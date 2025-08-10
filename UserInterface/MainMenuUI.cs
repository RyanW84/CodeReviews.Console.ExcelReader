// Ignore Spelling: pdf

using ExcelReader.RyanW84.Controller;
using Spectre.Console;
using ExcelReader.RyanW84.Abstractions.Services;

namespace ExcelReader.RyanW84.UserInterface;

public class MainMenuUI(
    ExcelWriteController excelWriteController,
    PdfFormWriteController pdfFormWriteController,
    CsvController csvController,
    AnyExcelReadController anyExcelReadController,
    ExcelBeginnerController excelBeginnerController,
    PdfTableController pdfController,
    PdfFormController pdfFormController,
    INotificationService notificationService
)
{
    private readonly ExcelWriteController _excelWriteController = excelWriteController;
    private readonly PdfFormWriteController _pdfFormWriteController = pdfFormWriteController;
    private readonly CsvController _csvController = csvController;
    private readonly AnyExcelReadController _anyExcelReadController = anyExcelReadController;
    private readonly ExcelBeginnerController _excelBeginnerController = excelBeginnerController;
    private readonly PdfTableController _pdfController = pdfController;
    private readonly PdfFormController _pdfFormController = pdfFormController;
    private readonly INotificationService _notificationService = notificationService;

    public async Task ShowMenuAsync()
    {
        ShowWelcomeMessage();
        var exit = false;

        while (!exit)
        {
            ShowMainMenu();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold yellow]Select an operation:[/]")
                    .AddChoices(
                        [
                            "Excel: Beginner Import",
                            "Excel: Dynamic Import", 
                            "Excel: Write",
                            "CSV: Import",
                            "PDF: Import",
                            "PDF: Form Import",
                            "PDF: Form Write",
                            "Exit",
                        ]
                    )
            );

            if (choice == "Exit")
            {
                ShowGoodbyeMessage();
                exit = true;
                continue;
            }

            await ProcessSelectedOperation(choice);
        }
    }

    private async Task ProcessSelectedOperation(string choice)
    {
        // Show operation startup
        Console.Clear();
        ShowOperationHeader(choice);
        
        var startTime = DateTime.Now;
        var operationSuccess = false;

        try
        {
            // Show operation is starting
            AnsiConsole.Status()
                .Start($"Initializing {choice}...", ctx =>
                {
                    ctx.Spinner(Spinner.Known.Star);
                    ctx.SpinnerStyle(Style.Parse("green"));
                    Thread.Sleep(800); // Brief pause to show initialization
                });

            _notificationService.ShowInfo($"Starting operation: {choice}");
            AnsiConsole.WriteLine();

            // Execute the selected operation
            await ExecuteOperation(choice);
            operationSuccess = true;
        }
        catch (Exception ex)
        {
            operationSuccess = false;
            AnsiConsole.WriteLine();
            _notificationService.ShowError($"Operation failed: {ex.Message}");
            
            // Show additional error details if available
            if (!string.IsNullOrEmpty(ex.InnerException?.Message))
            {
                _notificationService.ShowError($"Inner error: {ex.InnerException.Message}");
            }
        }

        // Show operation completion summary
        ShowOperationSummary(choice, operationSuccess, startTime);
        
        // Wait for user acknowledgment before returning to menu
        WaitForUserInput();
    }

    private void ShowWelcomeMessage()
    {
        Console.Clear();
        AnsiConsole.Write(new Rule("[bold green]Welcome to File Reader[/]").RuleStyle("green").Centered());
        AnsiConsole.WriteLine();
        _notificationService.ShowInfo("File Reader application initialized successfully");
        _notificationService.ShowInfo("Choose from various file import/export operations below");
        AnsiConsole.WriteLine();
    }

    private void ShowMainMenu()
    {
        Console.Clear();
        AnsiConsole.Write(new Rule("[yellow]File Reader - Main Menu[/]").RuleStyle("yellow").Centered());
        AnsiConsole.WriteLine();
        
        // Show available operations with descriptions
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Blue)
            .AddColumn("[bold]Operation[/]")
            .AddColumn("[bold]Description[/]");

        table.AddRow("Excel: Beginner Import", "Import data from Excel files with predefined structure");
        table.AddRow("Excel: Dynamic Import", "Import Excel files with flexible schema detection");
        table.AddRow("Excel: Write", "Update existing Excel files with database data");
        table.AddRow("CSV: Import", "Import CSV files with automatic table creation");
        table.AddRow("PDF: Import", "Extract tabular data from PDF documents");
        table.AddRow("PDF: Form Import", "Read data from fillable PDF forms");
        table.AddRow("PDF: Form Write", "Write data to PDF form fields");
        table.AddRow("Exit", "Close the application");

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    private void ShowOperationHeader(string choice)
    {
        AnsiConsole.Write(new Rule($"[bold cyan]Executing: {choice}[/]").RuleStyle("cyan").Centered());
        AnsiConsole.WriteLine();
        
        // Show operation-specific information
        var operationInfo = GetOperationDescription(choice);
        if (!string.IsNullOrEmpty(operationInfo))
        {
            AnsiConsole.MarkupLine($"[dim]{operationInfo}[/]");
            AnsiConsole.WriteLine();
        }
    }

    private string GetOperationDescription(string choice) => choice switch
    {
        "Excel: Beginner Import" => "This operation will read data from a basic Excel file and import it to the database.",
        "Excel: Dynamic Import" => "This operation will analyze Excel file structure and create dynamic database tables.",
        "Excel: Write" => "This operation will update existing Excel files with data from the database.",
        "CSV: Import" => "This operation will read CSV files and create corresponding database tables.",
        "PDF: Import" => "This operation will extract tabular data from PDF documents.",
        "PDF: Form Import" => "This operation will read data from fillable PDF form fields.",
        "PDF: Form Write" => "This operation will write data to PDF form fields and update the database.",
        _ => string.Empty
    };

    private void ShowOperationSummary(string choice, bool success, DateTime startTime)
    {
        var duration = DateTime.Now - startTime;
        
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold]Operation Summary[/]").RuleStyle("white").Centered());
        AnsiConsole.WriteLine();

        var summaryTable = new Table()
            .Border(TableBorder.Simple)
            .AddColumn("[bold]Property[/]")
            .AddColumn("[bold]Value[/]");

        summaryTable.AddRow("Operation", choice);
        summaryTable.AddRow("Status", success ? "[green]Completed Successfully[/]" : "[red]Failed[/]");
        summaryTable.AddRow("Duration", $"{duration.TotalSeconds:F2} seconds");
        summaryTable.AddRow("Completed At", DateTime.Now.ToString("HH:mm:ss"));

        AnsiConsole.Write(summaryTable);
        AnsiConsole.WriteLine();

        if (success)
        {
            _notificationService.ShowSuccess($"[green]Completed:[/] {choice} completed successfully!");
        }
        else
        {
            _notificationService.ShowError($"[red]Failed:[/] {choice} failed to complete.");
            _notificationService.ShowInfo("Please check the error messages above for details.");
        }
    }

    private void WaitForUserInput()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule().RuleStyle("dim"));
        
        AnsiConsole.MarkupLine("[bold yellow]Review the results above before continuing[/]");
        AnsiConsole.MarkupLine("[dim]Press any key to return to the main menu...[/]");
        
        Console.ReadKey(true);
        
        // Brief transition message
        AnsiConsole.MarkupLine("[dim]Returning to main menu...[/]");
        Thread.Sleep(500);
    }

    private void ShowGoodbyeMessage()
    {
        Console.Clear();
        AnsiConsole.Write(new Rule("[bold green]Thank You![/]").RuleStyle("green").Centered());
        AnsiConsole.WriteLine();
        _notificationService.ShowSuccess("Thank you for using File Reader!");
        _notificationService.ShowInfo("Have a great day!");
        AnsiConsole.WriteLine();
        Thread.Sleep(1500);
    }

    private async Task ExecuteOperation(string choice)
    {
        switch (choice)
        {
            case "Excel: Beginner Import":
                _notificationService.ShowInfo("Preparing to import Excel data using beginner mode...");
                await _excelBeginnerController.AddDataFromExcel();
                break;
            case "Excel: Dynamic Import":
                _notificationService.ShowInfo("Preparing to import Excel data with dynamic schema detection...");
                await _anyExcelReadController.AddDynamicDataFromExcel();
                break;
            case "Excel: Write":
                _notificationService.ShowInfo("Preparing to write data to Excel file...");
                await _excelWriteController.UpdateExcelAndDatabaseAsync();
                break;
            case "CSV: Import":
                _notificationService.ShowInfo("Preparing to import CSV data...");
                await _csvController.ImportCsvAsync();
                break;
            case "PDF: Import":
                _notificationService.ShowInfo("Preparing to extract data from PDF tables...");
                await _pdfController.AddDataFromPdf();
                break;
            case "PDF: Form Import":
                _notificationService.ShowInfo("Preparing to read PDF form data...");
                await _pdfFormController.ImportDataFromPdfForm();
                break;
            case "PDF: Form Write":
                _notificationService.ShowInfo("Preparing to write data to PDF form...");
                await _pdfFormWriteController.UpdatePdfFormAndDatabaseAsync();
                break;
        }
    }
}
