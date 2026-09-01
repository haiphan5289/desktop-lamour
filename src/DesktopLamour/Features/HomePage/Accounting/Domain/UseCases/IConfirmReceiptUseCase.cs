// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;

public interface IConfirmReceiptUseCase
{
    Task<ReceiptResponseDto> ExecuteAsync(int id, CancellationToken ct = default);
}
