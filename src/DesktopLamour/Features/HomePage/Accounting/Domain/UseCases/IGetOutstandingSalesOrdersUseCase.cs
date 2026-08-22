// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;

public interface IGetOutstandingSalesOrdersUseCase
{
    Task<IEnumerable<OutstandingSalesOrderDto>> ExecuteAsync(
        DateOnly fromDate, DateOnly toDate, int? employeeId = null, CancellationToken ct = default);
}
