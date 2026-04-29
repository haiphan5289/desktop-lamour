// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;

public interface ICreateReceiptUseCase
{
    Task<ReceiptResponseDto> ExecuteAsync(CreateReceiptRequestDto request, CancellationToken ct = default);
}
