// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Employees.Domain.UseCases;

public record CreateEmployeeInput(string Name, string Phone, string Role, string Unit, string JobTitle, string? BankAccountNumber, string? BankName, string Password, bool IsActive);
