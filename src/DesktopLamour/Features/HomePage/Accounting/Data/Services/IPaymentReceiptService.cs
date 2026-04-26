// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Accounting.Data.Services;

public interface IPaymentReceiptService
{
    Task<PaymentReceiptResponseDto> CreateAsync(
        CreatePaymentReceiptRequestDto request,
        CancellationToken ct = default);
}
