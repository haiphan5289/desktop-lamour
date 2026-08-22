// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Accounting.Data.Services;

namespace DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;

public sealed class GetNextReceiptCodeUseCase : IGetNextReceiptCodeUseCase
{
    private readonly IReceiptService _service;

    public GetNextReceiptCodeUseCase(IReceiptService service) => _service = service;

    public Task<string> ExecuteAsync(CancellationToken ct = default)
        => _service.GetNextCodeAsync(ct);
}
