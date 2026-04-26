// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Employees.Domain.UseCases;

public record UpdateEmployeeInput(int Id, string Name, string Phone, string Role, string Unit, string? Password, bool IsActive);
