// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Accounting.Data.Services;
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;

public sealed class GetReceiptsUseCase : IGetReceiptsUseCase
{
    private readonly IReceiptService _service;

    public GetReceiptsUseCase(IReceiptService service) => _service = service;

    public Task<IEnumerable<ReceiptResponseDto>> ExecuteAsync(CancellationToken ct = default)
        => _service.GetAllAsync(ct);
}
