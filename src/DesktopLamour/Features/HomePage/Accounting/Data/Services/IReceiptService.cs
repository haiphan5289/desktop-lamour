// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Accounting.Data.Services.Dtos;

namespace DesktopLamour.Features.HomePage.Accounting.Data.Services;

public interface IReceiptService
{
    Task<IEnumerable<ReceiptResponseDto>> GetAllAsync(CancellationToken ct = default);
    Task<ReceiptResponseDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ReceiptResponseDto> CreateAsync(CreateReceiptRequestDto request, CancellationToken ct = default);
    Task<ReceiptResponseDto> UpdateAsync(int id, UpdateReceiptRequestDto request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<ReceiptResponseDto> ConfirmAsync(int id, CancellationToken ct = default);
    Task<ReceiptResponseDto> UnconfirmAsync(int id, CancellationToken ct = default);

    Task<string> GetNextCodeAsync(CancellationToken ct = default);

    Task<IEnumerable<OutstandingSalesOrderDto>> GetOutstandingSalesOrdersAsync(
        DateOnly fromDate, DateOnly toDate, int? employeeId = null, CancellationToken ct = default);

    Task<CreateBulkCustomerReceiptResponseDto> CreateBulkAsync(
        CreateBulkCustomerReceiptRequestDto request, CancellationToken ct = default);

    // Dựng lại tab "2. Chứng từ" khi Sửa 1 phiếu thu hàng loạt đã lưu — ids lấy từ
    // ReceiptEntryDto.SalesOrderId của phiếu đó.
    Task<IEnumerable<OutstandingSalesOrderDto>> GetSalesOrdersByIdsAsync(
        IEnumerable<int> salesOrderIds, CancellationToken ct = default);
}
