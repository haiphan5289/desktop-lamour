// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Customers.Domain.UseCases;

public record CreateCustomerInput(
    string Name, string Phone, string Address,
    string Province, string CustomerGroup, string TaxCode, string SaleCare);
