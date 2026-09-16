namespace VetGest.Contracts.Common;

public sealed class HealthStatusDto
{
    public string Status { get; set; } = "Desconhecido";
    public DateTime Timestamp { get; set; }
    public string Version { get; set; } = "0.0.0";
    public string Environment { get; set; } = "Desconhecido";
}
