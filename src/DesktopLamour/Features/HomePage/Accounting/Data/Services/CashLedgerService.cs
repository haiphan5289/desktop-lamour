// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Storage;
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Accounting.Domain.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DesktopLamour.Features.HomePage.Accounting.Data.Services;

public sealed class CashLedgerService : ICashLedgerService
{
    private readonly HttpClient               _httpClient;
    private readonly IAuthTokenStorage        _tokenStorage;
    private readonly ILogger<CashLedgerService> _logger;

    public CashLedgerService(
        HttpClient httpClient,
        IAuthTokenStorage tokenStorage,
        ILogger<CashLedgerService> logger)
    {
        _httpClient   = httpClient;
        _tokenStorage = tokenStorage;
        _logger       = logger;
    }

    public async Task<CashLedgerResponseDto> GetCashLedgerAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching cash ledger {From} → {To}", fromDate, toDate);
        SetBearerToken();

        var url = $"/api/v1/accounting/cash-ledger?from_date={fromDate:yyyy-MM-dd}&to_date={toDate:yyyy-MM-dd}";
        var response = await _httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<CashLedgerResponseDto>(ct)
            ?? new CashLedgerResponseDto();
    }

    public async Task<CashLedgerDetailReportDto> GetDetailReportAsync(
        CashLedgerReportFilter filter,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching cash ledger detail report {From} → {To}", filter.FromDate, filter.ToDate);
        SetBearerToken();

        var url = "/api/v1/accounting/reports/cash-ledger-detail"
                + $"?from_date={filter.FromDate:yyyy-MM-dd}&to_date={filter.ToDate:yyyy-MM-dd}"
                + $"&account_codes={Uri.EscapeDataString(string.Join(",", filter.AccountCodes))}"
                + $"&merge_similar={(filter.MergeSimilar ? "true" : "false")}"
                + $"&order_by_created={(filter.OrderByCreated ? "true" : "false")}";
        var response = await _httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<CashLedgerDetailReportDto>(ct)
            ?? new CashLedgerDetailReportDto();
    }

    private void SetBearerToken()
    {
        var token = _tokenStorage.GetToken();
        _httpClient.DefaultRequestHeaders.Authorization =
            token is not null
                ? new AuthenticationHeaderValue("Bearer", token)
                : null;
    }
}
