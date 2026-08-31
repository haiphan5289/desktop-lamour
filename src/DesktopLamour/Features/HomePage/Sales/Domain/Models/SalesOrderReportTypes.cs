// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Sales.Domain.Models;

public static class SalesOrderReportTypes
{
    public const string ByProduct              = "Mặt hàng";
    public const string ByProductThenCustomer   = "Mặt hàng & khách hàng";
    public const string ByProductThenEmployee   = "Mặt hàng & nhân viên";
    public const string ByCustomer              = "Khách hàng";
    public const string ByEmployee              = "Nhân viên";
    public const string ByCustomerThenEmployee  = "Khách hàng & nhân viên";
    public const string ByCustomerThenProduct   = "Khách hàng & mặt hàng";
    // Nhân viên trước, Khách hàng lồng bên trong — khớp báo cáo MISA "Tổng hợp bán hàng theo
    // nhân viên và khách hàng" (khác ByCustomerThenEmployee vốn nhóm theo Khách hàng trước).
    public const string ByEmployeeThenCustomer  = "Nhân viên & khách hàng";

    public static readonly IReadOnlyList<string> All = new[]
    {
        ByProduct, ByProductThenCustomer, ByProductThenEmployee,
        ByCustomer, ByEmployee, ByCustomerThenEmployee, ByCustomerThenProduct,
        ByEmployeeThenCustomer,
    };
}
