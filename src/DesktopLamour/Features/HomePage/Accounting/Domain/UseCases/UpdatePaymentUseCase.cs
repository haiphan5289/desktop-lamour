// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Accounting.Data.Services;
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;

public sealed class UpdatePaymentUseCase : IUpdatePaymentUseCase
{
    private readonly IPaymentService _service;

    public UpdatePaymentUseCase(IPaymentService service) => _service = service;

    public Task<PaymentResponseDto> ExecuteAsync(int id, UpdatePaymentRequestDto request, CancellationToken ct = default)
        => _service.UpdateAsync(id, request, ct);
}
