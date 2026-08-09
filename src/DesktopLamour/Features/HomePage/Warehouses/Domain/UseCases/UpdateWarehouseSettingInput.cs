// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases;

public record UpdateWarehouseSettingInput(int Id, string Code, string Name, bool IsActive);
