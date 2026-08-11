// Copyright © 2026 DesktopLamour. All rights reserved.
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DesktopLamour.Features.HomePage.Warehouses.Domain.Models;
using DesktopLamour.Shared.Controls;

namespace DesktopLamour.Features.HomePage.Accounting.Domain.Models;

public class PaymentEntryItem : INotifyPropertyChanged
{
    private string  _description   = "";
    private ISearchableItem? _selectedDebitAccount;
    private ISearchableItem? _selectedCreditAccount;
    private decimal _amount;
    private string? _subjectCode;
    private string? _subjectName;
    private string? _bankAccount;
    private ExpenseCategory? _selectedExpenseCategory;

    public string Description
    {
        get => _description;
        set { _description = value; OnPropertyChanged(); }
    }

    // Bind qua SelectedItem (cả object) thay vì SelectedValue/SelectedValuePath (Id kiểu int?) —
    // SelectedValue TwoWay với kiểu Nullable<int> không đẩy được giá trị ngược lại nguồn khi
    // ItemsSource chỉ có 1 item (đã xác nhận qua debug log: SelectionChanged bắn ra đúng
    // SelectedValue nhưng PropertyChanged trên entry không bao giờ fire). SelectedItem không
    // gặp vấn đề này.
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

    public ExpenseCategory? SelectedExpenseCategory
    {
        get => _selectedExpenseCategory;
        set { _selectedExpenseCategory = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
