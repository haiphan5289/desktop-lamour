// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Customers.Domain.Models;
using DesktopLamour.Features.HomePage.Customers.Domain.UseCases;
using System.IO;
namespace DesktopLamour.Features.HomePage.Customers.Data.Repositories;

public interface ICustomerRepository
{
    Task<IEnumerable<Customer>> GetAllAsync(CancellationToken ct = default);
    Task DeleteAsync(int customerId, CancellationToken ct = default);
    Task<Customer> DuplicateAsync(int customerId, CancellationToken ct = default);
    Task<Customer> CreateAsync(CreateCustomerInput input, CancellationToken ct = default);
    Task<Customer> UpdateAsync(UpdateCustomerInput input, CancellationToken ct = default);
    Task<ImportCustomerResult> ImportExcelAsync(Stream fileStream, string fileName, CancellationToken ct = default);
}
