// GetHomeDashboardUseCase.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using DesktopLamour.Features.HomePage.Data.Repositories;
using DesktopLamour.Features.HomePage.Domain.Models;

namespace DesktopLamour.Features.HomePage.Domain.UseCases;

public sealed class GetHomeDashboardUseCase : IGetHomeDashboardUseCase
{
    private readonly IHomeRepository _repository;

    public GetHomeDashboardUseCase(IHomeRepository repository)
        => _repository = repository;

    public async Task<HomeDashboard> ExecuteAsync(CancellationToken ct = default)
    {
        var productsTask  = _repository.GetProductsAsync(ct);
        var suppliersTask = _repository.GetSuppliersAsync(ct);

        await Task.WhenAll(productsTask, suppliersTask);

        return new HomeDashboard(productsTask.Result, suppliersTask.Result);
    }
}
