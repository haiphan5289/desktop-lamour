// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Globalization;
using System.Linq;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DesktopLamour.Features.HomePage.Warehouse.Data.Services.Dtos;
using DesktopLamour.Shared.Helpers;

namespace DesktopLamour.Features.HomePage.Warehouse.Views;

// In "PHIẾU NHẬP KHO" — mẫu 01-VT chuẩn kế toán (Thông tư 200/2014/TT-BTC): 2 hàng tiêu đề bảng
// (nhãn cột + hàng ký hiệu A/B/C/D/E/1/2/3 bắt buộc theo mẫu chính thức, không phải cột dữ liệu tự
// thêm), dòng Nợ/Có, dòng "Tổng số tiền viết bằng chữ". Copy cấu trúc FlowDocument (khổ A5, logo,
// khung viền) từ SalesOrderPrintWindow/SalesReturnPrintWindow đã có, chỉ đổi nội dung thân phiếu.
public partial class WarehouseReceiptPrintWindow : Window
{
    private WarehouseReceiptResponseDto? _receipt;

    public WarehouseReceiptPrintWindow()
    {
        InitializeComponent();
    }

    // partnerAddress: địa chỉ Khách hàng/Nhà cung cấp — WarehouseReceiptResponseDto không mang field
    // địa chỉ riêng, resolve từ ngoài vào (giống cách SalesOrderPrintWindow/SalesReturnPrintWindow
    // nhận customerPhone/customerAddress từ ViewModel).
    public void Initialize(WarehouseReceiptResponseDto receipt, string? partnerAddress)
    {
        _receipt = receipt;
        DocumentViewer.Document = BuildDocument(receipt, partnerAddress);
    }

    private void PrintButton_Click(object sender, RoutedEventArgs e)
    {
        var document = DocumentViewer.Document;
        if (document is null) return;

        var printDialog = new PrintDialog();
        printDialog.PrintTicket.PageMediaSize = new PageMediaSize(PageMediaSizeName.ISOA5);
        if (printDialog.ShowDialog() != true) return;

        document.PageHeight  = A5PageHeight;
        document.PageWidth   = A5PageWidth;
        document.PagePadding = new Thickness(14);
        document.ColumnWidth = A5PageWidth;

        IDocumentPaginatorSource paginatorSource = document;
        printDialog.PrintDocument(paginatorSource.DocumentPaginator, $"Phiếu nhập kho {_receipt?.ReceiptNumber}");
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    private const double MmToDip = 96.0 / 25.4;
    private static readonly double A5PageWidth  = 148 * MmToDip;
    private static readonly double A5PageHeight = 210 * MmToDip;

    // STT | Mã hàng | Tên hàng | Mã quy cách | ĐVT | Số lượng | Đơn giá | Thành tiền — tổng 490,
    // cùng ngân sách bề ngang đã kiểm chứng vừa khít khổ A5 ở 2 print window trước.
    private static readonly int[] ColumnWidths = { 26, 55, 105, 55, 45, 55, 70, 79 };

    private static readonly SolidColorBrush OuterBorderBrush = new(Color.FromRgb(0x9D, 0xC1, 0xE0));

    private const int HeaderTextLineCount = 5;
    private const double HeaderLineHeight = 15;

    private static FlowDocument BuildDocument(WarehouseReceiptResponseDto receipt, string? partnerAddress)
    {
        var doc = new FlowDocument
        {
            FontFamily    = new FontFamily("Segoe UI"),
            FontSize      = 12,
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

        // ── Header: logo (floated left) + company info ────
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

        var firstLine = receipt.Lines.FirstOrDefault();
        var debitAccount  = firstLine?.DebitAccount  ?? "";
        var creditAccount = firstLine?.CreditAccount ?? "";
        var documentDate  = receipt.DocumentDate.ToLocalTime();

        // Title + Ngày ... | Nợ/Có — gộp CHUNG 1 bảng 3 cột (spacer/giữa/phải) thay vì để title là
        // 1 Paragraph full-width riêng: nếu tính center theo 2 cơ chế khác nhau (paragraph center =
        // nửa content width, table-cell center = spacer + nửa cột giữa) thì chỉ cần lệch 1 chút
        // trong ước lượng content width là 2 trục lệch nhau ngay. Đặt cả 3 dòng (title/Ngày/Số) vào
        // CÙNG cột giữa của CÙNG bảng → tâm luôn trùng nhau tuyệt đối bất kể content width thật là
        // bao nhiêu. Cột phải (Nợ/Có) CHỪA padding để không tràn/khuất sát khung viền phải.
        var dateNoTable = new Table { Margin = new Thickness(0, 0, 0, 8) };
        dateNoTable.Columns.Add(new TableColumn { Width = new GridLength(140) }); // spacer
        dateNoTable.Columns.Add(new TableColumn { Width = new GridLength(216) }); // Title/Ngày.../Số:
        dateNoTable.Columns.Add(new TableColumn { Width = new GridLength(110) }); // Nợ:/Có:
        var dateNoGroup = new TableRowGroup();

        // Dựng thẳng từng TableRow với đủ 3 TableCell thật của nó — KHÔNG add TableCell vào 1
        // TableRow tạm rồi lấy lại (TableCell.Cells[0]) để nhét vào TableRow khác như code cũ
        // (CombineRow): 1 TableCell đã add vào Cells của row nào thì WPF coi nó thuộc sở hữu
        // (logical parent) của đúng row đó — add tiếp vào row khác ném thẳng
        // "Item belongs to another collection currently. Item must be removed first."
        var noCoCellPadding = new Thickness(0, 0, 10, 0);

        var titleRow = new TableRow();
        titleRow.Cells.Add(new TableCell(new Paragraph()));
        titleRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run("PHIẾU NHẬP KHO")) { FontSize = 20 })
        {
            TextAlignment = TextAlignment.Center,
            Margin        = new Thickness(0, 6, 0, 4),
        }));
        titleRow.Cells.Add(new TableCell(new Paragraph()));
        dateNoGroup.Rows.Add(titleRow);

        var dateRow = new TableRow();
        dateRow.Cells.Add(new TableCell(new Paragraph()));
        dateRow.Cells.Add(new TableCell(new Paragraph(new Italic(new Run(
            $"Ngày {documentDate.Day} tháng {documentDate.Month} năm {documentDate.Year}")))
        { TextAlignment = TextAlignment.Center }));
        dateRow.Cells.Add(new TableCell(new Paragraph(new Run($"Nợ: {debitAccount}")) { TextAlignment = TextAlignment.Right })
        {
            Padding = noCoCellPadding,
        });
        dateNoGroup.Rows.Add(dateRow);

        var soPara = new Paragraph { TextAlignment = TextAlignment.Center };
        soPara.Inlines.Add(new Run("Số: "));
        soPara.Inlines.Add(new Bold(new Run(receipt.ReceiptNumber)) { Foreground = Brushes.Red });
        var soRow = new TableRow();
        soRow.Cells.Add(new TableCell(new Paragraph()));
        soRow.Cells.Add(new TableCell(soPara));
        soRow.Cells.Add(new TableCell(new Paragraph(new Run($"Có: {creditAccount}")) { TextAlignment = TextAlignment.Right })
        {
            Padding = noCoCellPadding,
        });
        dateNoGroup.Rows.Add(soRow);

        dateNoTable.RowGroups.Add(dateNoGroup);
        content.Blocks.Add(dateNoTable);

        // Thông tin chung
        var deliverer = !string.IsNullOrWhiteSpace(receipt.DeliveryPerson)
            ? receipt.DeliveryPerson
            : receipt.CustomerName ?? receipt.SupplierName ?? "";
        content.Blocks.Add(new Paragraph(new Run($"- Họ và tên người giao: {deliverer}")) { Margin = new Thickness(0, 0, 0, 3) });
        content.Blocks.Add(new Paragraph(new Run($"- Địa chỉ: {partnerAddress}")) { Margin = new Thickness(0, 0, 0, 3) });
        content.Blocks.Add(new Paragraph(new Run($"- Diễn giải: {receipt.Description}")) { Margin = new Thickness(0, 0, 0, 3) });
        content.Blocks.Add(new Paragraph(new Run(
            $"- Theo {receipt.Reference} ngày {documentDate.Day} tháng {documentDate.Month} năm {documentDate.Year} của {receipt.CustomerName ?? receipt.SupplierName}"))
        { Margin = new Thickness(0, 0, 0, 3) });

        var warehouseName = firstLine?.WarehouseName ?? "";
        var warehouseTable = new Table { CellSpacing = 0, Margin = new Thickness(0, 0, 0, 8) };
        warehouseTable.Columns.Add(new TableColumn { Width = new GridLength(280) });
        warehouseTable.Columns.Add(new TableColumn { Width = new GridLength(215) });
        var warehouseRow = new TableRow();
        warehouseRow.Cells.Add(new TableCell(new Paragraph(new Run($"- Nhập tại kho: {warehouseName}"))));
        warehouseRow.Cells.Add(new TableCell(new Paragraph(new Run("Địa điểm: "))));
        var warehouseGroup = new TableRowGroup();
        warehouseGroup.Rows.Add(warehouseRow);
        warehouseTable.RowGroups.Add(warehouseGroup);
        content.Blocks.Add(warehouseTable);

        // Line items table
        var table = new Table { CellSpacing = 0, Margin = new Thickness(0) };
        foreach (var width in ColumnWidths)
            table.Columns.Add(new TableColumn { Width = new GridLength(width) });

        var rowGroup = new TableRowGroup();
        rowGroup.Rows.Add(HeaderRow("STT", "Mã hàng", "Tên hàng", "Mã quy cách", "ĐVT", "Số lượng", "Đơn giá", "Thành tiền"));
        // Hàng ký hiệu A/B/C/.../3 — BẮT BUỘC theo đúng mẫu 01-VT chính thức, không phải cột dữ liệu
        // tự thêm (khác với "Nhóm HHDV mua vào"/"Số lô"... đã bỏ qua ở các tab trước vì không có data).
        rowGroup.Rows.Add(HeaderRow("A", "B", "C", "D", "E", "1", "2", "3"));

        var stt = 1;
        foreach (var line in receipt.Lines)
        {
            rowGroup.Rows.Add(DataRow(
                stt++.ToString(),
                line.ProductCode,
                line.ProductName,
                "",
                line.Unit,
                line.Quantity.ToString("0.##"),
                FormatMoney(line.UnitPrice),
                FormatMoney(line.Amount)));
        }

        var congRow = new TableRow();
        var congLabelPara = new Paragraph(new Bold(new Run("Cộng"))) { TextAlignment = TextAlignment.Center };
        congRow.Cells.Add(new TableCell(congLabelPara)
        {
            ColumnSpan      = ColumnWidths.Length - 1,
            BorderBrush     = Brushes.Black,
            BorderThickness = new Thickness(0.5),
            Padding         = new Thickness(4, 6, 4, 6),
        });
        var congValuePara = new Paragraph(new Bold(new Run(FormatMoney(receipt.TotalAmount))))
        {
            TextAlignment = TextAlignment.Right,
        };
        congRow.Cells.Add(new TableCell(congValuePara)
        {
            ColumnSpan      = 1,
            BorderBrush     = Brushes.Black,
            BorderThickness = new Thickness(0.5),
            Padding         = new Thickness(4, 6, 4, 6),
        });
        rowGroup.Rows.Add(congRow);

        table.RowGroups.Add(rowGroup);
        content.Blocks.Add(table);

        // Để trống khi chưa có tiền thật (TotalAmount = 0) thay vì tự ghi "Không đồng" — khớp mẫu
        // MISA (dòng để trống, chờ điền tay/dữ liệu thật) thay vì hiện chữ như đã có giá trị.
        var amountInWords = receipt.TotalAmount == 0 ? "" : VietnameseNumberToWordsHelper.ToWords(receipt.TotalAmount);
        content.Blocks.Add(new Paragraph(new Run($"- Tổng số tiền (Viết bằng chữ): {amountInWords}"))
        {
            Margin = new Thickness(0, 8, 0, 3),
        });
        content.Blocks.Add(new Paragraph(new Run("- Số chứng từ gốc kèm theo: "))
        {
            Margin = new Thickness(0, 0, 0, 12),
        });

        content.Blocks.Add(new Paragraph(new Italic(new Run("Ngày ..... tháng ..... năm .........")))
        {
            TextAlignment = TextAlignment.Right,
            FontSize      = 12,
            Margin        = new Thickness(0, 0, 0, 22),
        });

        var signTable = new Table { CellSpacing = 0 };
        for (var i = 0; i < 4; i++) signTable.Columns.Add(new TableColumn());
        var signRow = new TableRow();
        var signLabels = new[] { "Người lập phiếu", "Người giao hàng", "Thủ kho", "Kế toán trưởng" };
        var signNotes  = new[] { "(Ký, họ tên)", "(Ký, họ tên)", "(Ký, họ tên)", "(Hoặc bộ phận có nhu cầu nhập)\n(Ký, họ tên)" };
        for (var i = 0; i < signLabels.Length; i++)
        {
            var cellPara = new Paragraph(new Bold(new Run(signLabels[i])))
            {
                TextAlignment = TextAlignment.Center,
                FontSize      = 12,
            };
            cellPara.Inlines.Add(new LineBreak());
            cellPara.Inlines.Add(new Italic(new Run(signNotes[i])) { FontSize = 10 });
            signRow.Cells.Add(new TableCell(cellPara) { Padding = new Thickness(2, 3, 2, 48) });
        }
        var signGroup = new TableRowGroup();
        signGroup.Rows.Add(signRow);
        signTable.RowGroups.Add(signGroup);
        content.Blocks.Add(signTable);

        var estimatedContentHeight = EstimateContentHeight(receipt.Lines.Count);
        var spacerHeight = Math.Max(0, A5PageHeight - estimatedContentHeight);
        content.Blocks.Add(new BlockUIContainer(new Border { MinHeight = spacerHeight }));

        return doc;
    }

    private const int ProductNameColumnIndex = 2;

    private static TableRow HeaderRow(params string[] headers)
    {
        var row = new TableRow { Background = Brushes.WhiteSmoke };
        for (var i = 0; i < headers.Length; i++)
        {
            row.Cells.Add(new TableCell(new Paragraph(new Bold(new Run(headers[i])))
            {
                TextAlignment = TextAlignment.Center,
                FontSize      = 10,
            })
            {
                Padding         = new Thickness(1, 4, 1, 4),
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
            row.Cells.Add(new TableCell(new Paragraph(new Run(values[i])) { TextAlignment = alignment, FontSize = 11 })
            {
                Padding         = new Thickness(1, 5, 1, 5),
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
        const double title           = 40;
        const double dateNoRow       = 38;
        const double generalInfo     = 90;
        const double tableHeaderRow  = 48; // 2 hàng tiêu đề
        const double perProductRow   = 32;
        const double congRow         = 32;
        const double amountInWords   = 44;
        const double dateAndSignature = 100;
        const double framePadding    = 28;
        const double pagePadding     = 32;

        return header + title + dateNoRow + generalInfo + tableHeaderRow + (perProductRow * lineCount)
            + congRow + amountInWords + dateAndSignature + framePadding + pagePadding;
    }
}
