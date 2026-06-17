// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.SalesReturn.Data.Repositories;
using DesktopLamour.Features.HomePage.SalesReturn.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.SalesReturn.Domain.UseCases;

public class UpdateSalesReturnUseCase : IUpdateSalesReturnUseCase
{
    private readonly ISalesReturnRepository _repository;

    public UpdateSalesReturnUseCase(ISalesReturnRepository repository) => _repository = repository;

    public Task<SalesReturnResponseDto> ExecuteAsync(int id, UpdateSalesReturnRequestDto request, CancellationToken ct = default)
        => _repository.UpdateAsync(id, request, ct);
}
