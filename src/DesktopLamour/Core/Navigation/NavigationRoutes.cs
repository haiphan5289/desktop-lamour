// NavigationRoutes.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

namespace DesktopLamour.Core.Navigation;

public static class NavigationRoutes
{
    public static class Home
    {
        public const string Dashboard = "HomeView";
    }

    public static class Products
    {
        public const string List = "ProductListView";
    }

    public static class Suppliers
    {
        public const string List = "SupplierListView";
    }

    public static class Customers
    {
        public const string List = "CustomerListView";
    }

    public static class Employees
    {
        public const string List = "EmployeeListView";
    }

    public static class Warehouse
    {
        public const string Hub           = "WarehouseView";
        public const string TongHopTonKho = "TongHopTonKhoView";
        public const string PhieuNhapKho  = "WarehouseReceiptListView";
    }

    public static class Accounting
    {
        public const string Hub = "AccountingView";
    }


    public const string ProductList  = "ProductListView";
    public const string SupplierList = "SupplierListView";
    public const string CustomerList = "CustomerListView";
    public const string EmployeeList = "EmployeeListView";
    public const string Register     = "RegisterView";
    public const string Login        = "LoginView";
    public const string Main         = "MainView";
}
