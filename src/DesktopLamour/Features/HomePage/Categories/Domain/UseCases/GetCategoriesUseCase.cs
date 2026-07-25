// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Categories.Data.Repositories;
using DesktopLamour.Features.HomePage.Categories.Domain.Models;
namespace DesktopLamour.Features.HomePage.Categories.Domain.UseCases;

public sealed class GetCategoriesUseCase : IGetCategoriesUseCase
{
    private readonly ICategoryRepository _repository;
    public GetCategoriesUseCase(ICategoryRepository repository) => _repository = repository;

    public Task<IEnumerable<Category>> ExecuteAsync(CancellationToken ct = default)
        => _repository.GetAllAsync(ct);
}
