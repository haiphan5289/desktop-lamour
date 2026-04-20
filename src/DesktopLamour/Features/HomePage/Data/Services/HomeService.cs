// HomeService.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using System.Net.Http;
using System.Net.Http.Json;
using DesktopLamour.Features.HomePage.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Data.Services;

public sealed class HomeService : IHomeService
{
    private readonly HttpClient _httpClient;

    public HomeService(HttpClient httpClient)
        => _httpClient = httpClient;

    public async Task<IEnumerable<ProductResponseDto>> GetProductsAsync(CancellationToken ct = default)
    {
        var result = await _httpClient.GetFromJsonAsync<IEnumerable<ProductResponseDto>>("/products", ct);
        return result ?? Enumerable.Empty<ProductResponseDto>();
    }

    public async Task<IEnumerable<SupplierResponseDto>> GetSuppliersAsync(CancellationToken ct = default)
    {
        var result = await _httpClient.GetFromJsonAsync<IEnumerable<SupplierResponseDto>>("/suppliers", ct);
        return result ?? Enumerable.Empty<SupplierResponseDto>();
    }
}
