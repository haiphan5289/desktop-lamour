// CheckPhoneExistUseCase.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using DesktopLamour.Features.Authentication.Data.Repositories;

namespace DesktopLamour.Features.Authentication.Domain.UseCases;

public class CheckPhoneExistUseCase : ICheckPhoneExistUseCase
{
    private readonly IAuthenticationRepository _repository;

    public CheckPhoneExistUseCase(IAuthenticationRepository repository)
        => _repository = repository;

    public Task<bool> ExecuteAsync(string phone, CancellationToken cancellationToken = default)
        => _repository.CheckPhoneExistsAsync(phone, cancellationToken);
}
