using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using HavilaKystruten.Maritime.Models;
using Xunit;

namespace HavilaKystruten.Maritime.Tests.Integration;

public class MaritimeInsightsTests : IClassFixture<MaritimeWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = CreateJsonOptions();

    public MaritimeInsightsTests(MaritimeWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    [Fact]
    public async Task InsightsEndpoint_ReturnsOverview()
    {
        var response = await _client.GetAsync("/api/insights");
        response.EnsureSuccessStatusCode();

        using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = json.RootElement;

        Assert.True(root.TryGetProperty("generatedAt", out _));
        Assert.True(root.TryGetProperty("fleet", out var fleetElement));
        Assert.True(fleetElement.TryGetProperty("totalVessels", out var totalVessels));
        Assert.True(totalVessels.GetInt32() > 0);
    }

    [Fact]
    public async Task VesselStatusUpdate_ReflectsInSubsequentQueries()
    {
        var payload = JsonContent.Create(new { status = "Maintenance", notes = "Scheduled systems check" });
        var updateResponse = await _client.PutAsync("/api/vessel/1/status", payload);
        updateResponse.EnsureSuccessStatusCode();

        var vessel = await _client.GetFromJsonAsync<VesselDto>("/api/vessel/1", _jsonOptions);
        Assert.NotNull(vessel);
        Assert.Equal(VesselStatus.Maintenance, vessel!.Status);

        var statusSummary = await _client.GetFromJsonAsync<FleetStatusSummaryDto>("/api/insights/fleet-status", _jsonOptions);
        Assert.NotNull(statusSummary);
        Assert.True(statusSummary!.Maintenance >= 1);
    }

    [Fact]
    public async Task EmergencyEndpoint_FlagsVesselAndReturnsAcknowledgement()
    {
        var emergencyPayload = JsonContent.Create(new { Type = "Medical", Description = "Passenger injury", ReportedBy = "Bridge" });
        var response = await _client.PostAsync("/api/vessel/2/emergency", emergencyPayload);
        response.EnsureSuccessStatusCode();

        var emergency = await response.Content.ReadFromJsonAsync<EmergencyResponseDto>(_jsonOptions);
        Assert.NotNull(emergency);
        Assert.True(emergency!.Acknowledged);

        var fleetStatus = await _client.GetFromJsonAsync<FleetStatusSummaryDto>("/api/insights/fleet-status", _jsonOptions);
        Assert.NotNull(fleetStatus);
        Assert.True(fleetStatus!.Emergency >= 1);
    }

    private record VesselDto(int Id, string Name, VesselStatus Status);

    private record FleetStatusSummaryDto(int TotalVessels, int InService, int InPort, int Maintenance, int Emergency, DateTime LastUpdated);

    private record EmergencyResponseDto(Guid EmergencyId, string VesselName, bool Acknowledged, string Response, DateTime Timestamp);
}
