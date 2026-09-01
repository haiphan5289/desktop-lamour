// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.SalesReturn.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.SalesReturn.Data.Repositories;

public interface ISalesReturnRepository
{
    Task<IEnumerable<SalesReturnResponseDto>> GetAllAsync(
        DateTime? fromDate = null, DateTime? toDate = null, string? search = null, CancellationToken ct = default);
    Task<SalesReturnResponseDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<SalesReturnResponseDto> CreateAsync(CreateSalesReturnRequestDto request, CancellationToken ct = default);
    Task<SalesReturnResponseDto> UpdateAsync(int id, UpdateSalesReturnRequestDto request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<string> GetNextCodeAsync(CancellationToken ct = default);
    Task<CreateWarehouseReceiptResultDto> CreateWarehouseReceiptAsync(int id, CancellationToken ct = default);
    Task<SalesReturnResponseDto> ConfirmAsync(int id, CancellationToken ct = default);
    Task<SalesReturnResponseDto> UnconfirmAsync(int id, CancellationToken ct = default);
}
