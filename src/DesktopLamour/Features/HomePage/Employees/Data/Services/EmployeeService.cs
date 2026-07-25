// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Storage;
using DesktopLamour.Features.HomePage.Employees.Data.Cache;
using DesktopLamour.Features.HomePage.Employees.Data.Services.Dtos;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DesktopLamour.Features.HomePage.Employees.Data.Services;

public sealed class EmployeeService : IEmployeeService
{
    private readonly HttpClient              _httpClient;
    private readonly IAuthTokenStorage       _tokenStorage;
    private readonly IEmployeeCacheStore     _cache;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(HttpClient httpClient, IAuthTokenStorage tokenStorage, IEmployeeCacheStore cache, ILogger<EmployeeService> logger)
    {
        _httpClient   = httpClient;
        _tokenStorage = tokenStorage;
        _cache        = cache;
        _logger       = logger;
    }

    public async Task<IEnumerable<EmployeeResponseDto>> GetAllAsync(CancellationToken ct = default)
    {
        if (_cache.IsInitialized)
        {
            _logger.LogInformation("Returning employees from local cache");
            return _cache.GetAll();
        }

        _logger.LogInformation("Fetching all employees from API");
        SetBearerToken();

        var response = await _httpClient.GetAsync("/api/v1/employees", ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<IEnumerable<EmployeeResponseDto>>(ct)
            ?? Enumerable.Empty<EmployeeResponseDto>();

        _cache.SetAll(result);
        return result;
    }

    public async Task<EmployeeResponseDto> CreateAsync(CreateEmployeeRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating employee {Name}", request.Name);
        SetBearerToken();

        var response = await _httpClient.PostAsJsonAsync("/api/v1/employees", request, ct);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<EmployeeResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from create employee endpoint.");
        _cache.Upsert(created);
        return created;
    }

    public async Task<EmployeeResponseDto> UpdateAsync(int employeeId, UpdateEmployeeRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating employee {Id}", employeeId);
        SetBearerToken();

        var response = await _httpClient.PutAsJsonAsync($"/api/v1/employees/{employeeId}", request, ct);
        response.EnsureSuccessStatusCode();

        var updated = await response.Content.ReadFromJsonAsync<EmployeeResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from update employee endpoint.");
        _cache.Upsert(updated);
        return updated;
    }

    public async Task DeleteAsync(int employeeId, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting employee {Id}", employeeId);
        SetBearerToken();

        var response = await _httpClient.DeleteAsync($"/api/v1/employees/{employeeId}", ct);
        response.EnsureSuccessStatusCode();
        _cache.Remove(employeeId);
    }

    public async Task<EmployeeResponseDto> DuplicateAsync(int employeeId, CancellationToken ct = default)
    {
        _logger.LogInformation("Duplicating employee {Id}", employeeId);
        SetBearerToken();

        var response = await _httpClient.PostAsync($"/api/v1/employees/{employeeId}/duplicate", null, ct);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<EmployeeResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from duplicate employee endpoint.");
        _cache.Upsert(created);
        return created;
    }

    private void SetBearerToken()
    {
        var token = _tokenStorage.GetToken();
        _httpClient.DefaultRequestHeaders.Authorization =
            token is not null ? new AuthenticationHeaderValue("Bearer", token) : null;
    }
}
