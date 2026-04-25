// HomeServiceCollectionExtensions.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using DesktopLamour.Features.HomePage.Home.ViewModels;
using DesktopLamour.Features.HomePage.Home.Views;
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
    public static IServiceCollection AddHomeModule(this IServiceCollection services)
    {
        // ── Home hub ─────────────────────────────────────────────────────────
        services.AddTransient<HomeView>();
        services.AddTransient<HomeViewModel>();

        // ── ProductList ──────────────────────────────────────────────────────
        services.AddTransient<ProductListView>();
        services.AddTransient<ProductListViewModel>();

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
            client.BaseAddress = new Uri("http://192.168.64.1:5282");
            client.Timeout     = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        // ── Suppliers: Window factory ────────────────────────────────────────
        services.AddTransient<Func<SupplierFormWindow>>(sp => () => sp.GetRequiredService<SupplierFormWindow>());

        return services;
    }
}
