namespace VetGest.Contracts.Pregnancies;

public sealed class CreateDiaryEntryRequest
{
    public DateTime EntryDate { get; set; }
    public decimal? WeightKg { get; set; }
    public string? Appetite { get; set; }
    public string? Behavior { get; set; }
    public decimal? TemperatureCelsius { get; set; }
    public string[]? Symptoms { get; set; }
    public string[]? PhotoReferences { get; set; }
    public string? Notes { get; set; }
}

public sealed class DiaryEntryDto
{
    public Guid Id { get; set; }
    public Guid PregnancyId { get; set; }
    public DateTime EntryDate { get; set; }
    public decimal? WeightKg { get; set; }
    public string? Appetite { get; set; }
    public string? Behavior { get; set; }
    public decimal? TemperatureCelsius { get; set; }
    public string[] Symptoms { get; set; } = Array.Empty<string>();
    public string[] PhotoReferences { get; set; } = Array.Empty<string>();
    public string? Notes { get; set; }
}

public sealed class DiaryTrendPointDto
{
    public DateTime EntryDate { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? TemperatureCelsius { get; set; }
}