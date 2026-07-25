// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Storage;
using DesktopLamour.Features.HomePage.Customers.Data.Cache;
using DesktopLamour.Features.HomePage.Customers.Data.Services.Dtos;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DesktopLamour.Features.HomePage.Customers.Data.Services;

public sealed class CustomerService : ICustomerService
{
    private readonly HttpClient              _httpClient;
    private readonly IAuthTokenStorage       _tokenStorage;
    private readonly ICustomerCacheStore     _cache;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(
        HttpClient httpClient,
        IAuthTokenStorage tokenStorage,
        ICustomerCacheStore cache,
        ILogger<CustomerService> logger)
    {
        _httpClient   = httpClient;
        _tokenStorage = tokenStorage;
        _cache        = cache;
        _logger       = logger;
    }

    public async Task<IEnumerable<CustomerResponseDto>> GetAllAsync(CancellationToken ct = default)
    {
        if (_cache.IsInitialized)
        {
            _logger.LogInformation("Returning customers from local cache");
            return _cache.GetAll();
        }

        _logger.LogInformation("Fetching all customers from API");
        SetBearerToken();

        var response = await _httpClient.GetAsync("/api/v1/customers", ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<IEnumerable<CustomerResponseDto>>(ct)
            ?? Enumerable.Empty<CustomerResponseDto>();

        _cache.SetAll(result);
        return result;
    }

    public async Task<string> GetNextCodeAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching next customer code from API");
        SetBearerToken();

        var response = await _httpClient.GetAsync("/api/v1/customers/next-code", ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<NextCodeResponse>(ct);
        return result?.Code ?? "KH00001";
    }

    public async Task<CustomerResponseDto> CreateAsync(CreateCustomerRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating customer with name {Name}", request.Name);
        SetBearerToken();

        var response = await _httpClient.PostAsJsonAsync("/api/v1/customers", request, ct);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<CustomerResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from create customer endpoint.");
        _cache.Upsert(created);
        return created;
    }

    public async Task<CustomerResponseDto> UpdateAsync(int customerId, UpdateCustomerRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating customer {Id}", customerId);
        SetBearerToken();

        var response = await _httpClient.PutAsJsonAsync($"/api/v1/customers/{customerId}", request, ct);
        response.EnsureSuccessStatusCode();

        var updated = await response.Content.ReadFromJsonAsync<CustomerResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from update customer endpoint.");
        _cache.Upsert(updated);
        return updated;
    }

    public async Task DeleteAsync(int customerId, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting customer {Id}", customerId);
        SetBearerToken();

        var response = await _httpClient.DeleteAsync($"/api/v1/customers/{customerId}", ct);
        response.EnsureSuccessStatusCode();
        _cache.Remove(customerId);
    }

    public async Task<CustomerResponseDto> DuplicateAsync(int customerId, CancellationToken ct = default)
    {
        _logger.LogInformation("Duplicating customer {Id}", customerId);
        SetBearerToken();

        var response = await _httpClient.PostAsync($"/api/v1/customers/{customerId}/duplicate", null, ct);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<CustomerResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from duplicate customer endpoint.");
        _cache.Upsert(created);
        return created;
    }

    public async Task<ImportCustomerResultDto> ImportExcelAsync(Stream fileStream, string fileName, CancellationToken ct = default)
    {
        _logger.LogInformation("Importing customers from Excel file {FileName}", fileName);
        SetBearerToken();

        using var content       = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        content.Add(streamContent, "file", fileName);

        var response = await _httpClient.PostAsync("/api/v1/customers/import-excel", content, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ImportCustomerResultDto>(ct)
            ?? throw new InvalidOperationException("Empty response from import-excel endpoint.");
        _cache.Clear(); // bulk import → force next GetAllAsync to refetch the full list
        return result;
    }

    private void SetBearerToken()
    {
        var token = _tokenStorage.GetToken();
        _httpClient.DefaultRequestHeaders.Authorization =
            token is not null
                ? new AuthenticationHeaderValue("Bearer", token)
                : null;
    }

    private sealed record NextCodeResponse(
        [property: System.Text.Json.Serialization.JsonPropertyName("code")] string Code);
}
