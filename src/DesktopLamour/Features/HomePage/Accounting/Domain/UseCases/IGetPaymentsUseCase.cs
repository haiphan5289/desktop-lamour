// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;

public interface IGetPaymentsUseCase
{
    Task<IEnumerable<PaymentResponseDto>> ExecuteAsync(CancellationToken ct = default);
}
