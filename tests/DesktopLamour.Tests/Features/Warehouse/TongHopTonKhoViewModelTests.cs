// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Reflection;
using DesktopLamour.Core.Navigation;
using DesktopLamour.Features.HomePage.Categories.Domain.UseCases;
using DesktopLamour.Features.HomePage.Categories.Domain.Models;
using DesktopLamour.Features.HomePage.ProductList.Domain.UseCases;
using DesktopLamour.Features.HomePage.ProductList.Domain.Models;
using DesktopLamour.Features.HomePage.ProductUnits.Domain.UseCases;
using DesktopLamour.Features.HomePage.ProductUnits.Domain.Models;
using DesktopLamour.Features.HomePage.Warehouse.Domain.Models;
using DesktopLamour.Features.HomePage.Warehouse.Domain.UseCases;
using DesktopLamour.Features.HomePage.Warehouse.ViewModels;
using DesktopLamour.Features.HomePage.Warehouse.Views;
using DesktopLamour.Features.HomePage.Warehouses.Domain.UseCases;
using DesktopLamour.Features.HomePage.Warehouses.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DesktopLamour.Tests.Features.Warehouse;

// 2026-10-17: cột "Ngày HT" (LatestAccountingDate) đã bị gỡ khỏi màn "Tổng hợp tồn kho" theo yêu cầu
// khớp MISA — các test này khoá lại hành vi lọc/xuất Excel SAU khi gỡ cột để tránh một lần sửa khác
// vô tình bỏ sót FilterDate/DateValueFilter hoặc làm lệch số cột Excel (ExcelLastColumn) lần nữa.
public class TongHopTonKhoViewModelTests
{
    private readonly Mock<IGetInventorySummaryByWarehouseUseCase> _getSummary = new();

    private TongHopTonKhoViewModel CreateSut() => new(
        Mock.Of<INavigationService>(),
        _getSummary.Object,
        () => null!,
        Mock.Of<IGetWarehouseSettingsUseCase>(m => m.ExecuteAsync(default) == Task.FromResult<IEnumerable<WarehouseSetting>>(Array.Empty<WarehouseSetting>())),
        Mock.Of<IGetCategoriesUseCase>(m => m.ExecuteAsync(default) == Task.FromResult<IEnumerable<Category>>(Array.Empty<Category>())),
        Mock.Of<IGetProductUnitsUseCase>(m => m.ExecuteAsync(default) == Task.FromResult<IEnumerable<ProductUnit>>(Array.Empty<ProductUnit>())),
        Mock.Of<IGetProductsUseCase>(m => m.ExecuteAsync(default) == Task.FromResult<IEnumerable<Product>>(Array.Empty<Product>())),
        Mock.Of<ILogger<TongHopTonKhoViewModel>>());

    private static InventorySummaryItem Item(
        int productId, string code, string name, string unit,
        int openingQty, int importQty, int exportQty, int closingQty) => new()
    {
        ProductId    = productId,
        Code         = code,
        Name         = name,
        Unit         = unit,
        OpeningQty   = openingQty,
        ImportQty    = importQty,
        ExportQty    = exportQty,
        ClosingQty   = closingQty,
        // Khớp MISA: LatestAccountingDate vẫn còn trên model (dùng ở báo cáo khác) nhưng màn này
        // không hiển thị/lọc theo nó nữa — set có giá trị để chắc chắn test không vô tình phụ thuộc.
        LatestAccountingDate = new DateTime(2026, 10, 15, 7, 0, 0),
    };

    private void SetupGroups(params InventoryWarehouseGroup[] groups) =>
        _getSummary
            .Setup(r => r.ExecuteAsync(
                It.IsAny<DateOnly>(), It.IsAny<DateOnly>(),
                It.IsAny<IReadOnlyList<int>?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<IReadOnlyList<int>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(groups);

    [Fact]
    public async Task LoadAsync_PopulatesGroupHeaderAndItems_WithoutAnyDateColumnDependency()
    {
        var group = new InventoryWarehouseGroup
        {
            WarehouseId   = 4,
            WarehouseCode = "HH",
            WarehouseName = "Hàng Hóa",
            Items         = new List<InventorySummaryItem> { Item(1, "SP001", "Kem", "Hộp", 13, 10, 3, 20) },
        };
        SetupGroups(group);
        var sut = CreateSut();

        await sut.LoadCommand.ExecuteAsync(null);

        // Dòng tổng nhóm kho + 1 dòng mặt hàng.
        Assert.Equal(2, sut.Items.Count);
        Assert.True(sut.Items[0].IsGroupHeader);
        Assert.Equal("Tên kho : Hàng Hóa (1)", sut.Items[0].Name);
        Assert.False(sut.Items[1].IsGroupHeader);
        Assert.Equal(1, sut.RowCount);
        Assert.Equal(13, sut.TotalOpeningQty);
        Assert.Equal(3, sut.TotalExportQty);
    }

    [Fact]
    public async Task ApplyFilters_FilterCode_NarrowsVisibleItems_AndChainStillWorksAfterDateColumnRemoval()
    {
        var group = new InventoryWarehouseGroup
        {
            WarehouseId = 4, WarehouseCode = "HH", WarehouseName = "Hàng Hóa",
            Items = new List<InventorySummaryItem>
            {
                Item(1, "SP001", "Kem", "Hộp", 13, 10, 3, 20),
                Item(2, "SP002", "Sữa", "Chai", 5, 0, 1, 4),
            },
        };
        SetupGroups(group);
        var sut = CreateSut();
        await sut.LoadCommand.ExecuteAsync(null);

        sut.FilterCode = "SP002";

        var dataRows = sut.Items.Where(i => !i.IsGroupHeader).ToList();
        Assert.Single(dataRows);
        Assert.Equal("SP002", dataRows[0].Code);
        // Dòng tổng nhóm kho phải cập nhật lại số lượng theo đúng 1 dòng còn hiển thị.
        Assert.Equal("Tên kho : Hàng Hóa (1)", sut.Items[0].Name);
    }

    [Fact]
    public async Task ApplyFilters_LastValueListFilterInChain_StillAppliesCorrectly()
    {
        // ClosingValueValueFilter là điều kiện CUỐI trong chuỗi MatchesFilters (sau khi gỡ FilterDate/
        // DateValueFilter) — khoá lại để chắc chuỗi && không bị đứt/thiếu điều kiện sau khi sửa.
        var group = new InventoryWarehouseGroup
        {
            WarehouseId = 4, WarehouseCode = "HH", WarehouseName = "Hàng Hóa",
            Items = new List<InventorySummaryItem>
            {
                Item(1, "SP001", "Kem", "Hộp", 13, 10, 3, 20),
                Item(2, "SP002", "Sữa", "Chai", 5, 0, 1, 4),
            },
        };
        SetupGroups(group);
        var sut = CreateSut();
        await sut.LoadCommand.ExecuteAsync(null);

        // Tắt hết, chỉ giữ 1 giá trị ClosingValue (mặc định ClosingValue = 0 cho cả 2 dòng ở test này)
        // — kiểm tra việc loại bỏ 1 Option thực sự ẩn đúng dòng tương ứng.
        var toUncheck = sut.ClosingValueValueFilter.Options.First(o => o.Value == "4");
        toUncheck.IsChecked = false;
        sut.ClosingValueValueFilter.ApplyCommand.Execute(null);

        var dataRows = sut.Items.Where(i => !i.IsGroupHeader).ToList();
        Assert.Single(dataRows);
        Assert.Equal("SP001", dataRows[0].Code);
    }

    [Fact]
    public async Task BuildWorkbook_HasElevenColumns_AndNoNgayHtHeader()
    {
        var group = new InventoryWarehouseGroup
        {
            WarehouseId = 4, WarehouseCode = "HH", WarehouseName = "Hàng Hóa",
            Items = new List<InventorySummaryItem> { Item(1, "SP001", "Kem", "Hộp", 13, 10, 3, 20) },
        };
        SetupGroups(group);
        var sut = CreateSut();
        await sut.LoadCommand.ExecuteAsync(null);

        var method = typeof(TongHopTonKhoViewModel).GetMethod("BuildWorkbook", BindingFlags.NonPublic | BindingFlags.Instance)!;
        using var workbook  = (ClosedXML.Excel.XLWorkbook)method.Invoke(sut, null)!;
        var worksheet = workbook.Worksheets.First();

        const int headerRow = 4;
        var headerTexts = Enumerable.Range(1, 12)
            .Select(col => worksheet.Cell(headerRow, col).GetString())
            .ToList();

        Assert.DoesNotContain("Ngày HT", headerTexts);
        // Cột 12 không được dùng (ExcelLastColumn = 11) — ô tương ứng phải trống.
        Assert.Equal(string.Empty, worksheet.Cell(headerRow, 12).GetString());
        Assert.Equal("ĐVT", worksheet.Cell(headerRow, 3).GetString());
        Assert.Equal("Cuối kỳ", worksheet.Cell(headerRow, 10).GetString());
    }
}
