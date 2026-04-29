// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Accounting.Data.Services;
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;

public sealed class GetReceiptByIdUseCase : IGetReceiptByIdUseCase
{
    private readonly IReceiptService _service;

    public GetReceiptByIdUseCase(IReceiptService service) => _service = service;

    public Task<ReceiptResponseDto?> ExecuteAsync(int id, CancellationToken ct = default)
        => _service.GetByIdAsync(id, ct);
}
