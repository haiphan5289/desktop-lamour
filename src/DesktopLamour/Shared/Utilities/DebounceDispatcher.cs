// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Shared.Utilities;

/// <summary>
/// Coalesces rapid-fire calls (vd gõ vào ô tìm kiếm) thành 1 lần gọi action duy nhất sau khi im lặng
/// đủ <paramref name="delay"/>. Mỗi lần Debounce() được gọi lại sẽ huỷ lần chờ trước đó — dùng cho các
/// list ViewModel filter server-side (SalesOrders, SalesReturns, Deposits, Payments, Receipts,
/// WarehouseTransactions/Receipts) để không bắn 1 HTTP request cho mỗi ký tự gõ.
/// </summary>
public sealed class DebounceDispatcher
{
    private CancellationTokenSource? _cts;

    public void Debounce(TimeSpan delay, Func<CancellationToken, Task> action)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        _ = RunAsync(delay, action, token);
    }

    private static async Task RunAsync(TimeSpan delay, Func<CancellationToken, Task> action, CancellationToken token)
    {
        try
        {
            await Task.Delay(delay, token);
            if (!token.IsCancellationRequested)
                await action(token);
        }
        catch (OperationCanceledException) { }
    }
}
