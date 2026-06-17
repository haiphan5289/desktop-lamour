// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.SalesReturn.Data.Repositories;

namespace DesktopLamour.Features.HomePage.SalesReturn.Domain.UseCases;

public class DeleteSalesReturnUseCase : IDeleteSalesReturnUseCase
{
    private readonly ISalesReturnRepository _repository;

    public DeleteSalesReturnUseCase(ISalesReturnRepository repository) => _repository = repository;

    public Task ExecuteAsync(int id, CancellationToken ct = default)
        => _repository.DeleteAsync(id, ct);
}
