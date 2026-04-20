// ProductResponseDto.cs
// Copyright © 2026 DesktopLamour. All rights reserved.

using System.Text.Json.Serialization;

namespace DesktopLamour.Features.HomePage.Data.Services.Dtos;

public class ProductResponseDto
{
    [JsonPropertyName("id")]         public int Id { get; set; }
    [JsonPropertyName("name")]       public string Name { get; set; } = string.Empty;
    [JsonPropertyName("category")]   public string Category { get; set; } = string.Empty;
    [JsonPropertyName("sale_price")] public decimal SalePrice { get; set; }
    [JsonPropertyName("stock_qty")]  public int StockQuantity { get; set; }
}
