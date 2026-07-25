// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Globalization;
using System.Linq;
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
        if (printDialog.ShowDialog() != true) return;

        document.PageHeight  = printDialog.PrintableAreaHeight;
        document.PageWidth   = printDialog.PrintableAreaWidth;
        document.PagePadding = new Thickness(40);
        document.ColumnWidth = printDialog.PrintableAreaWidth;

        IDocumentPaginatorSource paginatorSource = document;
        printDialog.PrintDocument(paginatorSource.DocumentPaginator, $"Hóa đơn bán hàng {_order?.DocumentNumber}");
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    private static readonly SolidColorBrush OuterBorderBrush = new(Color.FromRgb(0x9D, 0xC1, 0xE0));

    private static FlowDocument BuildInvoiceDocument(SalesOrderResponseDto order, string? customerPhone, string? customerAddress)
    {
        var doc = new FlowDocument
        {
            FontFamily  = new FontFamily("Segoe UI"),
            FontSize    = 13,
            PagePadding = new Thickness(24),
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
            Padding         = new Thickness(20),
        };
        frameRow.Cells.Add(content);
        var frameGroup = new TableRowGroup();
        frameGroup.Rows.Add(frameRow);
        frame.RowGroups.Add(frameGroup);
        doc.Blocks.Add(frame);

        // ── Header: logo (left) + company info (right) ──────────────────────
        var headerTable = new Table { CellSpacing = 0, Margin = new Thickness(0) };
        headerTable.Columns.Add(new TableColumn { Width = new GridLength(160) });
        headerTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
        var headerRow = new TableRow();

        var logoCell = new TableCell { Padding = new Thickness(0, 0, 14, 0) };
        var logoImage = new Image
        {
            Source              = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/lamour-logo.png")),
            Width               = 150,
            Height              = 49,
            Stretch             = Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        logoCell.Blocks.Add(new BlockUIContainer(logoImage));
        headerRow.Cells.Add(logoCell);

        var infoCell = new TableCell();
        infoCell.Blocks.Add(new Paragraph(new Bold(new Run("CÔNG TY TNHH THƯƠNG MẠI DỊCH VỤ LAMOUR")) { FontSize = 14 })
        {
            Margin = new Thickness(0, 0, 0, 2),
        });
        infoCell.Blocks.Add(LeftLine("Số 110/20/38 Đường số 30, Phường An Nhơn, TP Hồ Chí Minh."));
        infoCell.Blocks.Add(LeftLine("Mã số thuế: 0319088143"));
        infoCell.Blocks.Add(LeftLine("Tel: 0868858975 - Website: www.skincoachlamour.com"));
        infoCell.Blocks.Add(LeftLine("Số tài khoản: 0071.0007.93865 - VCB - CN Tân Sơn Nhất"));
        headerRow.Cells.Add(infoCell);

        var headerGroup = new TableRowGroup();
        headerGroup.Rows.Add(headerRow);
        headerTable.RowGroups.Add(headerGroup);
        content.Blocks.Add(headerTable);

        // Title + invoice number
        var titleTable = new Table { Margin = new Thickness(0) };
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
        content.Blocks.Add(titleTable);

        // Customer info
        content.Blocks.Add(new Paragraph(new Run($"Tên khách hàng: {order.CustomerName}")) { FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 2) });
        content.Blocks.Add(new Paragraph(new Run($"Điện thoại: {customerPhone}")) { Margin = new Thickness(0, 0, 0, 2) });
        content.Blocks.Add(new Paragraph(new Run($"Địa chỉ: {customerAddress}")) { Margin = new Thickness(0, 0, 0, 2) });
        content.Blocks.Add(new Paragraph(new Run($"PT giao hàng: {order.DeliveryMethod}      PT thanh toán: {order.PaymentMethod}")) { Margin = new Thickness(0) });

        // Line items table
        var table = new Table { CellSpacing = 0, Margin = new Thickness(0) };
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
        content.Blocks.Add(table);

        var totalPara = new Paragraph(new Bold(new Run($"Tổng tiền thanh toán : {FormatMoney(order.GrandTotal)}")))
        {
            TextAlignment = TextAlignment.Right,
            FontSize      = 15,
            Margin        = new Thickness(0),
        };

        var totalTable = new Table { CellSpacing = 0, Margin = new Thickness(0) };
        totalTable.Columns.Add(new TableColumn());
        var totalRow = new TableRow();
        totalRow.Cells.Add(new TableCell(totalPara)
        {
            BorderBrush     = Brushes.Black,
            BorderThickness = new Thickness(0.5),
            Padding         = new Thickness(6, 5, 6, 5),
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
            Padding         = new Thickness(6, 4, 6, 4),
        });
        var noteGroup = new TableRowGroup();
        noteGroup.Rows.Add(noteRow);
        noteTable.RowGroups.Add(noteGroup);
        content.Blocks.Add(noteTable);

        content.Blocks.Add(new Paragraph(new Run($"Ngày {order.DocumentDate.Day:D2} Tháng {order.DocumentDate.Month:D2} Năm {order.DocumentDate.Year}"))
        {
            TextAlignment = TextAlignment.Right,
            Margin        = new Thickness(0, 4, 0, 40),
        });

        var signTable = new Table();
        for (int i = 0; i < 4; i++) signTable.Columns.Add(new TableColumn());
        var signGroup = new TableRowGroup();
        signGroup.Rows.Add(HeaderRow("Thủ kho", "Người nhận hàng", "Nhân viên giao hàng", "Người viết hóa đơn"));
        signTable.RowGroups.Add(signGroup);
        content.Blocks.Add(signTable);

        return doc;
    }

    private static Paragraph LeftLine(string text) =>
        new(new Run(text)) { TextAlignment = TextAlignment.Left, FontSize = 12, Margin = new Thickness(0, 0, 0, 1) };

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
