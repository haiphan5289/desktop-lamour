// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Accounting.Data.Services;
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;

public sealed class GetPaymentsUseCase : IGetPaymentsUseCase
{
    private readonly IPaymentService _service;

    public GetPaymentsUseCase(IPaymentService service) => _service = service;

    public Task<IEnumerable<PaymentResponseDto>> ExecuteAsync(CancellationToken ct = default)
        => _service.GetAllAsync(ct);
}
