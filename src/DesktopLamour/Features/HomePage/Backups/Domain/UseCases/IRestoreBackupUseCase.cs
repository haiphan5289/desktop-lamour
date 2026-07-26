// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Backups.Domain.UseCases;

public interface IRestoreBackupUseCase
{
    Task ExecuteAsync(string fileName, string password, CancellationToken ct = default);
}
