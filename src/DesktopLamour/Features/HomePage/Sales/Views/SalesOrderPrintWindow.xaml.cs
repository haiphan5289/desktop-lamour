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

    private static readonly int[] ProductTableColumnWidths = { 21, 152, 28, 62, 41, 69, 48, 69 };

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

        // Title + invoice number. 3 cột: đệm rỗng | tiêu đề (giữa) | Số HĐ — cột đệm bên trái
        // rộng bằng đúng cột "Số HĐ" bên phải để tiêu đề được bao đối xứng, căn giữa đúng theo
        // cả trang thay vì chỉ giữa nửa trang (2 cột bằng nhau trước đây làm tiêu đề bị lệch trái).
        const double invoiceNoColumnWidth = 140;
        var titleTable = new Table { Margin = new Thickness(0) };
        titleTable.Columns.Add(new TableColumn { Width = new GridLength(invoiceNoColumnWidth) });
        titleTable.Columns.Add(new TableColumn());
        titleTable.Columns.Add(new TableColumn { Width = new GridLength(invoiceNoColumnWidth) });
        var titleRow = new TableRow();

        titleRow.Cells.Add(new TableCell(new Paragraph()));

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
        content.Blocks.Add(new Paragraph(new Run($"Tên khách hàng: {order.CustomerName}")) { FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 2) });
        content.Blocks.Add(new Paragraph(new Run($"Điện thoại: {customerPhone}")) { Margin = new Thickness(0, 0, 0, 2) });
        content.Blocks.Add(new Paragraph(new Run($"Địa chỉ: {customerAddress}")) { Margin = new Thickness(0, 0, 0, 2) });
        // "PT giao hàng"/"PT thanh toán": dùng bảng 2 cột cố định thay vì nối chuỗi với khoảng
        // trắng cứng — trước đây vị trí "PT thanh toán" bị xô lệch tuỳ độ dài DeliveryMethod.
        var deliveryPaymentTable = new Table { CellSpacing = 0, Margin = new Thickness(0) };
        deliveryPaymentTable.Columns.Add(new TableColumn { Width = new GridLength(245) });
        deliveryPaymentTable.Columns.Add(new TableColumn { Width = new GridLength(245) });
        var deliveryPaymentRow = new TableRow();
        deliveryPaymentRow.Cells.Add(new TableCell(new Paragraph(new Run($"PT giao hàng: {order.DeliveryMethod}"))));
        deliveryPaymentRow.Cells.Add(new TableCell(new Paragraph(new Run($"PT thanh toán: {order.PaymentMethod}"))));
        var deliveryPaymentGroup = new TableRowGroup();
        deliveryPaymentGroup.Rows.Add(deliveryPaymentRow);
        deliveryPaymentTable.RowGroups.Add(deliveryPaymentGroup);
        content.Blocks.Add(deliveryPaymentTable);

        // Line items table
        var table = new Table { CellSpacing = 0, Margin = new Thickness(0) };
        foreach (var width in ProductTableColumnWidths)
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
                line.DiscountRate.ToString("N2", CultureInfo.GetCultureInfo("vi-VN")) + "%",
                FormatMoney(line.Amount),
                $"{line.TaxRate:0}%",
                FormatMoney(lineTotal)));
        }

        // Tổng tiền thanh toán + Ghi chú đơn hàng — thêm làm 2 hàng cuối của CÙNG bảng sản phẩm
        // (ColumnSpan hết các cột) thay vì 3 Table riêng biệt, để mép các hàng liền nhau, không
        // có khoảng cách/border đôi giữa bảng sản phẩm và 2 hàng này.
        var totalPara = new Paragraph(new Bold(new Run($"Tổng tiền thanh toán : {FormatMoney(order.GrandTotal)}")))
        {
            TextAlignment = TextAlignment.Right,
            FontSize      = 11,
            Margin        = new Thickness(0),
        };
        var totalRow = new TableRow();
        totalRow.Cells.Add(new TableCell(totalPara)
        {
            ColumnSpan      = ProductTableColumnWidths.Length,
            BorderBrush     = Brushes.Black,
            BorderThickness = new Thickness(0.5),
            Padding         = new Thickness(5, 4, 5, 4),
        });
        rowGroup.Rows.Add(totalRow);

        // Ghi chú đơn hàng — always rendered as a bordered row, empty or not.
        var notePara = new Paragraph { Margin = new Thickness(0) };
        notePara.Inlines.Add(new Bold(new Run("GHI CHÚ ĐƠN HÀNG: ")));
        notePara.Inlines.Add(new Run(order.Notes ?? string.Empty));
        var noteRow = new TableRow();
        noteRow.Cells.Add(new TableCell(notePara)
        {
            ColumnSpan      = ProductTableColumnWidths.Length,
            BorderBrush     = Brushes.Black,
            BorderThickness = new Thickness(0.5),
            Padding         = new Thickness(5, 3, 5, 3),
        });
        rowGroup.Rows.Add(noteRow);

        table.RowGroups.Add(rowGroup);
        content.Blocks.Add(table);

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

        // FlowDocumentScrollViewer (dùng cho preview trong app) chỉ dùng PageWidth để giới hạn bề
        // ngang — chiều cao tự co theo đúng nội dung, KHÔNG tự kéo nền trắng ra đủ PageHeight như
        // khi in thật (in dùng DocumentPaginator, có tôn trọng PageHeight). Hóa đơn ít dòng sản
        // phẩm vì vậy nhìn giống hình chữ nhật nằm ngang thay vì tờ giấy đứng. Chèn 1 spacer vô
        // hình ở cuối, cao bằng phần còn thiếu để bù đủ 1 trang — chỉ là ước lượng gần đúng
        // (Block/TableCell không expose được chiều cao đã render thật), không ảnh hưởng gì tới
        // hóa đơn đã dài hơn 1 trang (kẹp về 0, không âm).
        var estimatedContentHeight = EstimateContentHeight(order.Lines.Count);
        var spacerHeight = Math.Max(0, A5PageHeight - estimatedContentHeight);
        content.Blocks.Add(new BlockUIContainer(new Border { MinHeight = spacerHeight }));

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

    // Ước lượng thô chiều cao (DIU) của toàn bộ nội dung hóa đơn theo số dòng sản phẩm — dùng để
    // tính spacer bù đủ 1 trang cho preview (xem chỗ gọi). Không cần chính xác tuyệt đối, chỉ cần
    // đủ gần để hóa đơn ngắn không còn nhìn như hình chữ nhật nằm ngang.
    private static double EstimateContentHeight(int lineCount)
    {
        const double header          = 85;  // logo + 5 dòng thông tin công ty
        const double title           = 30;  // "HÓA ĐƠN BÁN HÀNG" + Số HĐ
        const double customerInfo    = 70;  // Tên KH/Điện thoại/Địa chỉ + PT giao hàng-thanh toán
        const double tableHeaderRow  = 24;
        const double perProductRow   = 28;  // xấp xỉ, dài hơn nếu tên sản phẩm wrap 2 dòng
        const double totalsRow       = 26;
        const double notesRow        = 26;
        const double dateAndSignature = 90; // dòng Ngày... + khoảng trống chữ ký có chủ đích
        const double framePadding    = 24;  // Padding(12) x2 của outer frame
        const double pagePadding     = 28;  // PagePadding(14) x2

        return header + title + customerInfo + tableHeaderRow + (perProductRow * lineCount)
            + totalsRow + notesRow + dateAndSignature + framePadding + pagePadding;
    }
}
