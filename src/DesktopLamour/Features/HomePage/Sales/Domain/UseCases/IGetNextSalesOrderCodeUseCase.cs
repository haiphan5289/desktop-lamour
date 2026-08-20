// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Sales.Domain.UseCases;

public interface IGetNextSalesOrderCodeUseCase
{
    // isFromWarehouseExport: true → prefix "XK" (mở từ "Xuất kho bán hàng"), false → prefix "BH"
    // (mở từ "Bán hàng"). 2 chuỗi số đếm độc lập — mọi business rule khác giữ nguyên như nhau.
    Task<string> ExecuteAsync(bool isFromWarehouseExport = true, CancellationToken ct = default);
}
