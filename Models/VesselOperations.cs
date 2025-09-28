namespace HavilaKystruten.Maritime.Models;

public record VesselStatusUpdateRequest
{
    public VesselStatus Status { get; init; }
    public string? Notes { get; init; }
}

public record VesselEmergencyReport
{
    public string Type { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string ReportedBy { get; init; } = string.Empty;
}

public record VesselEmergencyResponse
{
    public Guid EmergencyId { get; init; } = Guid.NewGuid();
    public string VesselName { get; init; } = string.Empty;
    public bool Acknowledged { get; init; }
    public string Response { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
