// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;

public interface IDuplicatePaymentUseCase
{
    Task<PaymentResponseDto> ExecuteAsync(int id, CancellationToken ct = default);
}
