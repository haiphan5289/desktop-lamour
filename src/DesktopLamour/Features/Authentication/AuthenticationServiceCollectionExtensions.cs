// AuthenticationServiceCollectionExtensions.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using DesktopLamour.Features.Authentication.Data.Repositories;
using DesktopLamour.Features.Authentication.Data.Services;
using DesktopLamour.Features.Authentication.Domain.UseCases;
using DesktopLamour.Features.Authentication.ViewModels;
using DesktopLamour.Features.Authentication.Views;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopLamour.Features.Authentication;

public static class AuthenticationServiceCollectionExtensions
{
    public static IServiceCollection AddAuthenticationModule(this IServiceCollection services)
    {
        // ── Views (Transient — new instance per navigation) ──────────────────
        services.AddTransient<RegisterView>();
        services.AddTransient<LoginView>();

        // ── ViewModels ───────────────────────────────────────────────────────
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<LoginViewModel>();

        // ── Domain: UseCases ─────────────────────────────────────────────────
        services.AddTransient<ICheckPhoneExistUseCase, CheckPhoneExistUseCase>();
        services.AddTransient<ISignUpWithPhoneUseCase, SignUpWithPhoneUseCase>();
        services.AddTransient<ILoginWithPhoneUseCase, LoginWithPhoneUseCase>();

        // ── Data: Repository ─────────────────────────────────────────────────
        services.AddTransient<IAuthenticationRepository, AuthenticationRepository>();

        // ── Data: Service + typed HttpClient ─────────────────────────────────
        services.AddHttpClient<IAuthenticationService, AuthenticationService>(client =>
        {
            // TODO: Move base address to configuration (appsettings.json / env)
            client.BaseAddress = new Uri("http://192.168.64.1:5282");
            client.Timeout     = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        return services;
    }
}
