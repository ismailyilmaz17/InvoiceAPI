﻿using InvoiceAPI.Models;
using Newtonsoft.Json;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;

namespace InvoiceAPI.Templates
{
    public class CompanyInvoiceTemplate : IInvoiceTemplate
    {
        private readonly CompanyInfo _companyInfo;

        public CompanyInvoiceTemplate()
        {
            _companyInfo = GetCompanyInfo();
        }

        public void Compose(IDocumentContainer container, InvoiceData invoiceData)
        {
            // Mal Hizmet Toplam Tutarı, Toplam Vergi ve Genel Toplam Hesaplama
            decimal totalAmount = invoiceData.Items.Sum(item => item.Quantity * item.UnitPrice);
            decimal totalTax = invoiceData.Items.Sum(item => (item.Quantity * item.UnitPrice) * item.TaxRate / 100);
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

                        //column.Item().Text(" "); // Boş satır
                        column.Item().PaddingTop(5).AlignLeft().Element(c => c.Width(220).LineHorizontal(1)); // Çizgi
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
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("SAYIN").Bold();
                            col.Item().Text($"{invoiceData.CustomerInfo.CustomerName}");
                            col.Item().Text($"Vergi Dairesi: {invoiceData.CustomerInfo.TaxOffice}");
                            col.Item().Text($"TCKN: {invoiceData.CustomerInfo.TCKN}");
                            col.Item().Text($"E-Posta: {invoiceData.CustomerInfo.Email}");
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

                            header.Cell().Element(CellStyle).AlignCenter().Text("Sıra No").Bold().FontSize(10);
                            header.Cell().Element(CellStyle).AlignCenter().Text("Mal/Hizmet Cinsi").Bold().FontSize(10);
                            header.Cell().Element(CellStyle).AlignCenter().Text("Miktar").Bold().FontSize(10);
                            header.Cell().Element(CellStyle).AlignCenter().Text("Birim Fiyat").Bold().FontSize(10);
                            header.Cell().Element(CellStyle).AlignCenter().Text("Ek Vergiler").Bold().FontSize(10);
                            header.Cell().Element(CellStyle).AlignCenter().Text("Ek Vergi Tutarı").Bold().FontSize(10);
                            header.Cell().Element(CellStyle).AlignCenter().Text("Mal Hizmet Tutarı").Bold().FontSize(10);
                        });

                        int index = 1;

                        // Tablo satırları
                        foreach (var item in invoiceData.Items)
                        {
                            decimal itemTotal = item.Quantity * item.UnitPrice;
                            decimal taxAmount = itemTotal * item.TaxRate / 100;
                            decimal itemTotalWithTax = itemTotal + taxAmount;

                            table.Cell().Element(CellStyle).AlignCenter().Text($"{index++}").FontSize(9);
                            table.Cell().Element(CellStyle).AlignCenter().Text(item.Description).FontSize(9);
                            table.Cell().Element(CellStyle).AlignCenter().Text($"{item.Quantity}").FontSize(9);
                            table.Cell().Element(CellStyle).AlignCenter().Text($"{item.UnitPrice:C}").FontSize(9);
                            table.Cell().Element(CellStyle).AlignCenter().Text("%"+item.TaxRate.ToString("0")).FontSize(9);
                            table.Cell().Element(CellStyle).AlignCenter().Text($"{taxAmount:C}").FontSize(9);
                            table.Cell().Element(CellStyle).AlignCenter().Text($"{itemTotalWithTax:C}").FontSize(9);
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
                    //column.Item().Text("").PaddingTop(10);
                    page.Footer().Element(c =>
                    {
                        c.Padding(10).Border(1).Column(col =>
                        {
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().PaddingLeft(5).Text("HALK BANK: TR55 001 2009 3010 0016 0000 90").FontSize(9);
                            });

                            col.Item().Row(row =>
                            {
                                row.RelativeItem().PaddingLeft(5).Text("VAKIF BANK: TR86 0001 5800 7296 3803 95").FontSize(9);
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