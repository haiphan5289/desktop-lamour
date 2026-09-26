// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Globalization;
using DesktopLamour.Shared.Converters;
using DesktopLamour.Shared.Helpers;
using DesktopLamour.Shared.Models;
using FluentAssertions;
using Xunit;

namespace DesktopLamour.Tests.Shared;

public class MoneyFormatTests
{
    [Theory]
    [InlineData(1600000, "1.600.000")]
    [InlineData(0, "0")]
    [InlineData(-1458000, "(1.458.000)")]
    public void Format_UsesVietnameseSeparators_AndParenthesesForNegative(decimal value, string expected)
        => MoneyFormat.Format(value).Should().Be(expected);

    [Fact]
    public void Format_N2_UsesCommaDecimal()
        => MoneyFormat.Format(35.5m, "N2").Should().Be("35,50");

    [Theory]
    [InlineData("(1.458.000)", true)]
    [InlineData("-1.458.000", true)]
    [InlineData("1.458.000", false)]
    [InlineData("(KM) Kem dưỡng", false)]
    [InlineData("", false)]
    public void IsNegativeText_OnlyMatchesNumbers(string text, bool expected)
        => MoneyFormat.IsNegativeText(text).Should().Be(expected);

    [Fact]
    public void MoneyConverter_IgnoresBindingCulture()
    {
        var converter = new MoneyConverter();
        converter.Convert(1600000m, typeof(string), null!, CultureInfo.GetCultureInfo("en-US"))
            .Should().Be("1.600.000");
        converter.Convert(-10, typeof(string), null!, CultureInfo.GetCultureInfo("en-US"))
            .Should().Be("(10)");
    }

    [Theory]
    [InlineData("1.500.000", 1500000)]
    [InlineData("1,500,000", 1500000)]
    [InlineData("1500000", 1500000)]
    [InlineData("35,5", 35.5)]
    [InlineData("35.5", 35.5)]
    [InlineData("(1.458.000)", -1458000)]
    public void NumericColumnFilter_AcceptsVietnameseAndLegacyInput(string input, double expected)
    {
        var filter = new NumericColumnFilter { Operator = FilterOperator.Equal, ValueText = input };
        filter.Matches((decimal)expected).Should().BeTrue();
    }
}
