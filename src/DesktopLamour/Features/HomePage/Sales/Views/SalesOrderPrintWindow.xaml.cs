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

    public void Initialize(
        SalesOrderResponseDto order, string? customerPhone, string? customerAddress,
        decimal depositDeductionAmount = 0m)
    {
        _order = order;
        InvoiceViewer.Document = BuildInvoiceDocument(order, customerPhone, customerAddress, depositDeductionAmount);
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

    private static readonly int[] ProductTableColumnWidths = { 32, 141, 28, 62, 41, 69, 48, 69 };

    private static readonly SolidColorBrush OuterBorderBrush = new(Color.FromRgb(0x9D, 0xC1, 0xE0));

    // Số dòng text trong header (tên công ty + 4 dòng thông tin) và LineHeight tương ứng — dùng để
    // tính chiều cao vùng float của logo (xem BuildInvoiceDocument), phải khớp với số Run/LineBreak
    // thực tế thêm vào headerPara bên dưới nếu sau này đổi nội dung header.
    private const int HeaderTextLineCount = 5;
    private const double HeaderLineHeight = 15;

    private static FlowDocument BuildInvoiceDocument(
        SalesOrderResponseDto order, string? customerPhone, string? customerAddress,
        decimal depositDeductionAmount = 0m)
    {
        var doc = new FlowDocument
        {
            FontFamily  = new FontFamily("Segoe UI"),
            FontSize    = 11,
            Background  = Brushes.White,
            PagePadding = new Thickness(16),
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
            Padding         = new Thickness(14),
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
            Width   = 120,
            Height  = 40,
            Stretch = Stretch.Uniform,
            VerticalAlignment = VerticalAlignment.Top,
        };
        // Floater chỉ chiếm chỗ cao bằng nội dung bên trong nó (ảnh logo = 40px) — nhưng đoạn text
        // bên cạnh có 5 dòng × LineHeight 15 = 75px. Qua khỏi 40px của ảnh, FlowDocument coi như hết
        // vùng float nên dòng cuối ("Số tài khoản...") bị đẩy về căn lề trái toàn Paragraph thay vì
        // tiếp tục thụt vào ngang hàng các dòng phía trên → lệch trái. Bọc logo trong Border cao bằng
        // đúng tổng chiều cao text (HeaderTextLineCount × LineHeight) để vùng float phủ hết cả 5 dòng.
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
        headerPara.Inlines.Add(new Run("Số 110/20/38 Đường số 30, Phường An Nhơn, TP Hồ Chí Minh.") { FontSize = 10 });
        headerPara.Inlines.Add(new LineBreak());
        headerPara.Inlines.Add(new Run("Mã số thuế: 0319088143") { FontSize = 10 });
        headerPara.Inlines.Add(new LineBreak());
        headerPara.Inlines.Add(new Run("Tel: 0868858975 - Website: www.skincoachlamour.com") { FontSize = 10 });
        headerPara.Inlines.Add(new LineBreak());
        headerPara.Inlines.Add(new Run("Số tài khoản: 0071.0007.93865 - VCB - CN Tân Sơn Nhất") { FontSize = 10 });
        content.Blocks.Add(headerPara);

        // Title + invoice number. 3 cột: đệm rỗng | tiêu đề (giữa) | Số HĐ — cột đệm bên trái
        // rộng bằng đúng cột "Số HĐ" bên phải để tiêu đề được bao đối xứng, căn giữa đúng theo
        // cả trang thay vì chỉ giữa nửa trang (2 cột bằng nhau trước đây làm tiêu đề bị lệch trái).
        const double invoiceNoColumnWidth = 140;
        var titleTable = new Table { Margin = new Thickness(0, 4, 0, 10) };
        titleTable.Columns.Add(new TableColumn { Width = new GridLength(invoiceNoColumnWidth) });
        titleTable.Columns.Add(new TableColumn());
        titleTable.Columns.Add(new TableColumn { Width = new GridLength(invoiceNoColumnWidth) });
        var titleRow = new TableRow();

        titleRow.Cells.Add(new TableCell(new Paragraph()));

        titleRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run("HÓA ĐƠN BÁN HÀNG")) { FontSize = 18 })
        {
            TextAlignment = TextAlignment.Center,
        }));

        var invoiceNoPara = new Paragraph { TextAlignment = TextAlignment.Right, FontSize = 12 };
        invoiceNoPara.Inlines.Add(new Run("Số HĐ: "));
        invoiceNoPara.Inlines.Add(new Run(order.DocumentNumber) { Foreground = Brushes.Red, FontWeight = FontWeights.Bold });
        titleRow.Cells.Add(new TableCell(invoiceNoPara));

        var titleGroup = new TableRowGroup();
        titleGroup.Rows.Add(titleRow);
        titleTable.RowGroups.Add(titleGroup);
        content.Blocks.Add(titleTable);

        // Customer info
        content.Blocks.Add(new Paragraph(new Run($"Tên khách hàng: {order.CustomerName}")) { FontWeight = FontWeights.Bold, FontSize = 12, Margin = new Thickness(0, 0, 0, 4) });
        content.Blocks.Add(new Paragraph(new Run($"Điện thoại: {customerPhone}")) { Margin = new Thickness(0, 0, 0, 4) });
        content.Blocks.Add(new Paragraph(new Run($"Địa chỉ: {customerAddress}")) { Margin = new Thickness(0, 0, 0, 4) });
        // "PT giao hàng"/"PT thanh toán": dùng bảng 2 cột cố định thay vì nối chuỗi với khoảng
        // trắng cứng — trước đây vị trí "PT thanh toán" bị xô lệch tuỳ độ dài DeliveryMethod.
        var deliveryPaymentTable = new Table { CellSpacing = 0, Margin = new Thickness(0, 0, 0, 8) };
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

            // Dòng "Đặt cọc" (sản phẩm IsDepositProduct): Đơn giá/CK/Thuế suất vô nghĩa (số tiền
            // cọc nhập tay qua Thành tiền thủ công, không phải Quantity × UnitPrice) — để trống
            // giống cách dòng khuyến mại ẩn các cột không áp dụng.
            rowGroup.Rows.Add(DataRow(
                stt++.ToString(),
                line.ProductName,
                line.Quantity.ToString(),
                line.IsDepositProduct ? "" : FormatMoney(line.UnitPrice),
                line.IsDepositProduct ? "" : line.DiscountRate.ToString("N2", CultureInfo.GetCultureInfo("vi-VN")) + "%",
                FormatMoney(line.Amount),
                line.IsDepositProduct ? "" : $"{line.TaxRate:0}%",
                FormatMoney(lineTotal)));
        }

        // Trừ Cọc — không phải 1 SalesOrderLine thật (DepositDeduction là bản ghi riêng), nên
        // amount được truyền từ ngoài vào (dòng "Trừ cọc" đang có trên form lúc in) thay vì đọc
        // từ order.Lines. Chỉ hiển thị Thành tiền/Tổng cộng (âm, đỏ, trong ngoặc) — các cột khác
        // để trống vì không phải hàng hóa.
        if (depositDeductionAmount != 0m)
            rowGroup.Rows.Add(DepositDeductionRow(stt++, depositDeductionAmount));

        // Tổng tiền thanh toán + Ghi chú đơn hàng — thêm làm 2 hàng cuối của CÙNG bảng sản phẩm
        // (ColumnSpan hết các cột) thay vì 3 Table riêng biệt, để mép các hàng liền nhau, không
        // có khoảng cách/border đôi giữa bảng sản phẩm và 2 hàng này.
        //
        // KHÔNG dùng order.GrandTotal thẳng — nó KHÔNG trừ Trừ Cọc (Deposit/DepositDeduction là
        // record riêng, không phải SalesOrderLine, nên BE tính GrandTotal chỉ từ dòng hàng thật;
        // xem CreateSalesOrderUseCase.GrandTotal = lines.Sum(Amount + TaxAmount)). Tự tính lại từ
        // TotalAmount + TotalTaxAmount (2 field này luôn = tổng dòng hàng thật, khớp cả 2 nguồn gọi
        // hàm này: response thật từ BE lúc vừa Ghi sổ, và preview dựng tại chỗ từ form chưa lưu) rồi
        // trừ depositDeductionAmount — để tổng tiền in ra luôn khớp với dòng "Trừ Cọc" ngay phía trên.
        var netGrandTotal = order.TotalAmount + order.TotalTaxAmount - depositDeductionAmount;
        var totalPara = new Paragraph(new Bold(new Run($"Tổng tiền thanh toán : {FormatMoney(netGrandTotal)}")))
        {
            TextAlignment = TextAlignment.Right,
            FontSize      = 13,
            Margin        = new Thickness(0),
        };
        var totalRow = new TableRow();
        totalRow.Cells.Add(new TableCell(totalPara)
        {
            ColumnSpan      = ProductTableColumnWidths.Length,
            BorderBrush     = Brushes.Black,
            BorderThickness = new Thickness(0.5),
            Padding         = new Thickness(6, 6, 6, 6),
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
            Padding         = new Thickness(6, 5, 6, 5),
        });
        rowGroup.Rows.Add(noteRow);

        table.RowGroups.Add(rowGroup);
        content.Blocks.Add(table);

        content.Blocks.Add(new Paragraph(new Run($"Ngày {order.DocumentDate.Day:D2} Tháng {order.DocumentDate.Month:D2} Năm {order.DocumentDate.Year}"))
        {
            TextAlignment = TextAlignment.Right,
            FontSize      = 11,
            Margin        = new Thickness(0, 8, 0, 22),
        });

        var signTable = new Table { CellSpacing = 0 };
        for (int i = 0; i < 4; i++) signTable.Columns.Add(new TableColumn());
        var signRow = new TableRow();
        foreach (var label in new[] { "Thủ kho", "Người nhận hàng", "Nhân viên giao hàng", "Người viết hóa đơn" })
        {
            signRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run(label))) { TextAlignment = TextAlignment.Center, FontSize = 11 })
            {
                // Extra bottom padding leaves blank space below the label for an actual signature.
                Padding = new Thickness(3, 3, 3, 48),
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
        var estimatedContentHeight = EstimateContentHeight(order.Lines.Count + (depositDeductionAmount != 0m ? 1 : 0));
        var spacerHeight = Math.Max(0, A5PageHeight - estimatedContentHeight);
        content.Blocks.Add(new BlockUIContainer(new Border { MinHeight = spacerHeight }));

        return doc;
    }

    // Cột "TÊN SẢN PHẨM" luôn ở index 1 trong mọi hàng của bảng sản phẩm — căn trái (chuẩn hóa
    // đơn thông dụng), các cột còn lại (số lượng/tiền/%) căn giữa.
    private const int ProductNameColumnIndex = 1;

    private static TableRow HeaderRow(params string[] headers)
    {
        var row = new TableRow { Background = Brushes.WhiteSmoke };
        for (var i = 0; i < headers.Length; i++)
        {
            var alignment = i == ProductNameColumnIndex ? TextAlignment.Left : TextAlignment.Center;
            row.Cells.Add(new TableCell(new Paragraph(new Bold(new Run(headers[i]))) { TextAlignment = alignment })
            {
                Padding         = new Thickness(4, 6, 4, 6),
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
                Padding         = new Thickness(4, 6, 4, 6),
                BorderBrush     = Brushes.Black,
                BorderThickness = new Thickness(0.5),
            });
        }
        return row;
    }

    private static TableRow DepositDeductionRow(int stt, decimal amount)
    {
        var negativeText = $"({Math.Abs(amount).ToString("N0", CultureInfo.GetCultureInfo("vi-VN"))})";
        var row = new TableRow();
        var values = new[] { stt.ToString(), "Trừ Cọc", "", "", "", negativeText, "", negativeText };
        for (var i = 0; i < values.Length; i++)
        {
            var isMoneyColumn = i == 5 || i == 7; // Thành tiền / Tổng cộng
            var alignment = i == ProductNameColumnIndex ? TextAlignment.Left : TextAlignment.Center;
            var run = new Run(values[i]) { Foreground = isMoneyColumn ? Brushes.Red : Brushes.Black };
            row.Cells.Add(new TableCell(new Paragraph(run) { TextAlignment = alignment })
            {
                Padding         = new Thickness(4, 6, 4, 6),
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
        const double header          = 96;  // logo + 5 dòng thông tin công ty (font lớn hơn 2026-08-15)
        const double title           = 38;  // "HÓA ĐƠN BÁN HÀNG" + Số HĐ
        const double customerInfo    = 84;  // Tên KH/Điện thoại/Địa chỉ + PT giao hàng-thanh toán
        const double tableHeaderRow  = 30;
        const double perProductRow   = 34;  // xấp xỉ, dài hơn nếu tên sản phẩm wrap 2 dòng
        const double totalsRow       = 32;
        const double notesRow        = 32;
        const double dateAndSignature = 98; // dòng Ngày... + khoảng trống chữ ký có chủ đích
        const double framePadding    = 28;  // Padding(14) x2 của outer frame
        const double pagePadding     = 32;  // PagePadding(16) x2

        return header + title + customerInfo + tableHeaderRow + (perProductRow * lineCount)
            + totalsRow + notesRow + dateAndSignature + framePadding + pagePadding;
    }
}
