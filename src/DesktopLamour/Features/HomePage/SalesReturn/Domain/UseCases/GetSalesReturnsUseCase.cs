// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.SalesReturn.Data.Repositories;
using DesktopLamour.Features.HomePage.SalesReturn.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.SalesReturn.Domain.UseCases;

public class GetSalesReturnsUseCase : IGetSalesReturnsUseCase
{
    private readonly ISalesReturnRepository _repository;

    public GetSalesReturnsUseCase(ISalesReturnRepository repository) => _repository = repository;

    public Task<IEnumerable<SalesReturnResponseDto>> ExecuteAsync(CancellationToken ct = default)
        => _repository.GetAllAsync(ct);
}
