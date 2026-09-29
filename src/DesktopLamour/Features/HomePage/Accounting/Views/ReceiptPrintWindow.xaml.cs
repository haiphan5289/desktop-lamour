// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Windows;
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Accounting.Views;

// In "PHIẾU THU" — mẫu 01-TT (Thông tư 200/2014/TT-BTC), khớp ảnh bản in MISA của Phiếu thu tiền
// mặt khách hàng hàng loạt (2026-09-29): 1 phiếu = 1 tổng Số tiền (Σ mọi dòng hạch toán), KHÔNG in
// bảng chi tiết từng khách hàng. 5 chữ ký (Giám đốc · Kế toán trưởng · Người nộp tiền · Người lập
// phiếu · Thủ quỹ) theo đúng mẫu MISA. Layout dùng chung với Phiếu chi: CashVoucherDocumentBuilder.
// Nhận ReceiptResponseDto nên phiếu thu thường (ReceiptWindow) dùng lại được sau này.
public partial class ReceiptPrintWindow : Window
{
    private ReceiptResponseDto? _receipt;

    public ReceiptPrintWindow()
    {
        InitializeComponent();
    }

    public void Initialize(ReceiptResponseDto receipt, string reasonLabel)
    {
        _receipt = receipt;
        VoucherViewer.Document = CashVoucherDocumentBuilder.Build(new CashVoucherPrintModel(
            Title:          "PHIẾU THU",
            FormNumber:     "01 - TT",
            DocumentNumber: receipt.DocumentNumber,
            DocumentDate:   receipt.DocumentDate,
            DebitCodes:     CashVoucherDocumentBuilder.JoinAccountCodes(receipt.Entries.Select(e => e.DebitAccount)),
            CreditCodes:    CashVoucherDocumentBuilder.JoinAccountCodes(receipt.Entries.Select(e => e.CreditAccount)),
            PartyLabel:     "Họ tên người nộp tiền",
            PartyName:      receipt.PayerName,
            Address:        receipt.Address,
            ReasonLabel:    "Lý do nộp",
            ReasonText:     reasonLabel,
            Amount:         receipt.Entries.Sum(e => e.Amount),
            Attachment:     receipt.Attachment,
            Signatures:     new[]
            {
                ("Giám đốc",        "(Ký, họ tên, đóng dấu)"),
                ("Kế toán trưởng",  "(Ký, họ tên)"),
                ("Người nộp tiền",  "(Ký, họ tên)"),
                ("Người lập phiếu", "(Ký, họ tên)"),
                ("Thủ quỹ",         "(Ký, họ tên)"),
            },
            PartySignatureIndex: 2));
    }

    private void PrintButton_Click(object sender, RoutedEventArgs e) =>
        CashVoucherDocumentBuilder.Print(VoucherViewer.Document, $"Phiếu thu {_receipt?.DocumentNumber}");

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
