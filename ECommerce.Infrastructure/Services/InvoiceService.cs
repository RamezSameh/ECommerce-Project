using ECommerce.Application.Exceptions;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ECommerce.Infrastructure.Services;

/// <summary>
/// Generates a PDF invoice for an order using QuestPDF.
/// Requires the QuestPDF community license to be acknowledged at startup.
/// </summary>
public class InvoiceService
{
    private readonly AppDbContext _context;

    public InvoiceService(AppDbContext context) => _context = context;

    public async Task<byte[]> GenerateAsync(int orderId)
    {
        var order = await _context.Orders.AsNoTracking().Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new NotFoundException($"Order {orderId} not found");

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(col =>
                {
                    col.Item().Text("Invoice").FontSize(24).Bold();
                    col.Item().Text($"Order #{order.OrderNumber}").FontColor(Colors.Grey.Darken1);
                    col.Item().Text($"Date: {order.CreatedAt:g}");
                });

                page.Content().PaddingVertical(20).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(3);
                            cols.RelativeColumn();
                            cols.RelativeColumn();
                            cols.RelativeColumn();
                        });
                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text("Item").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text("Qty").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(6).AlignRight().Text("Price").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(6).AlignRight().Text("Subtotal").Bold();
                        });

                        foreach (var item in order.Items)
                        {
                            table.Cell().Padding(4).Text(item.ProductName);
                            table.Cell().Padding(4).Text(item.Quantity.ToString());
                            table.Cell().Padding(4).AlignRight().Text(item.UnitPrice.ToString("C"));
                            table.Cell().Padding(4).AlignRight().Text((item.UnitPrice * item.Quantity).ToString("C"));
                        }
                    });

                    col.Item().PaddingTop(16).AlignRight().Column(total =>
                    {
                        total.Item().Text($"Subtotal: {order.Subtotal:C}");
                        total.Item().Text($"Discount: -{order.Discount:C}");
                        total.Item().Text($"Tax: {order.Tax:C}");
                        total.Item().Text($"Shipping: {order.Shipping:C}");
                        total.Item().Text($"Total: {order.Total:C}").Bold();
                    });
                });
            });
        });

        return document.GeneratePdf();
    }
}