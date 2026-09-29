// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Globalization;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DesktopLamour.Shared.Helpers;

namespace DesktopLamour.Features.HomePage.Accounting.Views;

// Dữ liệu 1 bản in phiếu thu / phiếu chi (mẫu 01-TT / 02-TT, Thông tư 200/2014/TT-BTC).
public sealed record CashVoucherPrintModel(
    string Title,                 // "PHIẾU THU" / "PHIẾU CHI"
    string FormNumber,            // "01 - TT" / "02 - TT"
    string DocumentNumber,
    DateTime DocumentDate,
    string DebitCodes,            // đã ở dạng in (1111, 6418...) — xem CashVoucherDocumentBuilder.JoinAccountCodes
    string CreditCodes,
    string PartyLabel,            // "Họ tên người nộp tiền" / "Họ tên người nhận tiền"
    string PartyName,
    string? Address,
    string ReasonLabel,           // "Lý do nộp" / "Lý do chi"
    string ReasonText,
    decimal Amount,
    string? Attachment,
    IReadOnlyList<(string Role, string Note)> Signatures,
    int PartySignatureIndex);     // cột chữ ký của người nộp/nhận, để in tên bên dưới

// Bản in A5 dạng FlowDocument dùng chung cho Phiếu thu (ReceiptPrintWindow) và Phiếu chi
// (PaymentPrintWindow) — khớp ảnh bản in MISA. Chi tiết layout + các lỗi FlowDocument đã gặp: xem
// skill ct-print-invoice-layout. Mỗi TableRow luôn tạo TableCell MỚI (không dùng lại cell giữa các row).
public static class CashVoucherDocumentBuilder
{
    private const double MmToDip = 96.0 / 25.4;
    public static readonly double A5PageWidth  = 148 * MmToDip;
    public static readonly double A5PageHeight = 210 * MmToDip;

    private static readonly SolidColorBrush OuterBorderBrush = new(Color.FromRgb(0x9D, 0xC1, 0xE0));

    // Số hiệu TK in trên phiếu theo MISA (TK chi tiết cấp 2): Tiền mặt VNĐ 1111, Tiền gửi VNĐ 1121.
    // Trên lưới hạch toán của phiếu thu vẫn hiện 111/112 — chỉ bản in dùng số chi tiết. Nhận cả tên
    // enum của phiếu thu (Cash111...) lẫn mã tài khoản của phiếu chi (111...); TK khác giữ nguyên.
    public static string PrintAccountCode(string? account) => account switch
    {
        "Cash111" or "111" => "1111",
        "Bank112" or "112" => "1121",
        "Receivable131"    => "131",
        _                  => account ?? "",
    };

    public static string JoinAccountCodes(IEnumerable<string?> accounts) =>
        string.Join(", ", accounts.Select(PrintAccountCode).Where(c => c.Length > 0).Distinct());

    public static void Print(FlowDocument? document, string jobName)
    {
        if (document is null) return;

        var printDialog = new PrintDialog();
        printDialog.PrintTicket.PageMediaSize = new PageMediaSize(PageMediaSizeName.ISOA5);
        if (printDialog.ShowDialog() != true) return;

        document.PageHeight  = A5PageHeight;
        document.PageWidth   = A5PageWidth;
        document.PagePadding = new Thickness(14);
        document.ColumnWidth = A5PageWidth;

        IDocumentPaginatorSource paginatorSource = document;
        printDialog.PrintDocument(paginatorSource.DocumentPaginator, jobName);
    }

    private static readonly string DottedFill = new('.', 135);

    public static FlowDocument Build(CashVoucherPrintModel m)
    {
        var doc = new FlowDocument
        {
            FontFamily  = new FontFamily("Times New Roman"),
            FontSize    = 12,
            // FlowDocument mặc định Justify → tên công ty bị giãn chữ ("CÔNG   TY   TNHH"). MISA căn trái.
            TextAlignment = TextAlignment.Left,
            Background  = Brushes.White,
            PagePadding = new Thickness(14),
            PageWidth   = A5PageWidth,
            PageHeight  = A5PageHeight,
            ColumnWidth = A5PageWidth,
        };

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

        // ── Header: logo | thông tin công ty | Mẫu số (01-TT phiếu thu / 02-TT phiếu chi) ── (1 bảng 3 cột — khớp MISA: mẫu số
        // nằm góc phải ngang hàng tên công ty, không nằm dưới)
        var headerTable = new Table { CellSpacing = 0, Margin = new Thickness(0, 0, 0, 6) };
        headerTable.Columns.Add(new TableColumn { Width = new GridLength(90) });
        headerTable.Columns.Add(new TableColumn());
        headerTable.Columns.Add(new TableColumn { Width = new GridLength(185) });
        var headerRow = new TableRow();

        var logoImage = new System.Windows.Controls.Image
        {
            Source            = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/lamour-logo.png")),
            Width             = 84,
            Height            = 30,
            Stretch           = Stretch.Uniform,
            VerticalAlignment = VerticalAlignment.Top,
        };
        headerRow.Cells.Add(new TableCell(new BlockUIContainer(logoImage)) { Padding = new Thickness(0, 14, 4, 0) });

        var companyPara = new Paragraph { Margin = new Thickness(0), FontSize = 10 };
        companyPara.Inlines.Add(new Bold(new Run("CÔNG TY TNHH THƯƠNG MẠI DỊCH VỤ LAMOUR")) { FontSize = 11 });
        companyPara.Inlines.Add(new LineBreak());
        companyPara.Inlines.Add(new Run("Số 110/20/38 Đường số 30, Phường An Nhơn, TP Hồ Chí Minh."));
        companyPara.Inlines.Add(new LineBreak());
        companyPara.Inlines.Add(new Run("Mã số thuế: 0319088143"));
        companyPara.Inlines.Add(new LineBreak());
        companyPara.Inlines.Add(new Run("Tel: 0868858975 - Website: www.skincoachlamour.com"));
        headerRow.Cells.Add(new TableCell(companyPara));

        var formPara = new Paragraph { Margin = new Thickness(0), TextAlignment = TextAlignment.Center };
        formPara.Inlines.Add(new Bold(new Run($"Mẫu số {m.FormNumber}")) { FontSize = 12 });
        formPara.Inlines.Add(new LineBreak());
        // Xuống dòng cố định như MISA — để tự wrap thì WPF ngắt giữa "200/2014/ TT-BTC".
        formPara.Inlines.Add(new Italic(new Run("(Ban hành theo Thông tư số 200/2014/TT-BTC")) { FontSize = 9 });
        formPara.Inlines.Add(new LineBreak());
        formPara.Inlines.Add(new Italic(new Run("Ngày 22/12/2014 của Bộ Tài chính)")) { FontSize = 9 });
        headerRow.Cells.Add(new TableCell(formPara) { Padding = new Thickness(0, 0, 8, 0) });

        var headerGroup = new TableRowGroup();
        headerGroup.Rows.Add(headerRow);
        headerTable.RowGroups.Add(headerGroup);
        content.Blocks.Add(headerTable);

        // ── Title + Ngày | Quyển số / Số / Nợ / Có ── 1 bảng 3 cột (spacer | giữa | phải) để title
        // và dòng Ngày cùng tâm theo cấu trúc, không phụ thuộc ước lượng bề rộng trang.
        var documentDate = m.DocumentDate.ToLocalTime();

        const double sideColumnWidth = 130;
        var titleTable = new Table { CellSpacing = 0, Margin = new Thickness(0, 0, 0, 8) };
        titleTable.Columns.Add(new TableColumn { Width = new GridLength(sideColumnWidth) });
        titleTable.Columns.Add(new TableColumn());
        titleTable.Columns.Add(new TableColumn { Width = new GridLength(sideColumnWidth) });
        var titleGroup = new TableRowGroup();

        titleGroup.Rows.Add(TitleRow(
            new Paragraph(new Bold(new Run(m.Title)) { FontSize = 17 }) { TextAlignment = TextAlignment.Center },
            SideParagraph("Quyển số: ", "..................")));
        titleGroup.Rows.Add(TitleRow(
            new Paragraph(new Bold(new Italic(new Run($"Ngày {documentDate.Day} tháng {documentDate.Month} năm {documentDate.Year}"))))
            {
                TextAlignment = TextAlignment.Center,
            },
            new Paragraph()));
        titleGroup.Rows.Add(TitleRow(new Paragraph(), SideParagraph("Số: ", m.DocumentNumber)));
        titleGroup.Rows.Add(TitleRow(new Paragraph(), SideParagraph("Nợ: ", m.DebitCodes)));
        titleGroup.Rows.Add(TitleRow(new Paragraph(), SideParagraph("Có: ", m.CreditCodes)));

        titleTable.RowGroups.Add(titleGroup);
        content.Blocks.Add(titleTable);

        // ── Thông tin chung ──
        var totalAmount = m.Amount;
        // Để trống khi chưa có tiền thật thay vì "Không đồng" (khớp WarehouseReceiptPrintWindow).
        // MISA ghi "... đồng chẵn." — helper trả "... đồng." nên thay đuôi.
        var amountInWords = totalAmount == 0
            ? ""
            : VietnameseNumberToWordsHelper.ToWords(totalAmount).TrimEnd('.') + " chẵn.";

        content.Blocks.Add(InfoParagraph($"{m.PartyLabel}:  ", new Run(m.PartyName)));
        content.Blocks.Add(InfoParagraph("Địa chỉ:  ", new Run(FillOrDots(m.Address))));
        content.Blocks.Add(InfoParagraph($"{m.ReasonLabel}:  ", new Run(m.ReasonText)));
        content.Blocks.Add(InfoParagraph("Số tiền:  ", new Bold(new Run($"{FormatMoney(totalAmount)} VND"))));
        content.Blocks.Add(InfoParagraph("Viết bằng chữ:  ", new Bold(new Italic(new Run(amountInWords)))));
        var attachment = string.IsNullOrWhiteSpace(m.Attachment) ? "..............." : m.Attachment.Trim();
        content.Blocks.Add(InfoParagraph("Kèm theo:  ", new Run($"{attachment} chứng từ gốc")));

        // ── Ngày ký + chữ ký ──
        content.Blocks.Add(new Paragraph(new Italic(new Run("Ngày..... tháng ..... năm.............")))
        {
            TextAlignment = TextAlignment.Right,
            Margin        = new Thickness(0, 14, 10, 4),
        });

        var signTable = new Table { CellSpacing = 0 };
        for (var i = 0; i < m.Signatures.Count; i++) signTable.Columns.Add(new TableColumn());
        var signGroup = new TableRowGroup();

        var signRow = new TableRow();
        foreach (var (role, note) in m.Signatures)
        {
            var para = new Paragraph { TextAlignment = TextAlignment.Center, Margin = new Thickness(0) };
            para.Inlines.Add(new Bold(new Run(role)));
            para.Inlines.Add(new LineBreak());
            para.Inlines.Add(new Italic(new Run(note)) { FontSize = 10 });
            signRow.Cells.Add(new TableCell(para) { Padding = new Thickness(2, 3, 2, 55) });
        }
        signGroup.Rows.Add(signRow);

        // Tên người nộp/nhận in dưới cột chữ ký của họ — khớp MISA. Cột giữa (index 2) thì ô tên trải 3 cột
        // (1..3) nên vẫn cùng tâm nhưng đủ rộng cho tên dài; cột khác thì chỉ 1 ô đúng cột đó.
        var nameRow = new TableRow();
        if (m.PartySignatureIndex == 2)
        {
            nameRow.Cells.Add(new TableCell(new Paragraph()));
            nameRow.Cells.Add(NameCell(m.PartyName, columnSpan: 3));
            nameRow.Cells.Add(new TableCell(new Paragraph()));
        }
        else
        {
            for (var i = 0; i < m.Signatures.Count; i++)
                nameRow.Cells.Add(i == m.PartySignatureIndex ? NameCell(m.PartyName, columnSpan: 1) : new TableCell(new Paragraph()));
        }
        signGroup.Rows.Add(nameRow);

        signTable.RowGroups.Add(signGroup);
        content.Blocks.Add(signTable);

        // ── Đã nhận đủ số tiền (Viết bằng chữ) ── 2 cột: nhãn | số tiền bằng chữ (tự xuống dòng
        // thẳng lề cột phải, như MISA).
        var receivedTable = new Table { CellSpacing = 0, Margin = new Thickness(0, 12, 0, 0) };
        receivedTable.Columns.Add(new TableColumn { Width = new GridLength(190) });
        receivedTable.Columns.Add(new TableColumn());
        var receivedRow = new TableRow();
        receivedRow.Cells.Add(new TableCell(new Paragraph(new Run("Đã nhận đủ số tiền (Viết bằng chữ)")) { Margin = new Thickness(0) }));
        receivedRow.Cells.Add(new TableCell(new Paragraph(new Run(amountInWords)) { Margin = new Thickness(0) })
        {
            Padding = new Thickness(0, 0, 10, 0),
        });
        var receivedGroup = new TableRowGroup();
        receivedGroup.Rows.Add(receivedRow);
        receivedTable.RowGroups.Add(receivedGroup);
        content.Blocks.Add(receivedTable);

        var spacerHeight = Math.Max(0, A5PageHeight - EstimateContentHeight());
        content.Blocks.Add(new BlockUIContainer(new Border { MinHeight = spacerHeight }));

        return doc;
    }

    // Mỗi dòng tạo TableCell MỚI — không dùng lại cell giữa các row (TableCell thuộc về đúng 1 row,
    // dùng lại sẽ ném ArgumentException lúc render).
    private static TableRow TitleRow(Paragraph center, Paragraph right)
    {
        var row = new TableRow();
        row.Cells.Add(new TableCell(new Paragraph()));
        row.Cells.Add(new TableCell(center));
        row.Cells.Add(new TableCell(right) { Padding = new Thickness(0, 0, 10, 0) });
        return row;
    }

    private static Paragraph SideParagraph(string label, string value)
    {
        var para = new Paragraph { Margin = new Thickness(0, 1, 0, 1) };
        para.Inlines.Add(new Run(label));
        para.Inlines.Add(new Run(value));
        return para;
    }

    private static Paragraph InfoParagraph(string label, Inline value)
    {
        var para = new Paragraph { Margin = new Thickness(0, 0, 0, 4) };
        para.Inlines.Add(new Run(label));
        para.Inlines.Add(value);
        return para;
    }

    private static string FillOrDots(string? value) =>
        string.IsNullOrWhiteSpace(value) ? DottedFill : value.Trim();

    private static string FormatMoney(decimal value) => value.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"));

    private static TableCell NameCell(string name, int columnSpan) =>
        new(new Paragraph(new Bold(new Run(name))) { TextAlignment = TextAlignment.Center, Margin = new Thickness(0) })
        {
            ColumnSpan = columnSpan,
        };

    private static double EstimateContentHeight()
    {
        const double header       = 75;
        const double title        = 110;
        const double info         = 140;
        const double signature    = 150;
        const double received     = 45;
        const double framePadding = 24;
        const double pagePadding  = 28;

        return header + title + info + signature + received + framePadding + pagePadding;
    }
}
