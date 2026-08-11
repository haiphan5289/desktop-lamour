// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Globalization;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Accounting.Views;

public partial class PaymentPrintWindow : Window
{
    private PaymentResponseDto? _payment;

    public PaymentPrintWindow()
    {
        InitializeComponent();
    }

    public void Initialize(PaymentResponseDto payment)
    {
        _payment = payment;
        VoucherViewer.Document = BuildVoucherDocument(payment);
    }

    private void PrintButton_Click(object sender, RoutedEventArgs e)
    {
        var document = VoucherViewer.Document;
        if (document is null) return;

        var printDialog = new PrintDialog();
        printDialog.PrintTicket.PageMediaSize = new PageMediaSize(PageMediaSizeName.ISOA5);
        if (printDialog.ShowDialog() != true) return;

        document.PageHeight  = A5PageHeight;
        document.PageWidth   = A5PageWidth;
        document.PagePadding = new Thickness(14);
        document.ColumnWidth = A5PageWidth;

        IDocumentPaginatorSource paginatorSource = document;
        printDialog.PrintDocument(paginatorSource.DocumentPaginator, $"Phiếu chi {_payment?.DocumentNumber}");
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    private const double MmToDip = 96.0 / 25.4;
    private static readonly double A5PageWidth  = 148 * MmToDip;
    private static readonly double A5PageHeight = 210 * MmToDip;

    private static readonly int[] EntryTableColumnWidths = { 152, 55, 55, 100, 88 };

    private static readonly SolidColorBrush OuterBorderBrush = new(Color.FromRgb(0x9D, 0xC1, 0xE0));

    private static readonly Dictionary<string, string> PaymentReasonLabels = new()
    {
        ["ChiKhac"]    = "Chi khác",
        ["ChiMuaHang"] = "Chi mua hàng",
        ["ChiTraNo"]   = "Chi trả nợ",
        ["ChiLuong"]   = "Chi lương",
    };

    private static FlowDocument BuildVoucherDocument(PaymentResponseDto payment)
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

        // ── Header: logo (floated left) + company info ──
        var logoImage = new System.Windows.Controls.Image
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
        content.Blocks.Add(headerPara);

        // ── Title + document number ──
        const double docNoColumnWidth = 140;
        var titleTable = new Table { Margin = new Thickness(0) };
        titleTable.Columns.Add(new TableColumn { Width = new GridLength(docNoColumnWidth) });
        titleTable.Columns.Add(new TableColumn());
        titleTable.Columns.Add(new TableColumn { Width = new GridLength(docNoColumnWidth) });
        var titleRow = new TableRow();
        titleRow.Cells.Add(new TableCell(new Paragraph()));
        titleRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run("PHIẾU CHI")) { FontSize = 15 })
        {
            TextAlignment = TextAlignment.Center,
        }));
        var docNoPara = new Paragraph { TextAlignment = TextAlignment.Right };
        docNoPara.Inlines.Add(new Run("Số: "));
        docNoPara.Inlines.Add(new Run(payment.DocumentNumber) { Foreground = Brushes.Red, FontWeight = FontWeights.Bold });
        titleRow.Cells.Add(new TableCell(docNoPara));
        var titleGroup = new TableRowGroup();
        titleGroup.Rows.Add(titleRow);
        titleTable.RowGroups.Add(titleGroup);
        content.Blocks.Add(titleTable);

        content.Blocks.Add(new Paragraph(new Run($"Ngày {payment.DocumentDate.Day:D2} Tháng {payment.DocumentDate.Month:D2} Năm {payment.DocumentDate.Year}"))
        {
            TextAlignment = TextAlignment.Center,
            Margin        = new Thickness(0, 0, 0, 8),
        });

        // ── Info ──
        var reasonLabel = PaymentReasonLabels.GetValueOrDefault(payment.PaymentReason, payment.PaymentReason);
        content.Blocks.Add(new Paragraph(new Run($"Họ tên người nhận tiền: {payment.PayeeName}")) { Margin = new Thickness(0, 0, 0, 2) });
        content.Blocks.Add(new Paragraph(new Run($"Địa chỉ: {payment.Address}")) { Margin = new Thickness(0, 0, 0, 2) });
        var reasonText = string.IsNullOrWhiteSpace(payment.ReasonDetail) ? reasonLabel : $"{reasonLabel} — {payment.ReasonDetail}";
        content.Blocks.Add(new Paragraph(new Run($"Lý do chi: {reasonText}")) { Margin = new Thickness(0, 0, 0, 8) });

        // ── Entries table ──
        var table = new Table { CellSpacing = 0, Margin = new Thickness(0) };
        foreach (var width in EntryTableColumnWidths)
            table.Columns.Add(new TableColumn { Width = new GridLength(width) });

        var rowGroup = new TableRowGroup();
        rowGroup.Rows.Add(HeaderRow("Diễn giải", "TK Nợ", "TK Có", "Khoản mục CP", "Số tiền"));

        foreach (var e in payment.Entries)
        {
            rowGroup.Rows.Add(DataRow(
                e.Description,
                e.DebitAccountCode ?? "",
                e.CreditAccountCode ?? "",
                e.ExpenseCategoryName ?? "",
                FormatMoney(e.Amount)));
        }

        var totalAmount = payment.Entries.Sum(e => e.Amount);
        var totalRow = new TableRow();
        totalRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run("Cộng"))) { TextAlignment = TextAlignment.Right })
        {
            ColumnSpan      = EntryTableColumnWidths.Length - 1,
            BorderBrush     = Brushes.Black,
            BorderThickness = new Thickness(0.5),
            Padding         = new Thickness(3),
        });
        totalRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run(FormatMoney(totalAmount)))) { TextAlignment = TextAlignment.Center })
        {
            BorderBrush     = Brushes.Black,
            BorderThickness = new Thickness(0.5),
            Padding         = new Thickness(3),
        });
        rowGroup.Rows.Add(totalRow);

        table.RowGroups.Add(rowGroup);
        content.Blocks.Add(table);

        // ── Signatures ──
        content.Blocks.Add(new Paragraph { Margin = new Thickness(0, 16, 0, 0) });
        var signTable = new Table { CellSpacing = 0 };
        for (int i = 0; i < 4; i++) signTable.Columns.Add(new TableColumn());
        var signRow = new TableRow();
        foreach (var label in new[] { "Người lập phiếu", "Người nhận tiền", "Thủ quỹ", "Kế toán trưởng" })
        {
            signRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run(label))) { TextAlignment = TextAlignment.Center })
            {
                Padding = new Thickness(3, 3, 3, 45),
            });
        }
        var signGroup = new TableRowGroup();
        signGroup.Rows.Add(signRow);
        signTable.RowGroups.Add(signGroup);
        content.Blocks.Add(signTable);

        var estimatedContentHeight = EstimateContentHeight(payment.Entries.Count);
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

    private static double EstimateContentHeight(int lineCount)
    {
        const double header        = 60;
        const double title         = 45;
        const double info          = 60;
        const double tableHeaderRow = 24;
        const double perEntryRow   = 24;
        const double totalRow      = 24;
        const double signature     = 90;
        const double framePadding  = 24;
        const double pagePadding   = 28;

        return header + title + info + tableHeaderRow + (perEntryRow * lineCount)
            + totalRow + signature + framePadding + pagePadding;
    }
}
