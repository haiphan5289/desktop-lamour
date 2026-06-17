// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.SalesReturn.Data.Repositories;

namespace DesktopLamour.Features.HomePage.SalesReturn.Domain.UseCases;

public class GetNextSalesReturnCodeUseCase : IGetNextSalesReturnCodeUseCase
{
    private readonly ISalesReturnRepository _repository;

    public GetNextSalesReturnCodeUseCase(ISalesReturnRepository repository) => _repository = repository;

    public Task<string> ExecuteAsync(CancellationToken ct = default)
        => _repository.GetNextCodeAsync(ct);
}
