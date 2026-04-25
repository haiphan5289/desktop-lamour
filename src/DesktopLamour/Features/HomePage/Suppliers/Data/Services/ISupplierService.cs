// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Suppliers.Data.Services.Dtos;
namespace DesktopLamour.Features.HomePage.Suppliers.Data.Services;

public interface ISupplierService
{
    Task<IEnumerable<SupplierResponseDto>> GetAllAsync(CancellationToken ct = default);
    Task DeleteAsync(int supplierId, CancellationToken ct = default);
    Task<SupplierResponseDto> DuplicateAsync(int supplierId, CancellationToken ct = default);
    Task<SupplierResponseDto> CreateAsync(CreateSupplierRequestDto request, CancellationToken ct = default);
    Task<SupplierResponseDto> UpdateAsync(int supplierId, UpdateSupplierRequestDto request, CancellationToken ct = default);
}
