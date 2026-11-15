// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Net;
using System.Net.Http;
using System.Windows;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Core.Storage;
using DesktopLamour.Features.Realtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DesktopLamour.Core.Http;

// 2026-11-15: xử lý 401 CHUNG cho mọi HttpClient (gắn qua ConfigureHttpClientDefaults trong App.xaml.cs).
// Trước đây không chỗ nào xử lý 401 — token JWT hết hạn (BE cấp 8 giờ) hoặc bị BE từ chối thì mỗi màn tự
// hiện "Response status code does not indicate success: 401 (Unauthorized)" khó hiểu, người dùng không
// biết phải đăng nhập lại. Nay: gặp 401 trên request ĐÃ gửi kèm Bearer token → xoá token, báo 1 lần bằng
// tiếng Việt, đưa về màn đăng nhập (giống nút Đăng xuất).
//
// Không đụng tới 401 của chính request đăng nhập (sai mật khẩu): request đó không mang Authorization
// header nên bị bỏ qua ở đây.
public sealed class UnauthorizedResponseHandler : DelegatingHandler
{
    private static readonly object Gate = new();

    private readonly IAuthTokenStorage _tokenStorage;
    private readonly IServiceProvider  _services;
    private readonly ILogger<UnauthorizedResponseHandler> _logger;

    // IServiceProvider thay vì inject thẳng INavigationService/IPostLoginSyncService: handler này gắn vào
    // MỌI HttpClient nên inject trực tiếp dễ tạo vòng phụ thuộc (service → HttpClient → handler → service).
    public UnauthorizedResponseHandler(
        IAuthTokenStorage tokenStorage,
        IServiceProvider services,
        ILogger<UnauthorizedResponseHandler> logger)
    {
        _tokenStorage = tokenStorage;
        _services     = services;
        _logger       = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized && request.Headers.Authorization is not null)
            HandleSessionExpired(request);

        return response;
    }

    private void HandleSessionExpired(HttpRequestMessage request)
    {
        // Nhiều request chạy song song cùng nhận 401 → chỉ request ĐẦU TIÊN thấy còn token mới xử lý,
        // các request sau thấy token đã bị xoá và bỏ qua (không hiện hộp thoại lặp).
        lock (Gate)
        {
            if (!_tokenStorage.HasToken) return;
            _tokenStorage.Clear();
        }

        _logger.LogWarning("401 Unauthorized for {Method} {Uri} — session expired, returning to login",
            request.Method, request.RequestUri?.PathAndQuery);

        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null) return;

        _ = dispatcher.InvokeAsync(() =>
        {
            MessageBox.Show(
                "Phiên đăng nhập đã hết hạn hoặc không còn hợp lệ.\nVui lòng đăng nhập lại.",
                "Hết phiên đăng nhập",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            _ = _services.GetRequiredService<IPostLoginSyncService>().ShutdownAsync();
            _services.GetRequiredService<INavigationService>().NavigateToLogin();
        });
    }
}
