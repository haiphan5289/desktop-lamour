// HomeServiceCollectionExtensions.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

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

        return services;
    }
}
