﻿using InvoiceAPI.Models;
using InvoiceAPI.Services;
using InvoiceAPI.Templates;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        [HttpPost("invoicepdf")]
        public IActionResult CreateAndDownloadInvoice([FromBody] InvoiceData invoiceData)
        {
            var template = new CompanyInvoiceTemplate();  // İstediğiniz şablonu burada seçin
            string filePath = InvoiceGenerator.GenerateInvoice(invoiceData, template);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Fatura oluşturulamadı.");
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            string fileName = Path.GetFileName(filePath);
            return File(fileBytes, "application/pdf", fileName);
        }
        [HttpPost("invoicexml")]
        public IActionResult CreateAndDownloadXmlInvoice([FromBody] InvoiceData invoiceData)
        {
            string xmlFilePath = XmlInvoiceGenerator.GenerateEArchiveXml(invoiceData);

            if (!System.IO.File.Exists(xmlFilePath))
            {
                return NotFound("XML faturası oluşturulamadı.");
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(xmlFilePath);
            string fileName = Path.GetFileName(xmlFilePath);
            return File(fileBytes, "application/xml", fileName);
        }
    }
}
