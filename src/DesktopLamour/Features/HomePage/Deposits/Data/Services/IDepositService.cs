// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Deposits.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Deposits.Data.Services;

public interface IDepositService
{
    Task<IEnumerable<DepositResponseDto>> GetAllAsync(CancellationToken ct = default);
    Task<DepositResponseDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<string> GetNextCodeAsync(CancellationToken ct = default);
    Task<IEnumerable<DepositResponseDto>> GetByCustomerAsync(int customerId, int? excludeSalesOrderId = null, CancellationToken ct = default);
    Task<DepositResponseDto> CreateAsync(CreateDepositRequestDto request, CancellationToken ct = default);
    Task<DepositResponseDto> UpdateAsync(int id, UpdateDepositRequestDto request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
