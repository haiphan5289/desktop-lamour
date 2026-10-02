// Copyright © 2026 DesktopLamour. All rights reserved.
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DesktopLamour.Shared.Controls;

namespace DesktopLamour.Features.HomePage.Accounting.Domain.Models;

public class ReceiptEntryItem : INotifyPropertyChanged
{
    private string  _description   = "";
    private ISearchableItem? _selectedDebitAccount;
    private ISearchableItem? _selectedCreditAccount;
    private decimal _amount;
    private string? _subjectCode;
    private string? _subjectName;
    private string? _bankAccount;
    private int?     _salesOrderId;

    public string Description
    {
        get => _description;
        set { _description = value; OnPropertyChanged(); }
    }

    // TK Nợ/Có chọn từ danh mục Tài khoản kế toán — bind qua SelectedItem (cả object), cùng lý do với
    // PaymentEntryItem.SelectedDebitAccount.
    public ISearchableItem? SelectedDebitAccount
    {
        get => _selectedDebitAccount;
        set { _selectedDebitAccount = value; OnPropertyChanged(); }
    }

    public ISearchableItem? SelectedCreditAccount
    {
        get => _selectedCreditAccount;
        set { _selectedCreditAccount = value; OnPropertyChanged(); }
    }

    public decimal Amount
    {
        get => _amount;
        set { _amount = value; OnPropertyChanged(); }
    }

    public string? SubjectCode
    {
        get => _subjectCode;
        set { _subjectCode = value; OnPropertyChanged(); }
    }

    public string? SubjectName
    {
        get => _subjectName;
        set { _subjectName = value; OnPropertyChanged(); }
    }

    public string? BankAccount
    {
        get => _bankAccount;
        set { _bankAccount = value; OnPropertyChanged(); }
    }

    // Chứng từ bán hàng gốc đang được thu tiền (từ Phiếu thu hàng loạt khách hàng) — null cho dòng
    // thu bình thường không gắn với 1 đơn hàng cụ thể.
    public int? SalesOrderId
    {
        get => _salesOrderId;
        set { _salesOrderId = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    // Dòng chưa nhập gì — lưới hiện trống hẳn (ẩn nút xoá, ô TK) như dòng trống ở các popup chứng từ khác.
    public bool IsEmpty =>
        Amount == 0
        && string.IsNullOrWhiteSpace(Description)
        && SelectedDebitAccount is null && SelectedCreditAccount is null
        && string.IsNullOrWhiteSpace(SubjectCode) && string.IsNullOrWhiteSpace(SubjectName)
        && string.IsNullOrWhiteSpace(BankAccount);

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        if (name != nameof(IsEmpty))
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEmpty)));
    }
}
