using InvoiceAPI.Models;
using QuestPDF.Infrastructure;

namespace InvoiceAPI.Templates
{
    public interface IInvoiceTemplate
    {
        void Compose(IDocumentContainer container, InvoiceData invoiceData);
    }
}

