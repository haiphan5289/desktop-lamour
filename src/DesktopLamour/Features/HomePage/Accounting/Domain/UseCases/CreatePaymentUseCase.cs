// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Accounting.Data.Services;
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;

public sealed class CreatePaymentUseCase : ICreatePaymentUseCase
{
    private readonly IPaymentService _service;

    public CreatePaymentUseCase(IPaymentService service) => _service = service;

    public Task<PaymentResponseDto> ExecuteAsync(CreatePaymentRequestDto request, CancellationToken ct = default)
        => _service.CreateAsync(request, ct);
}
