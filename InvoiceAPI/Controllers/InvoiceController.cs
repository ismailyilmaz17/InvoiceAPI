using InvoiceAPI.Models;
using InvoiceAPI.Templates;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        [HttpPost("invoice")]
        public IActionResult CreateAndDownloadInvoice([FromBody] InvoiceData invoiceData)
        {
            var template = new CompanyeInvoiceTemplate();  // İstediğiniz şablonu burada seçin
            string filePath = InvoiceGenerator.GenerateInvoice(invoiceData, template);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Fatura oluşturulamadı.");
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            string fileName = Path.GetFileName(filePath);
            return File(fileBytes, "application/pdf", fileName);
        }
    }
}
