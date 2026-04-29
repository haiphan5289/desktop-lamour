// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Accounting.Data.Services;
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;

public sealed class UpdateReceiptUseCase : IUpdateReceiptUseCase
{
    private readonly IReceiptService _service;

    public UpdateReceiptUseCase(IReceiptService service) => _service = service;

    public Task<ReceiptResponseDto> ExecuteAsync(int id, UpdateReceiptRequestDto request, CancellationToken ct = default)
        => _service.UpdateAsync(id, request, ct);
}
