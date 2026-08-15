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

    public static class Categories
    {
        public const string List = "CategoryListView";
    }

    public static class ProductUnits
    {
        public const string List = "ProductUnitListView";
    }

    public static class AccountSettings
    {
        public const string List = "AccountSettingListView";
    }

    public static class Backup
    {
        public const string List = "BackupView";
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
        public const string TongHopTonKho = "TongHopTonKhoView";
        public const string PhieuNhapKho  = "WarehouseReceiptListView";
        public const string NhapXuatKho   = "WarehouseTransactionListView";
    }

    public static class Warehouses
    {
        public const string List = "WarehouseListView";
    }

    public static class Departments
    {
        public const string List = "DepartmentListView";
    }

    public static class ExpenseCategories
    {
        public const string List = "ExpenseCategoryListView";
    }

    public static class Sales
    {
        public const string Hub = "SalesView";
    }

    public static class SalesOrders
    {
        public const string List         = "SalesOrderListView";
        public const string Report       = "SalesOrderReportView";
        public const string ReportDetail = "SalesOrderReportDetailView";
    }

    public static class SalesReturns
    {
        public const string List = "SalesReturnListView";
    }

    public static class Accounting
    {
        public const string Hub = "AccountingView";
    }

    public static class Deposits
    {
        public const string DeductionReport = "DepositDeductionReportView";
    }


    public const string ProductList  = "ProductListView";
    public const string SupplierList = "SupplierListView";
    public const string CustomerList = "CustomerListView";
    public const string EmployeeList = "EmployeeListView";
    public const string Register     = "RegisterView";
    public const string Login        = "LoginView";
    public const string Main         = "MainView";
}
