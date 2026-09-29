// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Windows;
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;
using DesktopLamour.Shared.Converters;

namespace DesktopLamour.Features.HomePage.Accounting.Views;

// In "PHIẾU CHI" — mẫu 02-TT (Thông tư 200/2014/TT-BTC). 2026-09-29: làm lại theo đúng layout Phiếu thu
// (khớp ảnh bản in MISA) thay cho bản bảng dòng hạch toán cũ — đổi tiêu đề, Nợ/Có, lý do chi và
// chữ ký (Giám đốc · Kế toán trưởng · Thủ quỹ · Người lập phiếu · Người nhận tiền). 1 phiếu = 1 tổng Số
// tiền (Σ các dòng hạch toán). Nợ = TK Nợ thật của phiếu (vd 6418), Có = TK Có (vd 1111).
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

        // "Lý do chi" in nội dung chi tiết người dùng gõ (vd "làm cờ even 21/7"); trống thì lấy nhãn lý do.
        var reasonText = string.IsNullOrWhiteSpace(payment.ReasonDetail)
            ? PaymentReasonDisplayConverter.Label(payment.PaymentReason)
            : payment.ReasonDetail.Trim();

        VoucherViewer.Document = CashVoucherDocumentBuilder.Build(new CashVoucherPrintModel(
            Title:          "PHIẾU CHI",
            FormNumber:     "02 - TT",
            DocumentNumber: payment.DocumentNumber,
            DocumentDate:   payment.DocumentDate,
            DebitCodes:     CashVoucherDocumentBuilder.JoinAccountCodes(payment.Entries.Select(e => e.DebitAccountCode)),
            CreditCodes:    CashVoucherDocumentBuilder.JoinAccountCodes(payment.Entries.Select(e => e.CreditAccountCode)),
            PartyLabel:     "Họ tên người nhận tiền",
            PartyName:      payment.PayeeName,
            Address:        payment.Address,
            ReasonLabel:    "Lý do chi",
            ReasonText:     reasonText,
            Amount:         payment.Entries.Sum(e => e.Amount),
            Attachment:     payment.Attachment,
            Signatures:     new[]
            {
                ("Giám đốc",        "(Ký, họ tên, đóng dấu)"),
                ("Kế toán trưởng",  "(Ký, họ tên)"),
                ("Thủ quỹ",         "(Ký, họ tên)"),
                ("Người lập phiếu", "(Ký, họ tên)"),
                ("Người nhận tiền", "(Ký, họ tên)"),
            },
            PartySignatureIndex: 4));
    }

    private void PrintButton_Click(object sender, RoutedEventArgs e) =>
        CashVoucherDocumentBuilder.Print(VoucherViewer.Document, $"Phiếu chi {_payment?.DocumentNumber}");

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
