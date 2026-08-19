// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Suppliers.Data.Services;
using DesktopLamour.Features.HomePage.Suppliers.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Suppliers.Domain.Models;
using DesktopLamour.Features.HomePage.Suppliers.Domain.UseCases;
using System.IO;
using System.Linq;
namespace DesktopLamour.Features.HomePage.Suppliers.Data.Repositories;

public sealed class SupplierRepository : ISupplierRepository
{
    private readonly ISupplierService _service;
    public SupplierRepository(ISupplierService service) => _service = service;

    public async Task<IEnumerable<Supplier>> GetAllAsync(CancellationToken ct = default)
    {
        var dtos = await _service.GetAllAsync(ct);
        return dtos.Select(d => new Supplier
        {
            Id             = d.Id,
            Code           = d.Code,
            Name           = d.Name,
            Address        = d.Address,
            Group          = d.Group,
            TaxCode        = d.TaxCode,
            Phone          = d.Phone,
            IsStopTracking = d.IsStopTracking
        });
    }

    public Task DeleteAsync(int supplierId, CancellationToken ct = default)
        => _service.DeleteAsync(supplierId, ct);

    public async Task<Supplier> DuplicateAsync(int supplierId, CancellationToken ct = default)
    {
        var d = await _service.DuplicateAsync(supplierId, ct);
        return new Supplier
        {
            Id             = d.Id,
            Code           = d.Code,
            Name           = d.Name,
            Address        = d.Address,
            Group          = d.Group,
            TaxCode        = d.TaxCode,
            Phone          = d.Phone,
            IsStopTracking = d.IsStopTracking
        };
    }

    public async Task<Supplier> CreateAsync(CreateSupplierInput input, CancellationToken ct = default)
    {
        var request = new CreateSupplierRequestDto
        {
            Code           = input.Code,
            Name           = input.Name,
            Phone          = input.Phone,
            Address        = input.Address,
            Group          = input.Group,
            TaxCode        = input.TaxCode,
            IsStopTracking = input.IsStopTracking
        };
        var d = await _service.CreateAsync(request, ct);
        return new Supplier
        {
            Id             = d.Id,
            Code           = d.Code,
            Name           = d.Name,
            Phone          = d.Phone,
            Address        = d.Address,
            Group          = d.Group,
            TaxCode        = d.TaxCode,
            IsStopTracking = d.IsStopTracking
        };
    }

    public async Task<Supplier> UpdateAsync(UpdateSupplierInput input, CancellationToken ct = default)
    {
        var request = new UpdateSupplierRequestDto
        {
            Code           = input.Code,
            Name           = input.Name,
            Phone          = input.Phone,
            Address        = input.Address,
            Group          = input.Group,
            TaxCode        = input.TaxCode,
            IsStopTracking = input.IsStopTracking
        };
        var d = await _service.UpdateAsync(input.Id, request, ct);
        return new Supplier
        {
            Id             = d.Id,
            Code           = d.Code,
            Name           = d.Name,
            Phone          = d.Phone,
            Address        = d.Address,
            Group          = d.Group,
            TaxCode        = d.TaxCode,
            IsStopTracking = d.IsStopTracking
        };
    }

    public async Task<ImportSupplierResult> ImportExcelAsync(Stream fileStream, string fileName, CancellationToken ct = default)
    {
        var dto = await _service.ImportExcelAsync(fileStream, fileName, ct);
        return new ImportSupplierResult(
            dto.Total,
            dto.Imported,
            dto.Skipped,
            dto.Errors.Select(e => new ImportRowError(e.Row, e.Reason)).ToList());
    }
}
