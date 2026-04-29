// Copyright © 2026 DesktopLamour. All rights reserved.
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DesktopLamour.Features.HomePage.Accounting.Domain.Models;

public class PaymentEntryItem : INotifyPropertyChanged
{
    private string  _description   = "";
    private string  _debitAccount  = "";
    private string  _creditAccount = "";
    private decimal _amount;
    private string? _subjectCode;
    private string? _subjectName;
    private string? _bankAccount;

    public string Description
    {
        get => _description;
        set { _description = value; OnPropertyChanged(); }
    }

    public string DebitAccount
    {
        get => _debitAccount;
        set { _debitAccount = value; OnPropertyChanged(); }
    }

    public string CreditAccount
    {
        get => _creditAccount;
        set { _creditAccount = value; OnPropertyChanged(); }
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

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
