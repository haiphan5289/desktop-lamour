// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Sales.Domain.Models;

public static class SalesOrderReportTypes
{
    public const string ByProduct              = "Mặt hàng";
    public const string ByProductThenCustomer   = "Mặt hàng & khách hàng";
    public const string ByProductThenEmployee   = "Mặt hàng & nhân viên";
    public const string ByCustomer              = "Khách hàng";
    public const string ByEmployee              = "Nhân viên";
    // 2026-09-21: bỏ ByCustomerThenEmployee ("Khách hàng & nhân viên") và ByCustomerThenProduct
    // ("Khách hàng & mặt hàng") — trùng ý nghĩa với ByProductThenCustomer/ByEmployeeThenCustomer
    // (chỉ đảo thứ tự nhóm ngoài/trong), và 2 report bị bỏ chưa có ảnh mẫu MISA xác nhận trong khi
    // 2 report giữ lại đã confirmed. Xem ColumnConfigs/GroupingsByType trong SalesOrderReportViewModel.
    public const string ByEmployeeThenCustomer  = "Nhân viên & khách hàng";

    // 2026-09-18: "Đơn vị kinh doanh" — group theo Employee.Unit đã có sẵn (không phải entity mới).
    public const string ByEmployeeUnit = "Đơn vị kinh doanh";

    // 2026-09-18: report 3 CHIỀU cố định (case riêng, không tổng quát hoá kiến trúc cho N chiều) —
    // khớp báo cáo MISA "Tổng hợp bán hàng theo nhân viên, khách hàng và mặt hàng". Thứ tự cố định:
    // Nhân viên (ngoài, gom nhóm) > Khách hàng (giữa, cột phẳng) > Mặt hàng (trong, cột Mã/Tên hàng
    // sẵn có).
    public const string ByEmployeeCustomerProduct = "Nhân viên, khách hàng và mặt hàng";

    public static readonly IReadOnlyList<string> All = new[]
    {
        ByProduct, ByProductThenCustomer, ByProductThenEmployee,
        ByCustomer, ByEmployee,
        ByEmployeeThenCustomer, ByEmployeeUnit, ByEmployeeCustomerProduct,
    };
}
