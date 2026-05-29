// Copyright © 2026 DesktopLamour. All rights reserved.
namespace DesktopLamour.Features.HomePage.Customers.Domain.Models;

public record ImportCustomerResult(int Total, int Imported, int Skipped, IReadOnlyList<ImportRowError> Errors);

public record ImportRowError(int Row, string Reason);
