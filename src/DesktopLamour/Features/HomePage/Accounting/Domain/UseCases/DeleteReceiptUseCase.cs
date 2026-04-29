// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Accounting.Data.Services;

namespace DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;

public sealed class DeleteReceiptUseCase : IDeleteReceiptUseCase
{
    private readonly IReceiptService _service;

    public DeleteReceiptUseCase(IReceiptService service) => _service = service;

    public Task ExecuteAsync(int id, CancellationToken ct = default)
        => _service.DeleteAsync(id, ct);
}
