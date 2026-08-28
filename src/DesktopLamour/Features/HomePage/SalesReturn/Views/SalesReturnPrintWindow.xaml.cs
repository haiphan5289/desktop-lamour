// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Globalization;
using System.Linq;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DesktopLamour.Features.HomePage.SalesReturn.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.SalesReturn.Views;

// In "PHIẾU TRẢ LẠI HÀNG BÁN" — copy nguyên cấu trúc FlowDocument từ
// Sales/Views/SalesOrderPrintWindow.xaml.cs (khổ A5, cột bảng, layout chữ ký) theo yêu cầu "y hệt
// layout Hóa đơn bán hàng, chỉ đổi tiêu đề/nội dung". Khác biệt so với bản gốc:
// - Không có PT giao hàng/PT thanh toán (SalesReturn không có 2 field này) → thay bằng "Loại trả hàng".
// - Không có khái niệm Hàng khuyến mại/Đặt cọc/Trừ cọc trên dòng SalesReturnLineDto → DataRow luôn
//   hiển thị đủ cột, không cần nhánh ẩn cột như bản gốc.
// - Tổng tiền lấy thẳng order.TotalPayment (BE đã tính sẵn, không có Trừ Cọc cần trừ thêm như SalesOrder).
// - "Ghi chú" dùng Description (SalesReturnResponseDto không có field Notes riêng).
public partial class SalesReturnPrintWindow : Window
{
    private SalesReturnResponseDto? _return;

    public SalesReturnPrintWindow()
    {
        InitializeComponent();
    }

    public void Initialize(SalesReturnResponseDto salesReturn, string? customerPhone, string? customerAddress)
    {
        _return = salesReturn;
        InvoiceViewer.Document = BuildDocument(salesReturn, customerPhone, customerAddress);
    }

    private void PrintButton_Click(object sender, RoutedEventArgs e)
    {
        var document = InvoiceViewer.Document;
        if (document is null) return;

        var printDialog = new PrintDialog();
        printDialog.PrintTicket.PageMediaSize = new PageMediaSize(PageMediaSizeName.ISOA5);
        if (printDialog.ShowDialog() != true) return;

        // Force A5 regardless of what the selected printer reports as its printable area —
        // the document is laid out for A5, not whatever paper the printer defaulted to.
        document.PageHeight  = A5PageHeight;
        document.PageWidth   = A5PageWidth;
        document.PagePadding = new Thickness(14);
        document.ColumnWidth = A5PageWidth;

        IDocumentPaginatorSource paginatorSource = document;
        printDialog.PrintDocument(paginatorSource.DocumentPaginator, $"Phiếu trả lại hàng bán {_return?.DocumentNumber}");
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    private const double MmToDip = 96.0 / 25.4;
    private static readonly double A5PageWidth  = 148 * MmToDip;
    private static readonly double A5PageHeight = 210 * MmToDip;

    // Widths khớp nguyên bản SalesOrderPrintWindow (đã qua 4 vòng chỉnh để header không vỡ dòng —
    // xem lịch sử comment ở file gốc) — bảng dòng hàng ở đây dùng đúng 8 cột như bản gốc nên tái
    // dùng nguyên, không cần đoán lại.
    private static readonly int[] ProductTableColumnWidths = { 26, 84, 26, 62, 42, 82, 84, 84 };

    private static readonly SolidColorBrush OuterBorderBrush = new(Color.FromRgb(0x9D, 0xC1, 0xE0));

    private const int HeaderTextLineCount = 5;
    private const double HeaderLineHeight = 15;

    private static FlowDocument BuildDocument(
        SalesReturnResponseDto salesReturn, string? customerPhone, string? customerAddress)
    {
        var doc = new FlowDocument
        {
            FontFamily    = new FontFamily("Segoe UI"),
            FontSize      = 13,
            TextAlignment = TextAlignment.Left,
            Background    = Brushes.White,
            PagePadding   = new Thickness(16),
            PageWidth     = A5PageWidth,
            PageHeight    = A5PageHeight,
            ColumnWidth   = A5PageWidth,
        };

        var frame = new Table { CellSpacing = 0 };
        frame.Columns.Add(new TableColumn());
        var frameRow = new TableRow();
        var content = new TableCell
        {
            BorderBrush     = OuterBorderBrush,
            BorderThickness = new Thickness(1.2),
            Padding         = new Thickness(14),
        };
        frameRow.Cells.Add(content);
        var frameGroup = new TableRowGroup();
        frameGroup.Rows.Add(frameRow);
        frame.RowGroups.Add(frameGroup);
        doc.Blocks.Add(frame);

        // ── Header: logo (floated left) + company info (wraps beside it) ────
        var logoImage = new Image
        {
            Source  = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/lamour-logo.png")),
            Width   = 120,
            Height  = 40,
            Stretch = Stretch.Uniform,
            VerticalAlignment = VerticalAlignment.Top,
        };
        var logoContainer = new Border { Child = logoImage, MinHeight = HeaderTextLineCount * HeaderLineHeight };
        var logoFloater = new Floater
        {
            Width               = 135,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin              = new Thickness(0, 0, 10, 6),
        };
        logoFloater.Blocks.Add(new BlockUIContainer(logoContainer));

        var headerPara = new Paragraph { Margin = new Thickness(0, 0, 0, 10), LineHeight = HeaderLineHeight };
        headerPara.Inlines.Add(logoFloater);
        headerPara.Inlines.Add(new Bold(new Run("CÔNG TY TNHH THƯƠNG MẠI DỊCH VỤ LAMOUR")) { FontSize = 13 });
        headerPara.Inlines.Add(new LineBreak());
        headerPara.Inlines.Add(new Run("Số 110/20/38 Đường số 30, Phường An Nhơn, TP Hồ Chí Minh.") { FontSize = 12 });
        headerPara.Inlines.Add(new LineBreak());
        headerPara.Inlines.Add(new Run("Mã số thuế: 0319088143") { FontSize = 12 });
        headerPara.Inlines.Add(new LineBreak());
        headerPara.Inlines.Add(new Run("Tel: 0868858975 - Website: www.skincoachlamour.com") { FontSize = 12 });
        headerPara.Inlines.Add(new LineBreak());
        headerPara.Inlines.Add(new Run("Số tài khoản: 0071.0007.93865 - VCB - CN Tân Sơn Nhất") { FontSize = 12 });
        content.Blocks.Add(headerPara);

        // Title + số chứng từ — bọc trong Table 1-cột full-width để tránh Floater logo tràn vào
        // (xem giải thích chi tiết ở SalesOrderPrintWindow.xaml.cs, cùng cơ chế).
        const double titleTableWidth = 495;
        var titleTable = new Table { Margin = new Thickness(0, 4, 0, 0) };
        titleTable.Columns.Add(new TableColumn { Width = new GridLength(titleTableWidth) });
        var titleGroup = new TableRowGroup();

        var titlePara = new Paragraph(new Bold(new Run("PHIẾU TRẢ LẠI HÀNG BÁN")) { FontSize = 20 })
        {
            TextAlignment = TextAlignment.Center,
        };
        var titleRow = new TableRow();
        titleRow.Cells.Add(new TableCell(titlePara) { Padding = new Thickness(0, 0, 0, 2) });
        titleGroup.Rows.Add(titleRow);

        var documentNoPara = new Paragraph { TextAlignment = TextAlignment.Right, FontSize = 14 };
        documentNoPara.Inlines.Add(new Run("Số CT: "));
        documentNoPara.Inlines.Add(new Run(salesReturn.DocumentNumber) { Foreground = Brushes.Red, FontWeight = FontWeights.Bold });
        var documentNoRow = new TableRow();
        documentNoRow.Cells.Add(new TableCell(documentNoPara) { Padding = new Thickness(0, 0, 0, 10) });
        titleGroup.Rows.Add(documentNoRow);

        titleTable.RowGroups.Add(titleGroup);
        content.Blocks.Add(titleTable);

        // Customer info
        content.Blocks.Add(new Paragraph(new Run($"Tên khách hàng: {salesReturn.CustomerName}")) { FontWeight = FontWeights.Bold, FontSize = 14, Margin = new Thickness(0, 0, 0, 4) });
        if (!string.IsNullOrWhiteSpace(customerPhone))
            content.Blocks.Add(new Paragraph(new Run($"Điện thoại: {customerPhone}")) { Margin = new Thickness(0, 0, 0, 4) });
        content.Blocks.Add(new Paragraph(new Run($"Địa chỉ: {customerAddress}")) { Margin = new Thickness(0, 0, 0, 4) });

        // SalesReturn không có PT giao hàng/PT thanh toán như SalesOrder — thay bằng Loại trả hàng
        // (Giảm trừ công nợ / Trả lại tiền mặt), field thật sự có ý nghĩa với 1 phiếu trả hàng.
        var returnTypeLabel = salesReturn.ReturnType == 1 ? "Trả lại tiền mặt" : "Giảm trừ công nợ";
        content.Blocks.Add(new Paragraph(new Run($"Loại trả hàng: {returnTypeLabel}")) { Margin = new Thickness(0, 0, 0, 8) });

        // Line items table
        var table = new Table { CellSpacing = 0, Margin = new Thickness(0) };
        foreach (var width in ProductTableColumnWidths)
            table.Columns.Add(new TableColumn { Width = new GridLength(width) });

        var rowGroup = new TableRowGroup();
        rowGroup.Rows.Add(HeaderRow("STT", "TÊN SẢN PHẨM", "SL", "ĐƠN GIÁ", "CK (%)", "THÀNH TIỀN", "THUẾ SUẤT", "TỔNG CỘNG"));

        int stt = 1;
        foreach (var line in salesReturn.Lines)
        {
            var lineTotal = line.Amount + line.TaxAmount;
            rowGroup.Rows.Add(DataRow(
                stt++.ToString(),
                line.ProductName,
                line.Quantity.ToString(),
                FormatMoney(line.UnitPrice),
                line.DiscountRate.ToString("N2", CultureInfo.GetCultureInfo("vi-VN")) + "%",
                FormatMoney(line.Amount),
                $"{line.TaxRate:0}%",
                FormatMoney(lineTotal)));
        }

        // Tổng tiền — lấy thẳng salesReturn.TotalPayment (BE tính sẵn từ toàn bộ dòng hàng, không có
        // khoản trừ nào khác cần cộng/trừ thêm như SalesOrder có Trừ Cọc).
        var totalLabelPara = new Paragraph(new Bold(new Run("Tổng tiền thanh toán :")))
        {
            TextAlignment = TextAlignment.Right,
            FontSize      = 16,
            Margin        = new Thickness(0),
        };
        var totalValuePara = new Paragraph(new Bold(new Run(FormatMoney(salesReturn.TotalPayment))))
        {
            TextAlignment = TextAlignment.Right,
            FontSize      = 16,
            Margin        = new Thickness(0),
        };
        var totalRow = new TableRow();
        totalRow.Cells.Add(new TableCell(totalLabelPara)
        {
            ColumnSpan      = ProductTableColumnWidths.Length - 1,
            BorderBrush     = Brushes.Black,
            BorderThickness = new Thickness(0.5),
            Padding         = new Thickness(6, 6, 6, 6),
        });
        totalRow.Cells.Add(new TableCell(totalValuePara)
        {
            ColumnSpan      = 1,
            BorderBrush     = Brushes.Black,
            BorderThickness = new Thickness(0.5),
            Padding         = new Thickness(6, 6, 6, 6),
        });
        rowGroup.Rows.Add(totalRow);

        // Ghi chú — SalesReturnResponseDto không có field Notes riêng, dùng Description.
        var notePara = new Paragraph { Margin = new Thickness(0) };
        notePara.Inlines.Add(new Bold(new Run("GHI CHÚ: ")));
        notePara.Inlines.Add(new Run(salesReturn.Description ?? string.Empty));
        var noteRow = new TableRow();
        noteRow.Cells.Add(new TableCell(notePara)
        {
            ColumnSpan      = ProductTableColumnWidths.Length,
            BorderBrush     = Brushes.Black,
            BorderThickness = new Thickness(0.5),
            Padding         = new Thickness(6, 5, 6, 5),
        });
        rowGroup.Rows.Add(noteRow);

        table.RowGroups.Add(rowGroup);
        content.Blocks.Add(table);

        content.Blocks.Add(new Paragraph(new Run($"Ngày {salesReturn.DocumentDate.Day:D2} Tháng {salesReturn.DocumentDate.Month:D2} Năm {salesReturn.DocumentDate.Year}"))
        {
            TextAlignment = TextAlignment.Right,
            FontSize      = 13,
            Margin        = new Thickness(0, 8, 0, 22),
        });

        // Chữ ký — giữ nguyên 3/4 vai trò như SalesOrderPrintWindow; "Người viết hóa đơn" đổi thành
        // "Người viết phiếu" vì chứng từ này không phải hóa đơn.
        var signTable = new Table { CellSpacing = 0 };
        for (int i = 0; i < 4; i++) signTable.Columns.Add(new TableColumn());
        var signRow = new TableRow();
        foreach (var label in new[] { "Thủ kho", "Người nhận hàng", "Nhân viên giao hàng", "Người viết phiếu" })
        {
            signRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run(label))) { TextAlignment = TextAlignment.Center, FontSize = 13 })
            {
                Padding = new Thickness(3, 3, 3, 48),
            });
        }
        var signGroup = new TableRowGroup();
        signGroup.Rows.Add(signRow);
        signTable.RowGroups.Add(signGroup);
        content.Blocks.Add(signTable);

        var estimatedContentHeight = EstimateContentHeight(salesReturn.Lines.Count);
        var spacerHeight = Math.Max(0, A5PageHeight - estimatedContentHeight);
        content.Blocks.Add(new BlockUIContainer(new Border { MinHeight = spacerHeight }));

        return doc;
    }

    private const int ProductNameColumnIndex = 1;
    private const double HeaderFontSize = 10;

    private static TableRow HeaderRow(params string[] headers)
    {
        var row = new TableRow { Background = Brushes.WhiteSmoke };
        for (var i = 0; i < headers.Length; i++)
        {
            var alignment = i == ProductNameColumnIndex ? TextAlignment.Left : TextAlignment.Center;
            row.Cells.Add(new TableCell(new Paragraph(new Bold(new Run(headers[i]))) { TextAlignment = alignment, FontSize = HeaderFontSize })
            {
                Padding         = new Thickness(1, 6, 1, 6),
                BorderBrush     = Brushes.Black,
                BorderThickness = new Thickness(0.5),
            });
        }
        return row;
    }

    private static TableRow DataRow(params string[] values)
    {
        var row = new TableRow();
        for (var i = 0; i < values.Length; i++)
        {
            var alignment = i == ProductNameColumnIndex ? TextAlignment.Left : TextAlignment.Center;
            row.Cells.Add(new TableCell(new Paragraph(new Run(values[i])) { TextAlignment = alignment })
            {
                Padding         = new Thickness(1, 6, 1, 6),
                BorderBrush     = Brushes.Black,
                BorderThickness = new Thickness(0.5),
            });
        }
        return row;
    }

    private static string FormatMoney(decimal value) => value.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"));

    private static double EstimateContentHeight(int lineCount)
    {
        const double header          = 112;
        const double title           = 42;
        const double customerInfo    = 80;  // Tên KH/Điện thoại/Địa chỉ + Loại trả hàng (ngắn hơn bản gốc — không có 2 dòng PT giao hàng/thanh toán)
        const double tableHeaderRow  = 36;
        const double perProductRow   = 40;
        const double totalsRow       = 38;
        const double notesRow        = 38;
        const double dateAndSignature = 114;
        const double framePadding    = 28;
        const double pagePadding     = 32;

        return header + title + customerInfo + tableHeaderRow + (perProductRow * lineCount)
            + totalsRow + notesRow + dateAndSignature + framePadding + pagePadding;
    }
}
