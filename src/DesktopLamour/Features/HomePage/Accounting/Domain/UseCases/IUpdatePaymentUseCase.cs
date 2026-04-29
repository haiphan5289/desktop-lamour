// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;

public interface IUpdatePaymentUseCase
{
    Task<PaymentResponseDto> ExecuteAsync(int id, UpdatePaymentRequestDto request, CancellationToken ct = default);
}
