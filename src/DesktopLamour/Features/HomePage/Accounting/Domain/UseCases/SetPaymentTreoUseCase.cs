// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Accounting.Data.Services;
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;

public sealed class SetPaymentTreoUseCase : ISetPaymentTreoUseCase
{
    private readonly IPaymentService _service;

    public SetPaymentTreoUseCase(IPaymentService service) => _service = service;

    public Task<PaymentResponseDto> ExecuteAsync(int id, CancellationToken ct = default)
        => _service.TreoAsync(id, ct);
}
