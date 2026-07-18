// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using DesktopLamour.Features.HomePage.Sales.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Sales.Views;

public partial class SalesOrderPrintWindow : Window
{
    private SalesOrderResponseDto? _order;

    public SalesOrderPrintWindow()
    {
        InitializeComponent();
    }

    public void Initialize(SalesOrderResponseDto order, string? customerPhone, string? customerAddress)
    {
        _order = order;
        InvoiceViewer.Document = BuildInvoiceDocument(order, customerPhone, customerAddress);
    }

    private void PrintButton_Click(object sender, RoutedEventArgs e)
    {
        var document = InvoiceViewer.Document;
        if (document is null) return;

        var printDialog = new PrintDialog();
        if (printDialog.ShowDialog() != true) return;

        document.PageHeight  = printDialog.PrintableAreaHeight;
        document.PageWidth   = printDialog.PrintableAreaWidth;
        document.PagePadding = new Thickness(40);
        document.ColumnWidth = printDialog.PrintableAreaWidth;

        IDocumentPaginatorSource paginatorSource = document;
        printDialog.PrintDocument(paginatorSource.DocumentPaginator, $"Hóa đơn bán hàng {_order?.DocumentNumber}");
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    private static FlowDocument BuildInvoiceDocument(SalesOrderResponseDto order, string? customerPhone, string? customerAddress)
    {
        var doc = new FlowDocument
        {
            FontFamily  = new FontFamily("Segoe UI"),
            FontSize    = 13,
            PagePadding = new Thickness(24),
        };

        doc.Blocks.Add(new Paragraph(new Bold(new Run("CÔNG TY TNHH THƯƠNG MẠI DỊCH VỤ LAMOUR")) { FontSize = 15 })
        {
            TextAlignment = TextAlignment.Center,
            Margin        = new Thickness(0, 0, 0, 4),
        });
        doc.Blocks.Add(CenteredLine("Số 110/20/38 Đường số 30, Phường An Nhơn, TP Hồ Chí Minh."));
        doc.Blocks.Add(CenteredLine("Mã số thuế: 0319088143"));
        doc.Blocks.Add(CenteredLine("Tel: 0868858975 - Website: www.skincoachlamour.com"));
        doc.Blocks.Add(CenteredLine("Số tài khoản: 0071.0007.93865 - VCB - CN Tân Sơn Nhất"));

        // Title + invoice number
        var titleTable = new Table { Margin = new Thickness(0, 16, 0, 12) };
        titleTable.Columns.Add(new TableColumn());
        titleTable.Columns.Add(new TableColumn());
        var titleRow = new TableRow();

        titleRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run("HÓA ĐƠN BÁN HÀNG")) { FontSize = 20 })
        {
            TextAlignment = TextAlignment.Center,
        }));

        var invoiceNoPara = new Paragraph { TextAlignment = TextAlignment.Right };
        invoiceNoPara.Inlines.Add(new Run("Số HĐ: "));
        invoiceNoPara.Inlines.Add(new Run(order.DocumentNumber) { Foreground = Brushes.Red, FontWeight = FontWeights.Bold });
        titleRow.Cells.Add(new TableCell(invoiceNoPara));

        var titleGroup = new TableRowGroup();
        titleGroup.Rows.Add(titleRow);
        titleTable.RowGroups.Add(titleGroup);
        doc.Blocks.Add(titleTable);

        // Customer info
        doc.Blocks.Add(new Paragraph(new Run($"Tên khách hàng: {order.CustomerName}")) { FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 2) });
        doc.Blocks.Add(new Paragraph(new Run($"Điện thoại: {customerPhone}")) { Margin = new Thickness(0, 0, 0, 2) });
        doc.Blocks.Add(new Paragraph(new Run($"Địa chỉ: {customerAddress}")) { Margin = new Thickness(0, 0, 0, 2) });
        doc.Blocks.Add(new Paragraph(new Run($"PT giao hàng: {order.DeliveryMethod}      PT thanh toán: {order.PaymentMethod}")) { Margin = new Thickness(0, 0, 0, 8) });

        // Line items table
        var table = new Table { CellSpacing = 0, Margin = new Thickness(0, 8, 0, 8) };
        foreach (var width in new[] { 30, 220, 40, 90, 60, 100, 70, 100 })
            table.Columns.Add(new TableColumn { Width = new GridLength(width) });

        var rowGroup = new TableRowGroup();
        rowGroup.Rows.Add(HeaderRow("STT", "TÊN SẢN PHẨM", "SL", "ĐƠN GIÁ", "CK (%)", "THÀNH TIỀN", "THUẾ SUẤT", "TỔNG CỘNG"));

        int stt = 1;
        foreach (var line in order.Lines)
        {
            var lineTotal = line.Amount + line.TaxAmount;

            rowGroup.Rows.Add(DataRow(
                stt++.ToString(),
                line.ProductName,
                line.Quantity.ToString(),
                FormatMoney(line.UnitPrice),
                line.DiscountRate.ToString("0.##") + "%",
                FormatMoney(line.Amount),
                $"{line.TaxRate:0}%",
                FormatMoney(lineTotal)));
        }

        table.RowGroups.Add(rowGroup);
        doc.Blocks.Add(table);

        doc.Blocks.Add(new Paragraph(new Bold(new Run($"Tổng tiền thanh toán: {FormatMoney(order.GrandTotal)}")))
        {
            TextAlignment = TextAlignment.Right,
            FontSize      = 15,
            Margin        = new Thickness(0, 4, 0, 12),
        });

        if (!string.IsNullOrWhiteSpace(order.Notes))
            doc.Blocks.Add(new Paragraph(new Run($"Ghi chú đơn hàng: {order.Notes}")) { Margin = new Thickness(0, 0, 0, 12) });

        doc.Blocks.Add(new Paragraph(new Run($"Ngày {order.DocumentDate.Day:D2} Tháng {order.DocumentDate.Month:D2} Năm {order.DocumentDate.Year}"))
        {
            TextAlignment = TextAlignment.Right,
            Margin        = new Thickness(0, 16, 0, 40),
        });

        var signTable = new Table();
        for (int i = 0; i < 4; i++) signTable.Columns.Add(new TableColumn());
        var signGroup = new TableRowGroup();
        signGroup.Rows.Add(HeaderRow("Thủ kho", "Người nhận hàng", "Nhân viên giao hàng", "Người viết hóa đơn"));
        signTable.RowGroups.Add(signGroup);
        doc.Blocks.Add(signTable);

        return doc;
    }

    private static Paragraph CenteredLine(string text) =>
        new(new Run(text)) { TextAlignment = TextAlignment.Center, FontSize = 12, Margin = new Thickness(0) };

    private static TableRow HeaderRow(params string[] headers)
    {
        var row = new TableRow { Background = Brushes.WhiteSmoke };
        foreach (var h in headers)
        {
            row.Cells.Add(new TableCell(new Paragraph(new Bold(new Run(h))) { TextAlignment = TextAlignment.Center })
            {
                Padding         = new Thickness(4),
                BorderBrush     = Brushes.Black,
                BorderThickness = new Thickness(0.5),
            });
        }
        return row;
    }

    private static TableRow DataRow(params string[] values)
    {
        var row = new TableRow();
        foreach (var v in values)
        {
            row.Cells.Add(new TableCell(new Paragraph(new Run(v)) { TextAlignment = TextAlignment.Center })
            {
                Padding         = new Thickness(4),
                BorderBrush     = Brushes.Black,
                BorderThickness = new Thickness(0.5),
            });
        }
        return row;
    }

    private static string FormatMoney(decimal value) => value.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"));
}
