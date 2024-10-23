using InvoiceAPI.Models;
using InvoiceAPI.Templates;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;

public static class InvoiceGenerator
{
    public static string GenerateInvoice(InvoiceData invoiceData, IInvoiceTemplate template)
    {
        string wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        if (!Directory.Exists(wwwrootPath))
        {
            Directory.CreateDirectory(wwwrootPath);
        }

        string fileName = $"fatura_{invoiceData.InvoiceNumber}.pdf";
        string filePath = Path.Combine(wwwrootPath, fileName);

        Document.Create(container =>
        {
            template.Compose(container, invoiceData);
        }).GeneratePdf(filePath);

        return filePath;
    }
}
