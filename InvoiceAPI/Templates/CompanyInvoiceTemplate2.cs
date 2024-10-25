using InvoiceAPI.Models;
using Newtonsoft.Json;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Data.Common;
using System.IO;
using System.Reflection.PortableExecutable;

namespace InvoiceAPI.Templates
{
    public class CompanyInvoiceTemplate2 : IInvoiceTemplate
    {
        private readonly CompanyInfo _companyInfo;

        public CompanyInvoiceTemplate2()
        {
            _companyInfo = GetCompanyInfo();
        }

        public void Compose(IDocumentContainer container, InvoiceData invoiceData)
        {
            decimal totalAmount = invoiceData.Items.Sum(item => item.Quantity * item.UnitPrice);
            decimal totalTax = invoiceData.Items.Sum(item => (item.Quantity * item.UnitPrice) * item.TaxRate / 100);
            decimal grandTotal = totalAmount + totalTax;

            container.Page(page =>
            {
                page.Margin(20);

                // Üst kısım: Firma bilgileri ve Logo
                page.Header().Row(row =>
                {
                    // Müşteri bilgileri sol tarafta
                    row.RelativeItem().Column(column =>
                    {
                        column.Item().Text("SAYIN").Bold();
                        column.Item().Text($"{invoiceData.CustomerInfo.CustomerName}");
                        column.Item().Text($"Vergi Dairesi: {invoiceData.CustomerInfo.TaxOffice}");
                        column.Item().Text($"TCKN: {invoiceData.CustomerInfo.TCKN}");
                        column.Item().Text($"E-Posta: {invoiceData.CustomerInfo.Email}");

                    });

                    // Sağ tarafta Logo ve Firma Bilgileri
                    row.ConstantItem(200).Column(column =>
                    {
                        column.Item().AlignCenter().AlignMiddle()
                            .Height(80)
                            .Image(Path.Combine(Directory.GetCurrentDirectory(), "data/images/logo.png"));

                        column.Item().Text(_companyInfo.CompanyName).FontSize(14).Bold();
                        column.Item().Text(_companyInfo.Address);
                        column.Item().Text($"E-Posta: {_companyInfo.Email}");
                        column.Item().Text($"Vergi Dairesi: {_companyInfo.TaxOffice}");
                        column.Item().Text($"Vergi No: {_companyInfo.TaxNumber}");

                    });
                });
                

                //column.Item().PaddingTop(5).Element(c => c.LineHorizontal(1).Background(Colors.Black));
                // İçerik kısmı: Endeks Bilgileri ve Fatura Kalemleri
                page.Content().Column(column =>
                {
                    column.Item().PaddingVertical(5).Element(c =>
                    {
                        c.Row(row =>
                        {
                            row.RelativeItem().Element(e =>
                            {
                                e.Height(1) // Çizgi kalınlığı
                                 .Background(Colors.Black); // Çizgi rengi
                            });
                        });
                    });
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
                                table.Cell().Border(1).Padding(2).AlignLeft().Text($"{invoiceData.IndexInfo.ActiveIndex:N3}").FontSize(7);

                                table.Cell().Border(1).Padding(2).AlignLeft().Text("T1 Endeks").FontSize(7);
                                table.Cell().Border(1).Padding(2).AlignLeft().Text($"{invoiceData.IndexInfo.T1Index:N3}").FontSize(7);

                                table.Cell().Border(1).Padding(2).AlignLeft().Text("T2 Endeks").FontSize(7);
                                table.Cell().Border(1).Padding(2).AlignLeft().Text($"{invoiceData.IndexInfo.T2Index:N3}").FontSize(7);

                                table.Cell().Border(1).Padding(2).AlignLeft().Text("T3 Endeks").FontSize(7);
                                table.Cell().Border(1).Padding(2).AlignLeft().Text($"{invoiceData.IndexInfo.T3Index:N3}").FontSize(7);
                            });
                        });
                    }

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

                        // Tablo satırları
                        int index = 1;
                        foreach (var item in invoiceData.Items)
                        {
                            decimal itemTotal = item.Quantity * item.UnitPrice;
                            decimal taxAmount = itemTotal * item.TaxRate / 100;
                            decimal itemTotalWithTax = itemTotal + taxAmount;

                            table.Cell().Element(CellStyle).AlignCenter().Text($"{index++}");
                            table.Cell().Element(CellStyle).AlignCenter().Text(item.Description);
                            table.Cell().Element(CellStyle).AlignCenter().Text($"{item.Quantity}");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{item.UnitPrice:C}");
                            table.Cell().Element(CellStyle).AlignRight().Text($"%{item.TaxRate}");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{taxAmount:C}");
                            table.Cell().Element(CellStyle).AlignRight().Text($"{itemTotalWithTax:C}");
                        }
                    });

                    // Alt toplam ve KDV bilgileri
                    column.Item().Element(c =>
                    {
                        c.AlignRight().PaddingTop(5).Width(150).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(505); // Etiketler için geniş sütun
                                columns.ConstantColumn(50); // Değerler için sabit genişlik
                            });

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

                    // Footer: IBAN bilgileri
                    page.Footer().Element(c =>
                    {
                        c.Padding(10).Border(1).Column(col =>
                        {
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().PaddingLeft(5).Text("   HALK BANK: TR55 001 2009 3010 0016 0000 90").FontSize(9);
                            });

                            col.Item().Row(row =>
                            {
                                row.RelativeItem().PaddingLeft(5).Text("   VAKIF BANK: TR86 0001 5800 7296 3803 95").FontSize(9);
                            });

                            col.Item().Element(e =>
                            {
                                e.AlignCenter().PaddingTop(5).Text("Ödeme tutarının yukarıda belirtilen IBAN numaralarına yatırılması rica olunur.").FontSize(9);
                            });
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
