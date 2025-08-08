using iText.Forms;
using iText.Forms.Fields;
using iText.Kernel.Pdf;
using ExcelReader.RyanW84.Abstractions.FileOperations.Readers;

namespace ExcelReader.RyanW84.Services;

public class ReadFromPdfForm : IPdfFormReader
{
    public async Task<Dictionary<string, string>> ReadFormFieldsAsync(string filePath)
    {
        return await Task.Run(() =>
        {
            var result = new Dictionary<string, string>();
            if (!File.Exists(filePath))
                return result;

            using var pdfReader = new PdfReader(filePath);
            using var pdfDoc = new PdfDocument(pdfReader);
            var form = PdfAcroForm.GetAcroForm(pdfDoc, false);
            if (form == null)
                return result;
            IDictionary<string, PdfFormField> fields = form.GetAllFormFields();
            foreach (var field in fields)
            {
                result[field.Key] = field.Value.GetValueAsString();
            }
            return result;
        });
    }

    // Keep synchronous version for backward compatibility
    public Dictionary<string, string> ReadFormFields(string filePath)
    {
        return ReadFormFieldsAsync(filePath).GetAwaiter().GetResult();
    }
}
