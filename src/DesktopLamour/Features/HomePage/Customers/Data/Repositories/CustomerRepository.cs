// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Customers.Data.Services;
using DesktopLamour.Features.HomePage.Customers.Data.Services.Dtos;
using DesktopLamour.Features.HomePage.Customers.Domain.Models;
using DesktopLamour.Features.HomePage.Customers.Domain.UseCases;
using System.IO;
namespace DesktopLamour.Features.HomePage.Customers.Data.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly ICustomerService _service;
    public CustomerRepository(ICustomerService service) => _service = service;

    public async Task<IEnumerable<Customer>> GetAllAsync(CancellationToken ct = default)
    {
        var dtos = await _service.GetAllAsync(ct);
        return dtos.Select(MapToModel);
    }

    public Task DeleteAsync(int customerId, CancellationToken ct = default)
        => _service.DeleteAsync(customerId, ct);

    public async Task<Customer> DuplicateAsync(int customerId, CancellationToken ct = default)
    {
        var d = await _service.DuplicateAsync(customerId, ct);
        return MapToModel(d);
    }

    public async Task<Customer> CreateAsync(CreateCustomerInput input, CancellationToken ct = default)
    {
        var request = new CreateCustomerRequestDto
        {
            Name          = input.Name,
            Phone         = input.Phone,
            Address       = input.Address,
            Province      = input.Province,
            CustomerGroup = input.CustomerGroup,
            TaxCode       = input.TaxCode,
            SaleCare      = input.SaleCare,
        };
        var d = await _service.CreateAsync(request, ct);
        return MapToModel(d);
    }

    public async Task<Customer> UpdateAsync(UpdateCustomerInput input, CancellationToken ct = default)
    {
        var request = new UpdateCustomerRequestDto
        {
            Name          = input.Name,
            Phone         = input.Phone,
            Address       = input.Address,
            Province      = input.Province,
            CustomerGroup = input.CustomerGroup,
            TaxCode       = input.TaxCode,
            SaleCare      = input.SaleCare,
        };
        var d = await _service.UpdateAsync(input.Id, request, ct);
        return MapToModel(d);
    }

    public async Task<ImportCustomerResult> ImportExcelAsync(Stream fileStream, string fileName, CancellationToken ct = default)
    {
        var dto = await _service.ImportExcelAsync(fileStream, fileName, ct);
        return new ImportCustomerResult(
            dto.Total,
            dto.Imported,
            dto.Skipped,
            dto.Errors.Select(e => new ImportRowError(e.Row, e.Reason)).ToList());
    }

    private static Customer MapToModel(CustomerResponseDto d) => new()
    {
        Id            = d.Id,
        Code          = d.Code,
        Name          = d.Name,
        Address       = d.Address,
        Province      = d.Province,
        CustomerGroup = d.CustomerGroup,
        TaxCode       = d.TaxCode,
        Phone         = d.Phone,
        SaleCare      = d.SaleCare,
    };
}
