// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Accounting.Data.Storage;

public sealed class LastUsedPaymentAccountsStore : ILastUsedPaymentAccountsStore
{
    public int? LastDebitAccountId { get; set; }
    public int? LastCreditAccountId { get; set; }
}
