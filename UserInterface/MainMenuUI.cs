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

    // UI timing constants
    private const int InitializationDelayMs = 800;
    private const int MenuTransitionDelayMs = 500;
    private const int GoodbyeDelayMs = 1500;

    // Operation name constants
    private const string ExcelBeginnerImport = "Excel: Beginner Import";
    private const string ExcelDynamicImport = "Excel: Dynamic Import";
    private const string ExcelWrite = "Excel: Write";
    private const string CsvImport = "CSV: Import";
    private const string PdfImport = "PDF: Import";
    private const string PdfFormImport = "PDF: Form Import";
    private const string PdfFormWrite = "PDF: Form Write";
    private const string ExitOption = "Exit";

    // Common messages
    private const string SelectOperationPrompt = "[bold yellow]Select an operation:[/]";
    private const string WelcomeMessage = "Welcome to File Reader";
    private const string InitializedMessage = "File Reader application initialized successfully";
    private const string ChooseOperationsMessage = "Choose from various file import/export operations below";
    private const string MainMenuTitle = "File Reader - Main Menu";
    private const string GoodbyeTitle = "Thank You!";
    private const string ThankYouMessage = "Thank you for using File Reader!";
    private const string HaveGreatDayMessage = "Have a great day!";
    private const string OperationSummaryTitle = "[bold]Operation Summary[/]";
    private const string ReviewResultsMessage = "[bold yellow]Review the results above before continuing[/]";
    private const string PressKeyMessage = "[dim]Press any key to return to the main menu...[/]";
    private const string ReturningMessage = "[dim]Returning to main menu...[/]";

    public async Task ShowMenuAsync()
    {
        ShowWelcomeMessage();
        var exit = false;

        while (!exit)
        {
            ShowMainMenu();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(SelectOperationPrompt)
                    .AddChoices(
                        [
                            ExcelBeginnerImport,
                            ExcelDynamicImport,
                            ExcelWrite,
                            CsvImport,
                            PdfImport,
                            PdfFormImport,
                            PdfFormWrite,
                            ExitOption,
                        ]
                    )
            );

            if (choice == ExitOption)
            {
                await ShowGoodbyeMessageAsync();
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
            await ShowOperationInitialization(choice);
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
        await WaitForUserInputAsync();
    }

    private async Task ShowOperationInitialization(string choice)
    {
        // Show operation is starting
        await AnsiConsole.Status()
            .StartAsync($"Initializing {choice}...", async ctx =>
            {
                ctx.Spinner(Spinner.Known.Star);
                ctx.SpinnerStyle(Style.Parse("green"));
                await Task.Delay(InitializationDelayMs); // Brief pause to show initialization
            });

        _notificationService.ShowInfo($"Starting operation: {choice}");
        AnsiConsole.WriteLine();
    }

    private void ShowWelcomeMessage()
    {
        Console.Clear();
        AnsiConsole.Write(new Rule($"[bold green]{WelcomeMessage}[/]").RuleStyle("green").Centered());
        AnsiConsole.WriteLine();
        _notificationService.ShowInfo(InitializedMessage);
        _notificationService.ShowInfo(ChooseOperationsMessage);
        AnsiConsole.WriteLine();
    }

    private void ShowMainMenu()
    {
        Console.Clear();
        AnsiConsole.Write(new Rule(MainMenuTitle).RuleStyle("yellow").Centered());
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
        ExcelBeginnerImport => "This operation will read data from a basic Excel file and import it to the database.",
        ExcelDynamicImport => "This operation will analyze Excel file structure and create dynamic database tables.",
        ExcelWrite => "This operation will update existing Excel files with data from the database.",
        CsvImport => "This operation will read CSV files and create corresponding database tables.",
        PdfImport => "This operation will extract tabular data from PDF documents.",
        PdfFormImport => "This operation will read data from fillable PDF form fields.",
        PdfFormWrite => "This operation will write data to PDF form fields and update the database.",
        _ => string.Empty
    };

    private void ShowOperationSummary(string choice, bool success, DateTime startTime)
    {
        var duration = DateTime.Now - startTime;

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule(OperationSummaryTitle).RuleStyle("white").Centered());
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

    private async Task WaitForUserInputAsync()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule().RuleStyle("dim"));

        AnsiConsole.MarkupLine(ReviewResultsMessage);
        AnsiConsole.MarkupLine(PressKeyMessage);

        await Task.Run(() => Console.ReadKey(true));

        // Brief transition message
        AnsiConsole.MarkupLine(ReturningMessage);
        await Task.Delay(MenuTransitionDelayMs);
    }

    private async Task ShowGoodbyeMessageAsync()
    {
        Console.Clear();
        AnsiConsole.Write(new Rule($"[bold green]{GoodbyeTitle}[/]").RuleStyle("green").Centered());
        AnsiConsole.WriteLine();
        _notificationService.ShowSuccess(ThankYouMessage);
        _notificationService.ShowInfo(HaveGreatDayMessage);
        AnsiConsole.WriteLine();
        await Task.Delay(GoodbyeDelayMs);
    }

    private async Task ExecuteOperation(string choice)
    {
        switch (choice)
        {
            case ExcelBeginnerImport:
                _notificationService.ShowInfo("Preparing to import Excel data using beginner mode...");
                await _excelBeginnerController.AddDataFromExcel();
                break;
            case ExcelDynamicImport:
                _notificationService.ShowInfo("Preparing to import Excel data with dynamic schema detection...");
                await _anyExcelReadController.AddDynamicDataFromExcel();
                break;
            case ExcelWrite:
                _notificationService.ShowInfo("Preparing to write data to Excel file...");
                await _excelWriteController.UpdateExcelAndDatabaseAsync();
                break;
            case CsvImport:
                _notificationService.ShowInfo("Preparing to import CSV data...");
                await _csvController.ImportCsvAsync();
                break;
            case PdfImport:
                _notificationService.ShowInfo("Preparing to extract data from PDF tables...");
                await _pdfController.AddDataFromPdf();
                break;
            case PdfFormImport:
                _notificationService.ShowInfo("Preparing to read PDF form data...");
                await _pdfFormController.ImportDataFromPdfForm();
                break;
            case PdfFormWrite:
                _notificationService.ShowInfo("Preparing to write data to PDF form...");
                await _pdfFormWriteController.UpdatePdfFormAndDatabaseAsync();
                break;
        }
    }
}
