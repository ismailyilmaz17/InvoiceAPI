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

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string fileName = $"fatura_{invoiceData.InvoiceNumber}_{timestamp}.pdf";
        string filePath = Path.Combine(wwwrootPath, fileName);

        // PDF içeriğini oluştur ve kaydet
        Document.Create(container =>
        {
            template.Compose(container, invoiceData); // Şablonun içeriğini doldur
        }).GeneratePdf(filePath);

        return filePath; // PDF dosya yolunu döndür
    }
}
