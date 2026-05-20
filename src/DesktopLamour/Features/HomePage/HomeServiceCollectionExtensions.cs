// HomeServiceCollectionExtensions.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using DesktopLamour.Features.HomePage.Employees.Data.Repositories;
using DesktopLamour.Features.HomePage.Employees.Data.Services;
using DesktopLamour.Features.HomePage.Employees.Domain.UseCases;
using DesktopLamour.Features.HomePage.Employees.ViewModels;
using DesktopLamour.Features.HomePage.Employees.Views;
using DesktopLamour.Features.HomePage.Accounting.Data.Services;
using DesktopLamour.Features.HomePage.Accounting.Domain.UseCases;
using DesktopLamour.Features.HomePage.Accounting.ViewModels;
using DesktopLamour.Features.HomePage.Accounting.Views;
using DesktopLamour.Features.HomePage.Warehouse.Data.Repositories;
using DesktopLamour.Features.HomePage.Warehouse.Data.Services;
using DesktopLamour.Features.HomePage.Warehouse.Domain.UseCases;
using DesktopLamour.Features.HomePage.Warehouse.ViewModels;
using DesktopLamour.Features.HomePage.Warehouse.Views;
using DesktopLamour.Features.HomePage.Customers.Data.Repositories;
using DesktopLamour.Features.HomePage.Customers.Data.Services;
using DesktopLamour.Features.HomePage.Customers.Domain.UseCases;
using DesktopLamour.Features.HomePage.Customers.ViewModels;
using DesktopLamour.Features.HomePage.Customers.Views;
using DesktopLamour.Features.HomePage.Home.ViewModels;
using DesktopLamour.Features.HomePage.Home.Views;
using DesktopLamour.Features.HomePage.Sales.Data.Repositories;
using DesktopLamour.Features.HomePage.Sales.Data.Services;
using DesktopLamour.Features.HomePage.Sales.Domain.UseCases;
using DesktopLamour.Features.HomePage.Sales.ViewModels;
using DesktopLamour.Features.HomePage.Sales.Views;
using DesktopLamour.Features.HomePage.ProductList.Data.Repositories;
using DesktopLamour.Features.HomePage.ProductList.Data.Services;
using DesktopLamour.Features.HomePage.ProductList.Domain.UseCases;
using DesktopLamour.Features.HomePage.ProductList.ViewModels;
using DesktopLamour.Features.HomePage.ProductList.Views;
using DesktopLamour.Features.HomePage.Suppliers.Data.Repositories;
using DesktopLamour.Features.HomePage.Suppliers.Data.Services;
using DesktopLamour.Features.HomePage.Suppliers.Domain.UseCases;
using DesktopLamour.Features.HomePage.Suppliers.ViewModels;
using DesktopLamour.Features.HomePage.Suppliers.Views;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopLamour.Features.HomePage;

public static class HomeServiceCollectionExtensions
{
    public static IServiceCollection AddHomeModule(this IServiceCollection services, string serverUrl)
    {
        // ── Home hub ─────────────────────────────────────────────────────────
        services.AddTransient<HomeView>();
        services.AddTransient<HomeViewModel>();

        // ── ProductList: Views + ViewModels ─────────────────────────────────
        services.AddTransient<ProductListView>();
        services.AddTransient<ProductListViewModel>();
        services.AddTransient<ProductFormWindow>();
        services.AddTransient<ProductFormViewModel>();

        // ── ProductList: UseCases ────────────────────────────────────────────
        services.AddTransient<IGetProductsUseCase, GetProductsUseCase>();
        services.AddTransient<ICreateProductUseCase, CreateProductUseCase>();
        services.AddTransient<IUpdateProductUseCase, UpdateProductUseCase>();
        services.AddTransient<IDeleteProductUseCase, DeleteProductUseCase>();
        services.AddTransient<IDuplicateProductUseCase, DuplicateProductUseCase>();

        // ── ProductList: Repository ──────────────────────────────────────────
        services.AddTransient<IProductRepository, ProductRepository>();

        // ── ProductList: Service + typed HttpClient ──────────────────────────
        services.AddHttpClient<IProductService, ProductService>(client =>
        {
            client.BaseAddress = new Uri(serverUrl);
            client.Timeout     = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        // ── Suppliers: Views + ViewModels ────────────────────────────────────
        services.AddTransient<SupplierListView>();
        services.AddTransient<SupplierListViewModel>();
        services.AddTransient<SupplierFormWindow>();
        services.AddTransient<SupplierFormViewModel>();

        // ── Suppliers: UseCases ──────────────────────────────────────────────
        services.AddTransient<IGetSuppliersUseCase, GetSuppliersUseCase>();
        services.AddTransient<ICreateSupplierUseCase, CreateSupplierUseCase>();
        services.AddTransient<IUpdateSupplierUseCase, UpdateSupplierUseCase>();
        services.AddTransient<IDeleteSupplierUseCase, DeleteSupplierUseCase>();
        services.AddTransient<IDuplicateSupplierUseCase, DuplicateSupplierUseCase>();

        // ── Suppliers: Repository ────────────────────────────────────────────
        services.AddTransient<ISupplierRepository, SupplierRepository>();

        // ── Suppliers: Service + typed HttpClient ────────────────────────────
        services.AddHttpClient<ISupplierService, SupplierService>(client =>
        {
            // TODO: Move base address to configuration (appsettings.json / env)
            client.BaseAddress = new Uri(serverUrl);
            client.Timeout     = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        // ── ProductList: Window factory ──────────────────────────────────────
        services.AddTransient<Func<ProductFormWindow>>(sp => () => sp.GetRequiredService<ProductFormWindow>());

        // ── Suppliers: Window factory ────────────────────────────────────────
        services.AddTransient<Func<SupplierFormWindow>>(sp => () => sp.GetRequiredService<SupplierFormWindow>());

        // ── Customers: Views + ViewModels ────────────────────────────────────
        services.AddTransient<CustomerListView>();
        services.AddTransient<CustomerListViewModel>();
        services.AddTransient<CustomerFormWindow>();
        services.AddTransient<CustomerFormViewModel>();

        // ── Customers: UseCases ──────────────────────────────────────────────
        services.AddTransient<IGetCustomersUseCase, GetCustomersUseCase>();
        services.AddTransient<ICreateCustomerUseCase, CreateCustomerUseCase>();
        services.AddTransient<IUpdateCustomerUseCase, UpdateCustomerUseCase>();
        services.AddTransient<IDeleteCustomerUseCase, DeleteCustomerUseCase>();
        services.AddTransient<IDuplicateCustomerUseCase, DuplicateCustomerUseCase>();

        // ── Customers: Repository ────────────────────────────────────────────
        services.AddTransient<ICustomerRepository, CustomerRepository>();

        // ── Customers: Service + typed HttpClient ────────────────────────────
        services.AddHttpClient<ICustomerService, CustomerService>(client =>
        {
            client.BaseAddress = new Uri(serverUrl);
            client.Timeout     = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        // ── Customers: Window factory ────────────────────────────────────────
        services.AddTransient<Func<CustomerFormWindow>>(sp => () => sp.GetRequiredService<CustomerFormWindow>());

        // ── Employees: Views + ViewModels ────────────────────────────────────────
        services.AddTransient<EmployeeListView>();
        services.AddTransient<EmployeeListViewModel>();
        services.AddTransient<EmployeeFormWindow>();
        services.AddTransient<EmployeeFormViewModel>();

        // ── Employees: UseCases ──────────────────────────────────────────────────
        services.AddTransient<IGetEmployeesUseCase, GetEmployeesUseCase>();
        services.AddTransient<ICreateEmployeeUseCase, CreateEmployeeUseCase>();
        services.AddTransient<IUpdateEmployeeUseCase, UpdateEmployeeUseCase>();
        services.AddTransient<IDeleteEmployeeUseCase, DeleteEmployeeUseCase>();
        services.AddTransient<IDuplicateEmployeeUseCase, DuplicateEmployeeUseCase>();

        // ── Employees: Repository ────────────────────────────────────────────────
        services.AddTransient<IEmployeeRepository, EmployeeRepository>();

        // ── Employees: Service + typed HttpClient ────────────────────────────────
        services.AddHttpClient<IEmployeeService, EmployeeService>(client =>
        {
            client.BaseAddress = new Uri(serverUrl);
            client.Timeout     = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        // ── Employees: Window factory ────────────────────────────────────────────
        services.AddTransient<Func<EmployeeFormWindow>>(sp => () => sp.GetRequiredService<EmployeeFormWindow>());

        // ── Accounting: Views + ViewModels ───────────────────────────────────────
        services.AddTransient<AccountingView>();
        services.AddTransient<AccountingViewModel>();
        services.AddTransient<ReceiptWindow>();
        services.AddTransient<ReceiptViewModel>();
        services.AddTransient<PaymentWindow>();
        services.AddTransient<PaymentViewModel>();

        // ── Accounting: UseCases ─────────────────────────────────────────────────
        services.AddTransient<IGetCashLedgerUseCase, GetCashLedgerUseCase>();
        services.AddTransient<IGetReceiptsUseCase, GetReceiptsUseCase>();
        services.AddTransient<IGetReceiptByIdUseCase, GetReceiptByIdUseCase>();
        services.AddTransient<ICreateReceiptUseCase, CreateReceiptUseCase>();
        services.AddTransient<IUpdateReceiptUseCase, UpdateReceiptUseCase>();
        services.AddTransient<IDeleteReceiptUseCase, DeleteReceiptUseCase>();
        services.AddTransient<IGetPaymentsUseCase, GetPaymentsUseCase>();
        services.AddTransient<IGetPaymentByIdUseCase, GetPaymentByIdUseCase>();
        services.AddTransient<ICreatePaymentUseCase, CreatePaymentUseCase>();
        services.AddTransient<IUpdatePaymentUseCase, UpdatePaymentUseCase>();
        services.AddTransient<IDeletePaymentUseCase, DeletePaymentUseCase>();
        services.AddTransient<IDuplicatePaymentUseCase, DuplicatePaymentUseCase>();

        // ── Accounting: Service + typed HttpClient ───────────────────────────────
        services.AddHttpClient<ICashLedgerService, CashLedgerService>(client =>
        {
            client.BaseAddress = new Uri(serverUrl);
            client.Timeout     = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        services.AddHttpClient<IReceiptService, ReceiptService>(client =>
        {
            client.BaseAddress = new Uri(serverUrl);
            client.Timeout     = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        services.AddHttpClient<IPaymentService, PaymentService>(client =>
        {
            client.BaseAddress = new Uri(serverUrl);
            client.Timeout     = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        // ── Accounting: Window factory ───────────────────────────────────────────
        services.AddTransient<Func<ReceiptWindow>>(sp => () => sp.GetRequiredService<ReceiptWindow>());
        services.AddTransient<Func<PaymentWindow>>(sp => () => sp.GetRequiredService<PaymentWindow>());

        // ── Warehouse: Views + ViewModels ────────────────────────────────────────
        services.AddTransient<WarehouseView>();
        services.AddTransient<WarehouseViewModel>();
        services.AddTransient<TongHopTonKhoView>();
        services.AddTransient<TongHopTonKhoViewModel>();

        // ── Warehouse: UseCases ──────────────────────────────────────────────────
        services.AddTransient<IGetInventorySummaryUseCase, GetInventorySummaryUseCase>();

        // ── Warehouse: Repository ────────────────────────────────────────────────
        services.AddTransient<IWarehouseRepository, WarehouseRepository>();

        // ── Warehouse: Service + typed HttpClient ────────────────────────────────
        services.AddHttpClient<IWarehouseService, WarehouseService>(client =>
        {
            client.BaseAddress = new Uri(serverUrl);
            client.Timeout     = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        // ── WarehouseReceipts: Views + ViewModels ────────────────────────────────
        services.AddTransient<WarehouseReceiptListView>();
        services.AddTransient<WarehouseReceiptListViewModel>();
        services.AddTransient<WarehouseReceiptFormWindow>();
        services.AddTransient<WarehouseReceiptFormViewModel>();

        // ── WarehouseReceipts: UseCases ──────────────────────────────────────────
        services.AddTransient<IGetWarehouseReceiptsUseCase, GetWarehouseReceiptsUseCase>();
        services.AddTransient<ICreateWarehouseReceiptUseCase, CreateWarehouseReceiptUseCase>();
        services.AddTransient<IConfirmWarehouseReceiptUseCase, ConfirmWarehouseReceiptUseCase>();

        // ── WarehouseReceipts: Service + typed HttpClient ────────────────────────
        services.AddHttpClient<IWarehouseReceiptService, WarehouseReceiptService>(client =>
        {
            client.BaseAddress = new Uri(serverUrl);
            client.Timeout     = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        // ── WarehouseReceipts: Window factory ────────────────────────────────────
        services.AddTransient<Func<WarehouseReceiptFormWindow>>(sp => () => sp.GetRequiredService<WarehouseReceiptFormWindow>());

        // ── Sales: Views + ViewModels ────────────────────────────────────────────
        services.AddTransient<SalesOrderListView>();
        services.AddTransient<SalesOrderListViewModel>();
        services.AddTransient<SalesOrderWindow>();
        services.AddTransient<SalesOrderViewModel>();

        // ── Sales: UseCases ──────────────────────────────────────────────────────
        services.AddTransient<IGetSalesOrdersUseCase, GetSalesOrdersUseCase>();
        services.AddTransient<ICreateSalesOrderUseCase, CreateSalesOrderUseCase>();
        services.AddTransient<IUpdateSalesOrderUseCase, UpdateSalesOrderUseCase>();
        services.AddTransient<IDeleteSalesOrderUseCase, DeleteSalesOrderUseCase>();

        // ── Sales: Repository ────────────────────────────────────────────────────
        services.AddTransient<ISalesOrderRepository, SalesOrderRepository>();

        // ── Sales: Service + typed HttpClient ────────────────────────────────────
        services.AddHttpClient<ISalesOrderService, SalesOrderService>(client =>
        {
            client.BaseAddress = new Uri(serverUrl);
            client.Timeout     = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        // ── Sales: Window factory ────────────────────────────────────────────────
        services.AddTransient<Func<SalesOrderWindow>>(sp => () => sp.GetRequiredService<SalesOrderWindow>());

        return services;
    }
}
