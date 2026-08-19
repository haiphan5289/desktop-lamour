// Copyright © 2026 DesktopLamour. All rights reserved.
using System.Text.Json.Serialization;

namespace DesktopLamour.Features.HomePage.Employees.Data.Services.Dtos;

public class ImportEmployeeResultDto
{
    [JsonPropertyName("total")]
    public int Total { get; init; }

    [JsonPropertyName("imported")]
    public int Imported { get; init; }

    [JsonPropertyName("skipped")]
    public int Skipped { get; init; }

    [JsonPropertyName("errors")]
    public IReadOnlyList<ImportRowErrorDto> Errors { get; init; } = [];
}

public class ImportRowErrorDto
{
    [JsonPropertyName("row")]
    public int Row { get; init; }

    [JsonPropertyName("reason")]
    public string Reason { get; init; } = string.Empty;
}
