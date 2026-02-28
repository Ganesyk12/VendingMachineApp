using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using VendingMachineApp.Models;

namespace VendingMachineApp.Helpers
{
    public static class ReceiptPdfGenerator
    {
        public static byte[] Generate(UserTransaction trx)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(header => ComposeHeader(header, trx));
                    page.Content().Element(content => ComposeContent(content, trx));
                    page.Footer().Element(ComposeFooter);
                });
            }).GeneratePdf();
        }

        private static void ComposeHeader(IContainer container, UserTransaction trx)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("Transaction Receipt").FontSize(16).SemiBold().FontColor(Colors.Blue.Darken2);
                    column.Item().Text($"Vending Apps").FontSize(12).FontColor(Colors.Grey.Medium);

                    column.Item().PaddingTop(10).Text(text =>
                    {
                        text.Span("Transaction Code: ").SemiBold();
                        text.Span(trx.TrxCode);
                    });
                    column.Item().Text(text =>
                    {
                        text.Span("Date: ").SemiBold();
                        text.Span(trx.Date.ToString("dd MMM yyyy HH:mm:ss"));
                    });
                    column.Item().Text(text =>
                    {
                        text.Span("Customer: ").SemiBold();
                        text.Span(trx.User?.UserBalance?.Name ?? "Guest");
                    });
                });
            });
        }

        private static void ComposeContent(IContainer container, UserTransaction trx)
        {
            container.PaddingVertical(1, Unit.Centimetre).Column(column =>
            {
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(20); // No
                        columns.RelativeColumn(3); // Product Name
                        columns.RelativeColumn(1); // Qty
                        columns.RelativeColumn(2); // Price
                        columns.RelativeColumn(2); // SubTotal
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("No");
                        header.Cell().Element(CellStyle).Text("Product");
                        header.Cell().Element(CellStyle).AlignCenter().Text("Qty");
                        header.Cell().Element(CellStyle).AlignRight().Text("Price");
                        header.Cell().Element(CellStyle).AlignRight().Text("SubTotal");

                        static IContainer CellStyle(IContainer container)
                        {
                            return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1)
                                .BorderColor(Colors.Black);
                        }
                    });

                    // Body
                    int i = 1;
                    if (trx.TransactionDetails != null)
                    {
                        foreach (var item in trx.TransactionDetails)
                        {
                            table.Cell().Element(CellStyle).Text(i.ToString());
                            table.Cell().Element(CellStyle).Text(item.Product?.Name ?? "Product");
                            table.Cell().Element(CellStyle).AlignCenter().Text(item.Quantity.ToString());
                            table.Cell().Element(CellStyle).AlignRight().Text($"Rp {item.Price:N0}");
                            table.Cell().Element(CellStyle).AlignRight().Text($"Rp {item.SubTotal:N0}");

                            static IContainer CellStyle(IContainer container)
                            {
                                return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3);
                            }

                            i++;
                        }
                    }
                });

                // Summary
                column.Item().PaddingTop(15).Row(row =>
                {
                    row.RelativeItem().AlignLeft().Text(text =>
                    {
                        text.Span(
                                "Bukti Transaksi ini digenerate otomatis oleh sistem\nsebagai informasi rincian transaksi Anda.")
                            .FontSize(8).FontColor(Colors.Grey.Medium);
                    });
                    row.RelativeItem().Column(sumCol =>
                    {
                        sumCol.Item().AlignRight().Text($"Total: Rp {trx.TotalAmount:N0}").FontSize(14).SemiBold();
                        sumCol.Item().AlignRight()
                            .Text($"Balance After Transaction: Rp {trx.BalanceAfterTransaction?.ToString("N0")}")
                            .FontSize(10).FontColor(Colors.Grey.Darken1);
                    });
                });
            });
        }
        private static void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(x => { x.Span("Thank you for shopping at Vending Apps.").FontSize(9); });
        }
    }
}
