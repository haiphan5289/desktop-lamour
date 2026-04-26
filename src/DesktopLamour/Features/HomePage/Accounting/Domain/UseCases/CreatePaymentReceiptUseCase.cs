// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Accounting.Data.Services;
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;

public sealed class CreatePaymentReceiptUseCase : ICreatePaymentReceiptUseCase
{
    private readonly IPaymentReceiptService _service;

    public CreatePaymentReceiptUseCase(IPaymentReceiptService service)
        => _service = service;

    public Task<PaymentReceiptResponseDto> ExecuteAsync(
        CreatePaymentReceiptRequestDto request,
        CancellationToken ct = default)
        => _service.CreateAsync(request, ct);
}
