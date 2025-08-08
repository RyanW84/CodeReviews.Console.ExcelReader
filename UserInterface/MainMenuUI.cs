// Ignore Spelling: pdf

using ExcelReader.RyanW84.Controller;
using Spectre.Console;

namespace ExcelReader.RyanW84.UserInterface;

public class MainMenuUI(
    ExcelWriteController excelWriteController,
    PdfFormWriteController pdfFormWriteController,
    CsvController csvController,
    AnyExcelReadController anyExcelReadController,
    ExcelBeginnerController excelBeginnerController,
    PdfTableController pdfController,
    PdfFormController pdfFormController
)
{
    private readonly ExcelWriteController _excelWriteController = excelWriteController;
    private readonly PdfFormWriteController _pdfFormWriteController = pdfFormWriteController;
    private readonly CsvController _csvController = csvController;
    private readonly AnyExcelReadController _anyExcelReadController = anyExcelReadController;
    private readonly ExcelBeginnerController _excelBeginnerController = excelBeginnerController;
    private readonly PdfTableController _pdfController = pdfController;
    private readonly PdfFormController _pdfFormController = pdfFormController;

    public async Task ShowMenuAsync()
    {
        Console.Clear();
        AnsiConsole.Write(new Rule("[yellow]FileRead[/]").RuleStyle("yellow").Centered());
        var exit = false;

        while (!exit)
        {
            ShowHeader();

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
                exit = true;
                continue;
            }

            // Show operation header
            Console.Clear();
            ShowHeader();

            AnsiConsole.MarkupLine($"[bold cyan]Running: {choice}[/]");

            try
            {
                // Execute the selected operation
                await ExecuteOperation(choice);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]An error occurred: {ex.Message}[/]");
            }

            // Wait for user input before returning to menu
            AnsiConsole.MarkupLine("[dim]Press any key to return to the main menu...[/]");
            Console.ReadKey(true);
        }
    }

    private void ShowHeader()
    {
        Console.Clear();
        AnsiConsole.Write(new Rule("[yellow]File Reader[/]").RuleStyle("yellow").Centered());
    }

    private async Task ExecuteOperation(string choice)
    {
        switch (choice)
        {
            case "Excel: Beginner Import":
                await _excelBeginnerController.AddDataFromExcel();
                break;
            case "Excel: Dynamic Import":
                await _anyExcelReadController.AddDynamicDataFromExcel();
                break;
            case "Excel: Write":
                await _excelWriteController.UpdateExcelAndDatabaseAsync();
                break;
            case "CSV: Import":
                await _csvController.AddDataFromCsv();
                break;
            case "PDF: Import":
                await _pdfController.AddDataFromPdf();
                break;
            case "PDF: Form Import":
                await _pdfFormController.ImportDataFromPdfForm();
				break;
            case "PDF: Form Write":
                await _pdfFormWriteController.UpdatePdfFormAndDatabaseAsync();
                break;
        }
    }
}
