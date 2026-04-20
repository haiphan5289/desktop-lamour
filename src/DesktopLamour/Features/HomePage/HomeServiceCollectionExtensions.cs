// HomeServiceCollectionExtensions.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using DesktopLamour.Features.HomePage.Data.Repositories;
using DesktopLamour.Features.HomePage.Data.Services;
using DesktopLamour.Features.HomePage.Domain.UseCases;
using DesktopLamour.Features.HomePage.ViewModels;
using DesktopLamour.Features.HomePage.Views;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopLamour.Features.HomePage;

public static class HomeServiceCollectionExtensions
{
    public static IServiceCollection AddHomeModule(this IServiceCollection services)
    {
        // ── Views (Transient — new instance per navigation) ──────────────────
        services.AddTransient<HomeView>();
        services.AddTransient<ProductListView>();
        services.AddTransient<SupplierListView>();

        // ── ViewModels ───────────────────────────────────────────────────────
        services.AddTransient<HomeViewModel>();
        services.AddTransient<ProductListViewModel>();
        services.AddTransient<SupplierListViewModel>();

        // ── Supplier Service (Singleton to keep mock data alive across navigations)
        services.AddSingleton<ISupplierService, SupplierService>();

        // ── Supplier Repository ──────────────────────────────────────────────
        services.AddTransient<ISupplierRepository, SupplierRepository>();

        // ── Supplier UseCases ────────────────────────────────────────────────
        services.AddTransient<IGetSuppliersUseCase, GetSuppliersUseCase>();
        services.AddTransient<IDeleteSupplierUseCase, DeleteSupplierUseCase>();
        services.AddTransient<IDuplicateSupplierUseCase, DuplicateSupplierUseCase>();

        // ── Supplier Form (Add / Edit popup) ─────────────────────────────────
        services.AddTransient<SupplierFormWindow>();
        services.AddTransient<SupplierFormViewModel>();
        services.AddTransient<ICreateSupplierUseCase, CreateSupplierUseCase>();
        services.AddTransient<IUpdateSupplierUseCase, UpdateSupplierUseCase>();
        services.AddTransient<Func<SupplierFormWindow>>(sp => () => sp.GetRequiredService<SupplierFormWindow>());

        return services;
    }
}
