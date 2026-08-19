// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Suppliers.Domain.Models;

public record ImportSupplierResult(int Total, int Imported, int Skipped, IReadOnlyList<ImportRowError> Errors);

public record ImportRowError(int Row, string Reason);
