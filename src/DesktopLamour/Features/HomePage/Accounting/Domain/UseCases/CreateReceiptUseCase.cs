// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Accounting.Data.Services;
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;

public sealed class CreateReceiptUseCase : ICreateReceiptUseCase
{
    private readonly IReceiptService _service;

    public CreateReceiptUseCase(IReceiptService service) => _service = service;

    public Task<ReceiptResponseDto> ExecuteAsync(CreateReceiptRequestDto request, CancellationToken ct = default)
        => _service.CreateAsync(request, ct);
}
