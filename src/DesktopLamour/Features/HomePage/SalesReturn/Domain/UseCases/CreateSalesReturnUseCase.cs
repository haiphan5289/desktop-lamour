// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.SalesReturn.Data.Repositories;
using DesktopLamour.Features.HomePage.SalesReturn.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.SalesReturn.Domain.UseCases;

public class CreateSalesReturnUseCase : ICreateSalesReturnUseCase
{
    private readonly ISalesReturnRepository _repository;

    public CreateSalesReturnUseCase(ISalesReturnRepository repository) => _repository = repository;

    public Task<SalesReturnResponseDto> ExecuteAsync(CreateSalesReturnRequestDto request, CancellationToken ct = default)
        => _repository.CreateAsync(request, ct);
}
