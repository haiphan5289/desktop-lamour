// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.ProductList.Domain.Models;
using FluentAssertions;
using Xunit;

namespace DesktopLamour.Tests.Features.ProductList;

// Cột "SL tồn | Giá bán" của dropdown chọn sản phẩm (AppSearchableComboBox.ShowStockAndPrice) ở
// Chứng từ bán hàng / Hàng bán bị trả lại.
public class ProductDropdownColumnsTests
{
    [Theory]
    [InlineData(711, "711")]
    [InlineData(1575, "1.575")]
    [InlineData(0, "0")]
    [InlineData(-5, "-5")]
    public void DropdownStockText_UsesVietnameseThousandSeparator(int stock, string expected)
        => new Product { StockQuantity = stock }.DropdownStockText.Should().Be(expected);

    [Theory]
    [InlineData(1125000, "1.125.000")]
    [InlineData(400000, "400.000")]
    [InlineData(0, "0")]
    public void DropdownPriceText_FormatsSellingPriceLikeMisa(decimal price, string expected)
        => new Product { SellingPrice = price }.DropdownPriceText.Should().Be(expected);

    [Theory]
    [InlineData(0, true)]
    [InlineData(-3, true)]
    [InlineData(1, false)]
    [InlineData(575, false)]
    public void IsDropdownOutOfStock_TrueWhenStockAtOrBelowZero(int stock, bool expected)
        => new Product { StockQuantity = stock }.IsDropdownOutOfStock.Should().Be(expected);

    [Fact]
    public void DepositProduct_ShowsBlankColumns_AndIsNeverOutOfStock()
    {
        // "Đặt cọc" / "Trừ cọc" (mã 36) không có tồn kho/giá thật — không được hiện "0" đỏ gây hiểu nhầm.
        var deposit = new Product { IsDepositProduct = true, StockQuantity = 0, SellingPrice = 0 };

        deposit.DropdownStockText.Should().BeEmpty();
        deposit.DropdownPriceText.Should().BeEmpty();
        deposit.IsDropdownOutOfStock.Should().BeFalse();
    }

    [Fact]
    public void Service_StillShowsStock_BecauseBeChecksStockForServicesToo()
    {
        var service = new Product { Nature = ProductNature.DichVu, StockQuantity = 0 };

        service.DropdownStockText.Should().Be("0");
        service.IsDropdownOutOfStock.Should().BeTrue();
    }
}
