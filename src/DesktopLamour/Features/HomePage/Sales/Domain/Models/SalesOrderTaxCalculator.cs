// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.ProductList.Domain.Models;

namespace DesktopLamour.Features.HomePage.Sales.Domain.Models;

public static class SalesOrderTaxCalculator
{
    public static decimal ToPercent(VatRateType? vatRate) => vatRate switch
    {
        VatRateType.Five  => 5m,
        VatRateType.Eight => 8m,
        VatRateType.Ten   => 10m,
        _                 => 0m, // Zero, KCT, KKKNT, KHAC, null — không tính thuế
    };
}
