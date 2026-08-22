// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Accounting.Data.Services;
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;

public sealed class CreateBulkCustomerReceiptUseCase : ICreateBulkCustomerReceiptUseCase
{
    private readonly IReceiptService _service;

    public CreateBulkCustomerReceiptUseCase(IReceiptService service) => _service = service;

    public Task<CreateBulkCustomerReceiptResponseDto> ExecuteAsync(
        CreateBulkCustomerReceiptRequestDto request, CancellationToken ct = default)
        => _service.CreateBulkAsync(request, ct);
}
