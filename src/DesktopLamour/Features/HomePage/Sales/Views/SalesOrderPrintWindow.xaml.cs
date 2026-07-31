// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Globalization;
using System.Linq;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
        printDialog.PrintTicket.PageMediaSize = new PageMediaSize(PageMediaSizeName.ISOA5);
        if (printDialog.ShowDialog() != true) return;

        // Force A5 regardless of what the selected printer reports as its printable area —
        // the invoice is laid out for A5, not whatever paper the printer defaulted to.
        document.PageHeight  = A5PageHeight;
        document.PageWidth   = A5PageWidth;
        document.PagePadding = new Thickness(14);
        document.ColumnWidth = A5PageWidth;

        IDocumentPaginatorSource paginatorSource = document;
        printDialog.PrintDocument(paginatorSource.DocumentPaginator, $"Hóa đơn bán hàng {_order?.DocumentNumber}");
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    private const double MmToDip = 96.0 / 25.4;
    private static readonly double A5PageWidth  = 148 * MmToDip;
    private static readonly double A5PageHeight = 210 * MmToDip;

    private static readonly SolidColorBrush OuterBorderBrush = new(Color.FromRgb(0x9D, 0xC1, 0xE0));

    private static FlowDocument BuildInvoiceDocument(SalesOrderResponseDto order, string? customerPhone, string? customerAddress)
    {
        var doc = new FlowDocument
        {
            FontFamily  = new FontFamily("Segoe UI"),
            FontSize    = 10,
            Background  = Brushes.White,
            PagePadding = new Thickness(14),
            PageWidth   = A5PageWidth,
            PageHeight  = A5PageHeight,
            ColumnWidth = A5PageWidth,
        };

        // Outer frame — everything below is added to `content` (the bordered cell),
        // not directly to `doc.Blocks`, so the whole invoice sits inside one border.
        var frame = new Table { CellSpacing = 0 };
        frame.Columns.Add(new TableColumn());
        var frameRow = new TableRow();
        var content = new TableCell
        {
            BorderBrush     = OuterBorderBrush,
            BorderThickness = new Thickness(1.2),
            Padding         = new Thickness(12),
        };
        frameRow.Cells.Add(content);
        var frameGroup = new TableRowGroup();
        frameGroup.Rows.Add(frameRow);
        frame.RowGroups.Add(frameGroup);
        doc.Blocks.Add(frame);

        // ── Header: logo (floated left) + company info (wraps beside it) ────
        // NOTE: a nested Table with a Star column here previously caused the
        // company info to render as one-character-per-line at the page's far
        // right edge — nested Table + Star column sizing is unreliable in
        // FlowDocument. Floater is the purpose-built mechanism for "image on
        // one side, text wraps around it" and doesn't have that failure mode.
        var logoImage = new Image
        {
            Source  = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/lamour-logo.png")),
            Width   = 110,
            Height  = 36,
            Stretch = Stretch.Uniform,
        };
        var logoFloater = new Floater
        {
            Width               = 125,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin              = new Thickness(0, 0, 10, 6),
        };
        logoFloater.Blocks.Add(new BlockUIContainer(logoImage));

        var headerPara = new Paragraph { Margin = new Thickness(0, 0, 0, 8) };
        headerPara.Inlines.Add(logoFloater);
        headerPara.Inlines.Add(new Bold(new Run("CÔNG TY TNHH THƯƠNG MẠI DỊCH VỤ LAMOUR")) { FontSize = 11 });
        headerPara.Inlines.Add(new LineBreak());
        headerPara.Inlines.Add(new Run("Số 110/20/38 Đường số 30, Phường An Nhơn, TP Hồ Chí Minh.") { FontSize = 9 });
        headerPara.Inlines.Add(new LineBreak());
        headerPara.Inlines.Add(new Run("Mã số thuế: 0319088143") { FontSize = 9 });
        headerPara.Inlines.Add(new LineBreak());
        headerPara.Inlines.Add(new Run("Tel: 0868858975 - Website: www.skincoachlamour.com") { FontSize = 9 });
        headerPara.Inlines.Add(new LineBreak());
        headerPara.Inlines.Add(new Run("Số tài khoản: 0071.0007.93865 - VCB - CN Tân Sơn Nhất") { FontSize = 9 });
        content.Blocks.Add(headerPara);

        // Title + invoice number
        var titleTable = new Table { Margin = new Thickness(0) };
        titleTable.Columns.Add(new TableColumn());
        titleTable.Columns.Add(new TableColumn());
        var titleRow = new TableRow();

        titleRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run("HÓA ĐƠN BÁN HÀNG")) { FontSize = 15 })
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
        content.Blocks.Add(titleTable);

        // Customer info
        content.Blocks.Add(new Paragraph(new Run($"Tên khách hàng: {order.CustomerName}")) { FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 2) });
        content.Blocks.Add(new Paragraph(new Run($"Điện thoại: {customerPhone}")) { Margin = new Thickness(0, 0, 0, 2) });
        content.Blocks.Add(new Paragraph(new Run($"Địa chỉ: {customerAddress}")) { Margin = new Thickness(0, 0, 0, 2) });
        content.Blocks.Add(new Paragraph(new Run($"PT giao hàng: {order.DeliveryMethod}      PT thanh toán: {order.PaymentMethod}")) { Margin = new Thickness(0) });

        // Line items table
        var table = new Table { CellSpacing = 0, Margin = new Thickness(0) };
        foreach (var width in new[] { 21, 152, 28, 62, 41, 69, 48, 69 })
            table.Columns.Add(new TableColumn { Width = new GridLength(width) });

        var rowGroup = new TableRowGroup();
        rowGroup.Rows.Add(HeaderRow("STT", "TÊN SẢN PHẨM", "SL", "ĐƠN GIÁ", "CK (%)", "THÀNH TIỀN", "THUẾ SUẤT", "TỔNG CỘNG"));

        int stt = 1;
        foreach (var line in order.Lines)
        {
            // Hàng khuyến mại: không hiển thị Đơn giá/CK/Thành tiền/Thuế suất/Tổng cộng (đều = 0).
            if (line.IsPromotion)
            {
                rowGroup.Rows.Add(DataRow(
                    stt++.ToString(),
                    $"{line.ProductName} (Hàng khuyến mãi không thu tiền)",
                    line.Quantity.ToString(),
                    "", "", "", "", ""));
                continue;
            }

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
        content.Blocks.Add(table);

        var totalPara = new Paragraph(new Bold(new Run($"Tổng tiền thanh toán : {FormatMoney(order.GrandTotal)}")))
        {
            TextAlignment = TextAlignment.Right,
            FontSize      = 11,
            Margin        = new Thickness(0),
        };

        var totalTable = new Table { CellSpacing = 0, Margin = new Thickness(0) };
        totalTable.Columns.Add(new TableColumn());
        var totalRow = new TableRow();
        totalRow.Cells.Add(new TableCell(totalPara)
        {
            BorderBrush     = Brushes.Black,
            BorderThickness = new Thickness(0.5),
            Padding         = new Thickness(5, 4, 5, 4),
        });
        var totalGroup = new TableRowGroup();
        totalGroup.Rows.Add(totalRow);
        totalTable.RowGroups.Add(totalGroup);
        content.Blocks.Add(totalTable);

        // Ghi chú đơn hàng — always rendered as a bordered row, empty or not.
        var notePara = new Paragraph { Margin = new Thickness(0) };
        notePara.Inlines.Add(new Bold(new Run("GHI CHÚ ĐƠN HÀNG: ")));
        notePara.Inlines.Add(new Run(order.Notes ?? string.Empty));

        var noteTable = new Table { CellSpacing = 0, Margin = new Thickness(0) };
        noteTable.Columns.Add(new TableColumn());
        var noteRow = new TableRow();
        noteRow.Cells.Add(new TableCell(notePara)
        {
            BorderBrush     = Brushes.Black,
            BorderThickness = new Thickness(0.5),
            Padding         = new Thickness(5, 3, 5, 3),
        });
        var noteGroup = new TableRowGroup();
        noteGroup.Rows.Add(noteRow);
        noteTable.RowGroups.Add(noteGroup);
        content.Blocks.Add(noteTable);

        content.Blocks.Add(new Paragraph(new Run($"Ngày {order.DocumentDate.Day:D2} Tháng {order.DocumentDate.Month:D2} Năm {order.DocumentDate.Year}"))
        {
            TextAlignment = TextAlignment.Right,
            Margin        = new Thickness(0, 4, 0, 20),
        });

        var signTable = new Table { CellSpacing = 0 };
        for (int i = 0; i < 4; i++) signTable.Columns.Add(new TableColumn());
        var signRow = new TableRow();
        foreach (var label in new[] { "Thủ kho", "Người nhận hàng", "Nhân viên giao hàng", "Người viết hóa đơn" })
        {
            signRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run(label))) { TextAlignment = TextAlignment.Center })
            {
                // Extra bottom padding leaves blank space below the label for an actual signature.
                Padding = new Thickness(3, 3, 3, 45),
            });
        }
        var signGroup = new TableRowGroup();
        signGroup.Rows.Add(signRow);
        signTable.RowGroups.Add(signGroup);
        content.Blocks.Add(signTable);

        return doc;
    }

    private static TableRow HeaderRow(params string[] headers)
    {
        var row = new TableRow { Background = Brushes.WhiteSmoke };
        foreach (var h in headers)
        {
            row.Cells.Add(new TableCell(new Paragraph(new Bold(new Run(h))) { TextAlignment = TextAlignment.Center })
            {
                Padding         = new Thickness(3),
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
                Padding         = new Thickness(3),
                BorderBrush     = Brushes.Black,
                BorderThickness = new Thickness(0.5),
            });
        }
        return row;
    }

    private static string FormatMoney(decimal value) => value.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"));
}
