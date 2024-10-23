using InvoiceAPI.Models;
using Newtonsoft.Json;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.IO;

namespace InvoiceAPI.Templates
{
    public class CompanyeInvoiceTemplate : IInvoiceTemplate
    {
        private readonly CompanyInfo _companyInfo;

        public CompanyeInvoiceTemplate()
        {
            _companyInfo = GetCompanyInfo();
        }

        public void Compose(IDocumentContainer container, InvoiceData invoiceData)
        {
            decimal totalAmount = invoiceData.Items.Sum(item => item.Total);
            decimal totalTax = invoiceData.Items.Sum(item => item.Tax);
            decimal grandTotal = totalAmount + totalTax;
            container.Page(page =>
            {
                page.Margin(20);

                // Sayfa başlığı ve logo
                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(column =>
                    {
                        column.Item().Text(_companyInfo.CompanyName);
                        column.Item().Text(_companyInfo.Address);
                        column.Item().Text($"E-Posta: {_companyInfo.Email}");
                        column.Item().Text($"Vergi Dairesi: {_companyInfo.TaxOffice}");
                        column.Item().Text($"Vergi No: {_companyInfo.TaxNumber}");

                        column.Item().Text(" "); // Boş satır
                        column.Item().AlignLeft().Element(c => c.Width(150).LineHorizontal(1)); // Çizgi
                    });

                    row.ConstantItem(120).Height(100).Element(container =>
                    {
                        container.AlignCenter().AlignMiddle().Image(Path.Combine(Directory.GetCurrentDirectory(), "data/images/logo.png"));
                    });
                });

                // İçerik kısmı

                page.Content().Column(column =>
                {
                    // Müşteri bilgileri
                    column.Item().Text("");
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Müşteri Bilgileri").Bold();
                            col.Item().Text(invoiceData.CustomerName);
                            col.Item().Text($"Fatura No: {invoiceData.InvoiceNumber}");
                            col.Item().Text($"Tarih: {invoiceData.InvoiceDate.ToShortDateString()}");
                            column.Item().Text("");
                            column.Item().AlignLeft().Element(c => c.Width(150).LineHorizontal(1)); // Çizgi
                        });
                    });

                    column.Item().Text("");
                    // Endeks Bilgileri
                    if (invoiceData.IndexInfo != null)
                    {
                        
                        column.Item().Element(c =>
                        {
                            c.AlignLeft().Width(120).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1); // Endeks Adı
                                    columns.RelativeColumn(1); // Endeks Değeri
                                });

                                table.Cell().Border(1).Padding(2).AlignLeft().Text("Aktif Endeks").FontSize(7);
                                table.Cell().Border(1).Padding(2).AlignLeft().Text($"{invoiceData.IndexInfo.AktifEndeks:N3}").FontSize(7);

                                table.Cell().Border(1).Padding(2).AlignLeft().Text("T1 Endeks").FontSize(7);
                                table.Cell().Border(1).Padding(2).AlignLeft().Text($"{invoiceData.IndexInfo.T1Endeks:N3}").FontSize(7);

                                table.Cell().Border(1).Padding(2).AlignLeft().Text("T2 Endeks").FontSize(7);
                                table.Cell().Border(1).Padding(2).AlignLeft().Text($"{invoiceData.IndexInfo.T2Endeks:N3}").FontSize(7);

                                table.Cell().Border(1).Padding(2).AlignLeft().Text("T3 Endeks").FontSize(7);
                                table.Cell().Border(1).Padding(2).AlignLeft().Text($"{invoiceData.IndexInfo.T3Endeks:N3}").FontSize(7);
                            });
                        });
                    }
                    column.Item().Text("");

                    // Sağ alttaki fatura bilgileri tablosu
                    column.Item().Element(c =>
                    {
                        c.AlignRight().Width(120).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1); // Sol sütun (Etiketler)
                                columns.RelativeColumn(1); // Sağ sütun (Değerler)
                            });

                            // Özelleştirme No
                            table.Cell().Border(1).Padding(2).AlignLeft().Text("Özelleştirme No").FontSize(7);
                            table.Cell().Border(1).Padding(2).AlignLeft().Text(invoiceData.InvoiceDetails.CustomizationNumber).FontSize(7);

                            // Senaryo
                            table.Cell().Border(1).Padding(2).AlignLeft().Text("Senaryo").FontSize(7);
                            table.Cell().Border(1).Padding(2).AlignLeft().Text(invoiceData.InvoiceDetails.Scenario).FontSize(7);

                            // Fatura Tipi
                            table.Cell().Border(1).Padding(2).AlignLeft().Text("Fatura Tipi").FontSize(7);
                            table.Cell().Border(1).Padding(2).AlignLeft().Text(invoiceData.InvoiceDetails.InvoiceType).FontSize(7);

                            // Fatura Tarihi
                            table.Cell().Border(1).Padding(2).AlignLeft().Text("Fatura Tarihi").FontSize(7);
                            table.Cell().Border(1).Padding(2).AlignLeft().Text(invoiceData.InvoiceDetails.InvoiceDate.ToShortDateString()).FontSize(7);

                            // Düzenleme Tarihi
                            table.Cell().Border(1).Padding(2).AlignLeft().Text("Düzenleme Tarihi").FontSize(7);
                            table.Cell().Border(1).Padding(2).AlignLeft().Text(invoiceData.InvoiceDetails.EditDate.ToShortDateString()).FontSize(7);
                        });
                    });

                    column.Item().Text("");

                    // Fatura kalemleri tablosu
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(40);  // Sıra No
                            columns.RelativeColumn(3);   // Mal/Hizmet Cinsi
                            columns.ConstantColumn(50);  // Miktar
                            columns.ConstantColumn(80);  // Birim Fiyat
                            columns.ConstantColumn(80);  // Ek Vergiler
                            columns.ConstantColumn(80);  // Ek Vergi Tutarı
                            columns.ConstantColumn(100); // Mal Hizmet Tutarı
                        });

                        // Tablo başlıkları
                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).AlignCenter().Text("Sıra No").Bold();
                            header.Cell().Element(CellStyle).AlignCenter().Text("Mal/Hizmet Cinsi").Bold();
                            header.Cell().Element(CellStyle).AlignCenter().Text("Miktar").Bold();
                            header.Cell().Element(CellStyle).AlignRight().Text("Birim Fiyat").Bold();
                            header.Cell().Element(CellStyle).AlignRight().Text("Ek Vergiler").Bold();
                            header.Cell().Element(CellStyle).AlignRight().Text("Ek Vergi Tutarı").Bold();
                            header.Cell().Element(CellStyle).AlignRight().Text("Mal Hizmet Tutarı").Bold();
                        });

                        int index = 1;

                        // Tablo satırları
                        foreach (var item in invoiceData.Items)
                        {
                            table.Cell().Element(CellStyle).AlignCenter().Text($"{index++}");
                            table.Cell().Element(CellStyle).AlignCenter().Text(item.Description);
                            table.Cell().Element(CellStyle).AlignCenter().Text($"{item.Quantity}");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{item.UnitPrice:C}");
                            table.Cell().Element(CellStyle).AlignRight().Text(item.TaxRate);
                            table.Cell().Element(CellStyle).AlignRight().Text($"{item.Tax:C}");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{item.Total:C}");
                        }
                    });

                    // Alt toplam ve KDV bilgileri tablosu
                    column.Item().Text("");
                    column.Item().Element(c =>
                    {
                        c.AlignRight().Width(150).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(505); // Etiketler için daha geniş sütun
                                columns.ConstantColumn(50); // Değerler için sabit genişlik
                            });

                            // Mal Hizmet Toplam Tutarı
                            table.Cell().Border(1).Padding(2).AlignLeft().Text("Mal Hizmet Toplam Tutarı").FontSize(7);
                            table.Cell().Border(1).Padding(2).AlignRight().Text($"{totalAmount:C}").FontSize(7);


                            // Hesaplanan KDV Tutarı
                            table.Cell().Border(1).Padding(2).AlignLeft().Text("Hesaplanan KDV Tutarı").FontSize(7);
                            table.Cell().Border(1).Padding(2).AlignRight().Text($"{totalTax:C}").FontSize(7);

                            // Toplam Ödenecek Tutar
                            table.Cell().Border(1).Padding(2).AlignLeft().Text("Toplam Ödenecek Tutar").Bold().FontSize(7);
                            table.Cell().Border(1).Padding(2).AlignRight().Text($"{grandTotal:C}").Bold().FontSize(7);
                        });
                    });

                });
            });
        }

        private static IContainer CellStyle(IContainer container) =>
            container.BorderBottom(1).PaddingVertical(5);

        private static CompanyInfo GetCompanyInfo()
        {
            string json = File.ReadAllText("Data/companyInfo.json");
            return JsonConvert.DeserializeObject<CompanyInfo>(json);
        }
    }
}
