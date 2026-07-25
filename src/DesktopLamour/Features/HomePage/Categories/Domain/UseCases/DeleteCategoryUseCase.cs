// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Categories.Data.Repositories;
namespace DesktopLamour.Features.HomePage.Categories.Domain.UseCases;

public sealed class DeleteCategoryUseCase : IDeleteCategoryUseCase
{
    private readonly ICategoryRepository _repository;
    public DeleteCategoryUseCase(ICategoryRepository repository) => _repository = repository;
    public Task ExecuteAsync(int categoryId, CancellationToken ct = default)
        => _repository.DeleteAsync(categoryId, ct);
}
