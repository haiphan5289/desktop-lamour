// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Accounting.Data.Services;

public interface IPaymentService
{
    Task<IEnumerable<PaymentResponseDto>> GetAllAsync(CancellationToken ct = default);
    Task<PaymentResponseDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PaymentResponseDto> CreateAsync(CreatePaymentRequestDto request, CancellationToken ct = default);
    Task<PaymentResponseDto> UpdateAsync(int id, UpdatePaymentRequestDto request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<PaymentResponseDto> DuplicateAsync(int id, CancellationToken ct = default);
    Task<PaymentResponseDto> ConfirmAsync(int id, CancellationToken ct = default);
    Task<PaymentResponseDto> TreoAsync(int id, CancellationToken ct = default);
}
