using ExcelReader.RyanW84.Abstractions.FileOperations.Writers;
using ExcelReader.RyanW84.Abstractions.Services;
using iText.Forms;
using iText.Forms.Fields;
using iText.Kernel.Pdf;

namespace ExcelReader.RyanW84.Services;

public class WriteToPdfForm(INotificationService userNotifier) : IPdfFormWriter
{
    private readonly INotificationService _userNotifier = userNotifier;

	public async Task WriteFormFieldsAsync(string filePath, Dictionary<string, string> fieldValues)
    {
        await Task.Run(() => WriteFormFields(filePath, fieldValues));
    }

    // Keep synchronous version for backward compatibility
    public void WriteFormFields(string filePath, Dictionary<string, string> fieldValues)
    {
        if (!File.Exists(filePath))
        {
            _userNotifier.ShowError($"File not found: {filePath}");
            return;
        }

        // Open for modification
        using var pdfReader = new PdfReader(filePath);
        using var pdfWriter = new PdfWriter(filePath + ".tmp");
        using var pdfDoc = new PdfDocument(pdfReader, pdfWriter);
        var form = PdfAcroForm.GetAcroForm(pdfDoc, true);
        if (form == null)
        {
            _userNotifier.ShowError("No AcroForm found in PDF.");
            return;
        }
        var fields = form.GetAllFormFields();
        foreach (var kvp in fieldValues)
        {
            if (fields.TryGetValue(kvp.Key , out PdfFormField? field))
            {
				// Special handling for the "wanted" checkbox
				if (
                    kvp.Key.Equals("Wanted", StringComparison.OrdinalIgnoreCase)
                    && field is PdfButtonFormField field1
				)
                {
                    if (kvp.Value.Equals("Yes", StringComparison.OrdinalIgnoreCase))
                        field1.SetValue("Yes");
                    else
                        field1.SetValue("No");
                }
                else
                {
                    field.SetValue(kvp.Value);
                }
            }
        }

        pdfDoc.Close();
        pdfReader.Close();
        pdfWriter.Close();
        // Replace original file
        File.Delete(filePath);
        File.Move(filePath + ".tmp", filePath);
    }
}
