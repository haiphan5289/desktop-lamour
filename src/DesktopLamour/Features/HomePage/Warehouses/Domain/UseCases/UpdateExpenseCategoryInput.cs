// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases;

public record UpdateExpenseCategoryInput(int Id, string Code, string Name, int? DepartmentId, string? Description);
