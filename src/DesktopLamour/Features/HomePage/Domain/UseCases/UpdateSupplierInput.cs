// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Domain.UseCases;

public record UpdateSupplierInput(
    int Id, string Code, string Name, string Phone, string Address,
    string Group, string TaxCode, bool IsStopTracking);
