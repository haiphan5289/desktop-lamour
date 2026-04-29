// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Accounting.Data.Services;

public interface IReceiptService
{
    Task<IEnumerable<ReceiptResponseDto>> GetAllAsync(CancellationToken ct = default);
    Task<ReceiptResponseDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ReceiptResponseDto> CreateAsync(CreateReceiptRequestDto request, CancellationToken ct = default);
    Task<ReceiptResponseDto> UpdateAsync(int id, UpdateReceiptRequestDto request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
