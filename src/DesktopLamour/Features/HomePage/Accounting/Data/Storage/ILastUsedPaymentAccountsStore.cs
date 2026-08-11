// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Accounting.Data.Storage;

/// <summary>
/// Remembers the last TK Nợ / TK Có picked on a Phiếu chi entry so new lines default to
/// them. In-memory only (lives for the current app session) — this app has no disk-based
/// user-preference persistence yet, matching how IAuthTokenStorage itself doesn't survive
/// a restart either.
/// </summary>
public interface ILastUsedPaymentAccountsStore
{
    int? LastDebitAccountId { get; set; }
    int? LastCreditAccountId { get; set; }
}
