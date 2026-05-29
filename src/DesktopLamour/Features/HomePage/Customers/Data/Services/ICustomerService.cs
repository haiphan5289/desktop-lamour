// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Customers.Data.Services.Dtos;
using System.IO;
namespace DesktopLamour.Features.HomePage.Customers.Data.Services;

public interface ICustomerService
{
    Task<IEnumerable<CustomerResponseDto>> GetAllAsync(CancellationToken ct = default);
    Task<string> GetNextCodeAsync(CancellationToken ct = default);
    Task DeleteAsync(int customerId, CancellationToken ct = default);
    Task<CustomerResponseDto> DuplicateAsync(int customerId, CancellationToken ct = default);
    Task<CustomerResponseDto> CreateAsync(CreateCustomerRequestDto request, CancellationToken ct = default);
    Task<CustomerResponseDto> UpdateAsync(int customerId, UpdateCustomerRequestDto request, CancellationToken ct = default);
    Task<ImportCustomerResultDto> ImportExcelAsync(Stream fileStream, string fileName, CancellationToken ct = default);
}
