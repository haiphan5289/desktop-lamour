// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.SalesReturn.Data.Services;
using DesktopLamour.Features.HomePage.SalesReturn.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.SalesReturn.Data.Repositories;

public class SalesReturnRepository : ISalesReturnRepository
{
    private readonly ISalesReturnService _service;

    public SalesReturnRepository(ISalesReturnService service) => _service = service;

    public Task<IEnumerable<SalesReturnResponseDto>> GetAllAsync(
        DateTime? fromDate = null, DateTime? toDate = null, string? search = null, CancellationToken ct = default)
        => _service.GetAllAsync(fromDate, toDate, search, ct);

    public Task<SalesReturnResponseDto?> GetByIdAsync(int id, CancellationToken ct = default)
        => _service.GetByIdAsync(id, ct);

    public Task<SalesReturnResponseDto> CreateAsync(CreateSalesReturnRequestDto request, CancellationToken ct = default)
        => _service.CreateAsync(request, ct);

    public Task<SalesReturnResponseDto> UpdateAsync(int id, UpdateSalesReturnRequestDto request, CancellationToken ct = default)
        => _service.UpdateAsync(id, request, ct);

    public Task DeleteAsync(int id, CancellationToken ct = default)
        => _service.DeleteAsync(id, ct);

    public Task<string> GetNextCodeAsync(CancellationToken ct = default)
        => _service.GetNextCodeAsync(ct);
}
