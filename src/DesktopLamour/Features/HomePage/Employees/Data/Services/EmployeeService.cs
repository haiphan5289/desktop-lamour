// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Core.Storage;
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
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(HttpClient httpClient, IAuthTokenStorage tokenStorage, ILogger<EmployeeService> logger)
    {
        _httpClient   = httpClient;
        _tokenStorage = tokenStorage;
        _logger       = logger;
    }

    public async Task<IEnumerable<EmployeeResponseDto>> GetAllAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching all employees from API");
        SetBearerToken();

        var response = await _httpClient.GetAsync("/api/v1/employees", ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<IEnumerable<EmployeeResponseDto>>(ct)
            ?? Enumerable.Empty<EmployeeResponseDto>();
    }

    public async Task<EmployeeResponseDto> CreateAsync(CreateEmployeeRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating employee {Name}", request.Name);
        SetBearerToken();

        var response = await _httpClient.PostAsJsonAsync("/api/v1/employees", request, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<EmployeeResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from create employee endpoint.");
    }

    public async Task<EmployeeResponseDto> UpdateAsync(int employeeId, UpdateEmployeeRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating employee {Id}", employeeId);
        SetBearerToken();

        var response = await _httpClient.PutAsJsonAsync($"/api/v1/employees/{employeeId}", request, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<EmployeeResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from update employee endpoint.");
    }

    public async Task DeleteAsync(int employeeId, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting employee {Id}", employeeId);
        SetBearerToken();

        var response = await _httpClient.DeleteAsync($"/api/v1/employees/{employeeId}", ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task<EmployeeResponseDto> DuplicateAsync(int employeeId, CancellationToken ct = default)
    {
        _logger.LogInformation("Duplicating employee {Id}", employeeId);
        SetBearerToken();

        var response = await _httpClient.PostAsync($"/api/v1/employees/{employeeId}/duplicate", null, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<EmployeeResponseDto>(ct)
            ?? throw new InvalidOperationException("Empty response from duplicate employee endpoint.");
    }

    private void SetBearerToken()
    {
        var token = _tokenStorage.GetToken();
        _httpClient.DefaultRequestHeaders.Authorization =
            token is not null ? new AuthenticationHeaderValue("Bearer", token) : null;
    }
}
