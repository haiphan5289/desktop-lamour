// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;
namespace DesktopLamour.Features.HomePage.ProductList.Data.Services.Dtos;

public class CreateProductRequestDto
{
    [JsonPropertyName("code")]           public string  Code          { get; set; } = string.Empty;
    [JsonPropertyName("name")]           public string  Name          { get; set; } = string.Empty;
    [JsonPropertyName("category")]       public string  Category      { get; set; } = string.Empty;
    [JsonPropertyName("unit")]           public string  Unit          { get; set; } = string.Empty;
    [JsonPropertyName("cost_price")]     public decimal CostPrice     { get; set; }
    [JsonPropertyName("selling_price")]  public decimal SellingPrice  { get; set; }
    [JsonPropertyName("stock_quantity")] public int     StockQuantity { get; set; }
    [JsonPropertyName("is_active")]      public bool    IsActive      { get; set; } = true;
}
