// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.SalesReturn.Data.Repositories;
using DesktopLamour.Features.HomePage.SalesReturn.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.SalesReturn.Domain.UseCases;

public class ConfirmSalesReturnUseCase : IConfirmSalesReturnUseCase
{
    private readonly ISalesReturnRepository _repository;

    public ConfirmSalesReturnUseCase(ISalesReturnRepository repository) => _repository = repository;

    public Task<SalesReturnResponseDto> ExecuteAsync(int id, CancellationToken ct = default)
        => _repository.ConfirmAsync(id, ct);
}
