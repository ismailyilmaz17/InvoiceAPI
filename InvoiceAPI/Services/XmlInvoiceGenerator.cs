using InvoiceAPI.Models;
using System.Xml.Serialization;

namespace InvoiceAPI.Services
{
    public static class XmlInvoiceGenerator
    {
        public static string GenerateEArchiveXml(InvoiceData invoiceData)
        {
            string wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            if (!Directory.Exists(wwwrootPath))
            {
                Directory.CreateDirectory(wwwrootPath);
            }

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string xmlFileName = $"eArsiv_{invoiceData.InvoiceNumber}_{timestamp}.xml";
            string xmlFilePath = Path.Combine(wwwrootPath, xmlFileName);

            var xmlSerializer = new XmlSerializer(typeof(InvoiceData));
            using (var writer = new StreamWriter(xmlFilePath))
            {
                xmlSerializer.Serialize(writer, invoiceData);
            }

            return xmlFilePath;
        }
    }

}
