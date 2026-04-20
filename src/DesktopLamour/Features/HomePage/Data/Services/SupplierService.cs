// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Data.Services.Dtos;
namespace DesktopLamour.Features.HomePage.Data.Services;

public sealed class SupplierService : ISupplierService
{
    private static int _nextId = 100;

    private static readonly List<SupplierResponseDto> _mockData = new()
    {
        new() { Id=1,  Code="AO",           Name="ÁO BLOUSE",                Phone="0938365533" },
        new() { Id=2,  Code="BIA CUNG",     Name="BÌA CHỨNG CHỈ LAMOUR",     Phone="0974953273" },
        new() { Id=3,  Code="COSMO",        Name="COSMO C&T CO.LTD",          Address="103-1103~1104, Ssangyong 3rd Bucheon Techno Park" },
        new() { Id=4,  Code="HNB",          Name="CÔNG TY CỔ PHẦN MỸ PHẨM HNB", Address="Lô 1-RC, đường Tân Lập-Long Hậu, KCN Long Hậu" },
        new() { Id=5,  Code="HOATUOI_CITI", Name="HOA TƯƠI CITI",             Phone="0976574484" },
        new() { Id=6,  Code="HOATUOI_HCM",  Name="HOA TƯƠI HCM",              Phone="0947763426" },
        new() { Id=7,  Code="ISOV",         Name="SOREX CO.LTD",              Address="579, Gwangnaru-ro, Gwangjun-gu, Seoul, Korea" },
        new() { Id=8,  Code="MAYJUNE",      Name="MAYJUNE LIFE&HEALTH",       Address="10F, Y Tower, 129 Jayang-ro Rd., Gwangjin-gu Dist." },
        new() { Id=9,  Code="NUOCUONG",     Name="NƯỚC SUỐI",                 Phone="0786299330" },
        new() { Id=10, Code="SARESMIN",     Name="EK SANGSUNGCHE",            Address="C-3,114 Dohwa-Dong, Yeomjeon-Gu, Nam-Gu, Incheon" },
    };

    public Task<IEnumerable<SupplierResponseDto>> GetAllAsync(CancellationToken ct = default)
        => Task.FromResult<IEnumerable<SupplierResponseDto>>(_mockData.ToList());

    public Task DeleteAsync(int supplierId, CancellationToken ct = default)
    {
        _mockData.RemoveAll(s => s.Id == supplierId);
        return Task.CompletedTask;
    }

    public Task<SupplierResponseDto> DuplicateAsync(int supplierId, CancellationToken ct = default)
    {
        var original = _mockData.First(s => s.Id == supplierId);
        var copy = new SupplierResponseDto
        {
            Id             = _nextId++,
            Code           = original.Code + "_COPY",
            Name           = original.Name,
            Address        = original.Address,
            Group          = original.Group,
            TaxCode        = original.TaxCode,
            Phone          = original.Phone,
            IsStopTracking = original.IsStopTracking
        };
        _mockData.Add(copy);
        return Task.FromResult(copy);
    }

    public Task<SupplierResponseDto> CreateAsync(CreateSupplierRequestDto request, CancellationToken ct = default)
    {
        var dto = new SupplierResponseDto
        {
            Id             = _nextId++,
            Code           = request.Code,
            Name           = request.Name,
            Phone          = request.Phone,
            Address        = request.Address,
            Group          = request.Group,
            TaxCode        = request.TaxCode,
            IsStopTracking = request.IsStopTracking
        };
        _mockData.Add(dto);
        return Task.FromResult(dto);
    }

    public Task<SupplierResponseDto> UpdateAsync(int supplierId, UpdateSupplierRequestDto request, CancellationToken ct = default)
    {
        var existing = _mockData.FirstOrDefault(s => s.Id == supplierId)
            ?? throw new InvalidOperationException($"Supplier {supplierId} not found.");
        existing.Code           = request.Code;
        existing.Name           = request.Name;
        existing.Phone          = request.Phone;
        existing.Address        = request.Address;
        existing.Group          = request.Group;
        existing.TaxCode        = request.TaxCode;
        existing.IsStopTracking = request.IsStopTracking;
        return Task.FromResult(existing);
    }
}
