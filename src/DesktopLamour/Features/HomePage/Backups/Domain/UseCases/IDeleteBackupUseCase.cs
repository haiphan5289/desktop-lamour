// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Backups.Domain.UseCases;

public interface IDeleteBackupUseCase
{
    Task ExecuteAsync(string fileName, CancellationToken ct = default);
}
