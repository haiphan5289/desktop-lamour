// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Customers.Domain.UseCases;

public record UpdateCustomerInput(
    int Id, string Name, string Phone, string Address,
    string Province, string CustomerGroup, string TaxCode, string SaleCare);
