// Copyright © 2026 DesktopLamour. All rights reserved.

namespace DesktopLamour.Features.HomePage.Accounting.Domain.Models;

public class PaymentReceiptLineItem
{
    public DateTime  DocumentDate   { get; set; } = DateTime.Today;
    public string    DocumentNumber { get; set; } = "";
    public string    InvoiceNumber  { get; set; } = "";
    public string    Description    { get; set; } = "";
    public DateTime? DueDate        { get; set; }
    public decimal   AmountDue      { get; set; }
    public decimal   AmountPaid     { get; set; }
}
