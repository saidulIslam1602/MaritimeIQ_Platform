using Microsoft.AspNetCore.Mvc;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Text.Json;

namespace HavilaKystruten.Maritime.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PowerBIController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PowerBIController> _logger;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _tenantId;
        private readonly string _workspaceId;

        public PowerBIController(
            IConfiguration configuration,
            ILogger<PowerBIController> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _clientId = _configuration["PowerBI:ClientId"] ?? "";
            _clientSecret = _configuration["PowerBI:ClientSecret"] ?? "";
            _tenantId = _configuration["PowerBI:TenantId"] ?? "";
            _workspaceId = _configuration["PowerBI:WorkspaceId"] ?? "";
        }

        [HttpGet("dashboards")]
        public async Task<IActionResult> GetMaritimeDashboards()
        {
            try
            {
                var powerBIClient = await GetPowerBIClient();
                var reports = await powerBIClient.Reports.GetReportsInGroupAsync(new Guid(_workspaceId));

                var maritimeDashboards = new List<MaritimeDashboard>
                {
                    new MaritimeDashboard
                    {
                        Id = "fleet-operations",
                        Name = "Havila Fleet Operations Dashboard",
                        Description = "Real-time fleet monitoring, vessel positions, and operational metrics",
                        Category = "Operations",
                        Reports = new List<PowerBIReport>
                        {
                            new PowerBIReport
                            {
                                Id = "fleet-overview",
                                Name = "Fleet Overview",
                                Description = "Current vessel positions, status, and route progress",
                                Type = "Real-time",
                                RefreshFrequency = "Every 5 minutes",
                                DataSources = new[] { "AIS Data", "Vessel Telemetry", "Route Planning" }
                            },
                            new PowerBIReport
                            {
                                Id = "vessel-performance",
                                Name = "Vessel Performance Metrics",
                                Description = "Fuel consumption, speed optimization, and engine performance",
                                Type = "Analytics",
                                RefreshFrequency = "Every 15 minutes",
                                DataSources = new[] { "Engine Telemetry", "Fuel Systems", "Performance Logs" }
                            }
                        },
                        Tiles = GetFleetOperationsTiles(),
                        LastRefresh = DateTime.UtcNow.AddMinutes(-5),
                        AccessLevel = "Operations Team"
                    },
                    new MaritimeDashboard
                    {
                        Id = "environmental-compliance",
                        Name = "Environmental Compliance & Sustainability",
                        Description = "CO2 emissions, environmental impact, and sustainability metrics",
                        Category = "Environmental",
                        Reports = new List<PowerBIReport>
                        {
                            new PowerBIReport
                            {
                                Id = "emissions-tracking",
                                Name = "Emissions Tracking",
                                Description = "Real-time CO2, NOx, and particulate matter monitoring",
                                Type = "Real-time",
                                RefreshFrequency = "Every 5 minutes",
                                DataSources = new[] { "Environmental Sensors", "Emission Monitoring Systems" }
                            },
                            new PowerBIReport
                            {
                                Id = "sustainability-kpis",
                                Name = "Sustainability KPIs",
                                Description = "Green energy usage, waste reduction, and environmental goals",
                                Type = "Analytics",
                                RefreshFrequency = "Daily",
                                DataSources = new[] { "Hybrid Systems", "Waste Management", "Energy Consumption" }
                            }
                        },
                        Tiles = GetEnvironmentalTiles(),
                        LastRefresh = DateTime.UtcNow.AddMinutes(-10),
                        AccessLevel = "Environmental Team"
                    },
                    new MaritimeDashboard
                    {
                        Id = "passenger-experience",
                        Name = "Passenger Experience & Northern Lights",
                        Description = "Passenger satisfaction, Northern Lights forecasts, and service quality",
                        Category = "Passenger Services",
                        Reports = new List<PowerBIReport>
                        {
                            new PowerBIReport
                            {
                                Id = "northern-lights-forecast",
                                Name = "Northern Lights Forecast",
                                Description = "Aurora activity predictions and optimal viewing conditions",
                                Type = "Forecast",
                                RefreshFrequency = "Every 3 hours",
                                DataSources = new[] { "Aurora Forecast API", "Weather Data", "Geographic Position" }
                            },
                            new PowerBIReport
                            {
                                Id = "passenger-satisfaction",
                                Name = "Passenger Satisfaction Analytics",
                                Description = "Feedback analysis, service ratings, and experience metrics",
                                Type = "Analytics",
                                RefreshFrequency = "Every 2 hours",
                                DataSources = new[] { "Passenger Feedback", "Service Logs", "Booking Data" }
                            }
                        },
                        Tiles = GetPassengerExperienceTiles(),
                        LastRefresh = DateTime.UtcNow.AddMinutes(-30),
                        AccessLevel = "Guest Services"
                    },
                    new MaritimeDashboard
                    {
                        Id = "route-analytics",
                        Name = "Route Optimization & Weather",
                        Description = "Route efficiency, weather impact analysis, and port operations",
                        Category = "Navigation",
                        Reports = new List<PowerBIReport>
                        {
                            new PowerBIReport
                            {
                                Id = "route-efficiency",
                                Name = "Route Efficiency Analysis",
                                Description = "Time optimization, fuel savings, and route performance",
                                Type = "Analytics",
                                RefreshFrequency = "Every hour",
                                DataSources = new[] { "Navigation Data", "Route Planning", "Historical Routes" }
                            },
                            new PowerBIReport
                            {
                                Id = "weather-impact",
                                Name = "Weather Impact Assessment",
                                Description = "Weather effects on routes, delays, and operational decisions",
                                Type = "Real-time",
                                RefreshFrequency = "Every 15 minutes",
                                DataSources = new[] { "Weather Services", "Route Adjustments", "Operational Logs" }
                            }
                        },
                        Tiles = GetRouteAnalyticsTiles(),
                        LastRefresh = DateTime.UtcNow.AddMinutes(-15),
                        AccessLevel = "Navigation Team"
                    },
                    new MaritimeDashboard
                    {
                        Id = "safety-security",
                        Name = "Safety & Security Monitoring",
                        Description = "Safety incidents, security alerts, and emergency response",
                        Category = "Safety",
                        Reports = new List<PowerBIReport>
                        {
                            new PowerBIReport
                            {
                                Id = "safety-incidents",
                                Name = "Safety Incident Tracking",
                                Description = "Incident reports, safety KPIs, and preventive measures",
                                Type = "Analytics",
                                RefreshFrequency = "Real-time",
                                DataSources = new[] { "Incident Reports", "Safety Systems", "Crew Reports" }
                            },
                            new PowerBIReport
                            {
                                Id = "emergency-readiness",
                                Name = "Emergency Readiness Dashboard",
                                Description = "Emergency equipment status, drill compliance, and response capabilities",
                                Type = "Monitoring",
                                RefreshFrequency = "Every 30 minutes",
                                DataSources = new[] { "Safety Equipment", "Emergency Drills", "Crew Training" }
                            }
                        },
                        Tiles = GetSafetySecurityTiles(),
                        LastRefresh = DateTime.UtcNow.AddMinutes(-1),
                        AccessLevel = "Safety Team"
                    },
                    new MaritimeDashboard
                    {
                        Id = "financial-performance",
                        Name = "Financial Performance & Revenue",
                        Description = "Revenue analysis, cost optimization, and financial KPIs",
                        Category = "Financial",
                        Reports = new List<PowerBIReport>
                        {
                            new PowerBIReport
                            {
                                Id = "revenue-analysis",
                                Name = "Revenue Analysis",
                                Description = "Booking trends, revenue per voyage, and seasonal analysis",
                                Type = "Analytics",
                                RefreshFrequency = "Daily",
                                DataSources = new[] { "Booking System", "Payment Processing", "Seasonal Data" }
                            },
                            new PowerBIReport
                            {
                                Id = "operational-costs",
                                Name = "Operational Cost Analysis",
                                Description = "Fuel costs, maintenance expenses, and cost optimization opportunities",
                                Type = "Analytics",
                                RefreshFrequency = "Daily",
                                DataSources = new[] { "Fuel Data", "Maintenance Logs", "Operational Expenses" }
                            }
                        },
                        Tiles = GetFinancialPerformanceTiles(),
                        LastRefresh = DateTime.UtcNow.AddHours(-2),
                        AccessLevel = "Management"
                    }
                };

                return Ok(maritimeDashboards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving maritime dashboards");
                return StatusCode(500, "Error retrieving dashboards");
            }
        }

        [HttpGet("dashboard/{dashboardId}/embed-token")]
        public async Task<IActionResult> GetEmbedToken(string dashboardId, [FromQuery] string userId = "")
        {
            try
            {
                var powerBIClient = await GetPowerBIClient();
                
                // Get reports for the dashboard
                var reports = await powerBIClient.Reports.GetReportsInGroupAsync(new Guid(_workspaceId));
                var targetReport = reports.Value.FirstOrDefault(); // In real implementation, filter by dashboardId

                if (targetReport == null)
                {
                    return NotFound($"Dashboard {dashboardId} not found");
                }

                // Generate embed token
                var generateTokenRequestParameters = new GenerateTokenRequest(
                    accessLevel: "View",
                    identities: new List<EffectiveIdentity>()
                );

                var tokenResponse = await powerBIClient.Reports.GenerateTokenInGroupAsync(
                    new Guid(_workspaceId),
                    targetReport.Id,
                    generateTokenRequestParameters);

                var embedConfig = new PowerBIEmbedConfig
                {
                    Type = "report",
                    Id = targetReport.Id.ToString(),
                    EmbedUrl = targetReport.EmbedUrl,
                    AccessToken = tokenResponse.Token,
                    TokenId = tokenResponse.TokenId.ToString(),
                    Expiration = tokenResponse.Expiration,
                    Settings = new EmbedSettings
                    {
                        FilterPaneEnabled = true,
                        NavContentPaneEnabled = true,
                        Background = "transparent"
                    }
                };

                return Ok(embedConfig);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating embed token for dashboard {DashboardId}", dashboardId);
                return StatusCode(500, "Error generating embed token");
            }
        }

        [HttpPost("dashboard/{dashboardId}/refresh")]
        public async Task<IActionResult> RefreshDashboard(string dashboardId)
        {
            try
            {
                var powerBIClient = await GetPowerBIClient();
                
                // Get datasets for the workspace
                var datasets = await powerBIClient.Datasets.GetDatasetsInGroupAsync(new Guid(_workspaceId));
                
                foreach (var dataset in datasets.Value)
                {
                    if (IsDashboardDataset(dataset.Name, dashboardId))
                    {
                        await powerBIClient.Datasets.RefreshDatasetInGroupAsync(new Guid(_workspaceId), dataset.Id);
                        _logger.LogInformation($"Refreshed dataset {dataset.Name} for dashboard {dashboardId}");
                    }
                }

                return Ok(new { message = $"Dashboard {dashboardId} refresh initiated", timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing dashboard {DashboardId}", dashboardId);
                return StatusCode(500, "Error refreshing dashboard");
            }
        }

        [HttpGet("kpis/summary")]
        public IActionResult GetMaritimeKPISummary()
        {
            try
            {
                // In real implementation, this would query your data sources
                var kpiSummary = new MaritimeKPISummary
                {
                    FleetOperations = new FleetOperationsKPIs
                    {
                        ActiveVessels = 4,
                        OnTimePerformance = 94.2,
                        AverageSpeed = 16.8,
                        FuelEfficiency = 87.5,
                        CurrentlyAtSea = 3,
                        InPort = 1,
                        TotalVoyagesThisMonth = 28,
                        PassengersThisMonth = 8247
                    },
                    Environmental = new EnvironmentalKPIs
                    {
                        CO2EmissionsToday = 23.7,
                        CO2ReductionPercentage = 42.3,
                        HybridModeUsage = 67.8,
                        WasteRecyclingRate = 89.2,
                        EnergyEfficiencyScore = 91.5,
                        EnvironmentalComplianceScore = 98.7
                    },
                    PassengerExperience = new PassengerExperienceKPIs
                    {
                        AverageRating = 4.6,
                        NorthernLightsVisibilityTonight = 78.0,
                        ServiceComplaintRate = 2.1,
                        RecommendationScore = 92.4,
                        OnboardExperienceRating = 4.7,
                        DiningServiceRating = 4.5
                    },
                    Safety = new SafetyKPIs
                    {
                        IncidentsThisMonth = 0,
                        SafetyDrillCompliance = 100.0,
                        EmergencyResponseTime = 3.2,
                        SafetyTrainingCompletion = 97.8,
                        EquipmentReadinessScore = 99.1,
                        CrewSafetyCertificationRate = 100.0
                    },
                    Financial = new FinancialKPIs
                    {
                        RevenueThisMonth = 12_450_000,
                        BookingOccupancyRate = 87.3,
                        RevenuePerPassenger = 1_510,
                        OperationalCostPerVoyage = 185_000,
                        ProfitMargin = 23.8,
                        SeasonalVariance = -12.5 // Compared to peak season
                    },
                    LastUpdated = DateTime.UtcNow
                };

                return Ok(kpiSummary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving maritime KPI summary");
                return StatusCode(500, "Error retrieving KPI summary");
            }
        }

        [HttpGet("northern-lights/forecast")]
        public IActionResult GetNorthernLightsForecast()
        {
            try
            {
                var forecast = new NorthernLightsForecast
                {
                    CurrentActivity = "Moderate",
                    ActivityLevel = 6, // Scale of 1-10
                    VisibilityPercentage = 78.0,
                    BestViewingTime = DateTime.Today.AddHours(22), // 10 PM tonight
                    WeatherConditions = "Partly cloudy, clearing after midnight",
                    OptimalViewingLocations = new List<ViewingLocation>
                    {
                        new ViewingLocation
                        {
                            Name = "Upper Deck - Bow",
                            Latitude = 70.2,
                            Longitude = 28.1,
                            VisibilityScore = 9.2,
                            LightPollution = "Minimal",
                            Recommendation = "Excellent viewing position with unobstructed north view"
                        },
                        new ViewingLocation
                        {
                            Name = "Upper Deck - Starboard",
                            Latitude = 70.2,
                            Longitude = 28.1,
                            VisibilityScore = 8.7,
                            LightPollution = "Low",
                            Recommendation = "Good viewing with slight obstruction from ship's lighting"
                        }
                    },
                    Forecast7Day = new List<DailyForecast>
                    {
                        new DailyForecast { Date = DateTime.Today, ActivityLevel = 6, VisibilityPercentage = 78 },
                        new DailyForecast { Date = DateTime.Today.AddDays(1), ActivityLevel = 8, VisibilityPercentage = 85 },
                        new DailyForecast { Date = DateTime.Today.AddDays(2), ActivityLevel = 4, VisibilityPercentage = 45 },
                        new DailyForecast { Date = DateTime.Today.AddDays(3), ActivityLevel = 7, VisibilityPercentage = 72 },
                        new DailyForecast { Date = DateTime.Today.AddDays(4), ActivityLevel = 9, VisibilityPercentage = 92 },
                        new DailyForecast { Date = DateTime.Today.AddDays(5), ActivityLevel = 5, VisibilityPercentage = 58 },
                        new DailyForecast { Date = DateTime.Today.AddDays(6), ActivityLevel = 6, VisibilityPercentage = 69 }
                    },
                    LastUpdated = DateTime.UtcNow.AddMinutes(-15)
                };

                return Ok(forecast);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Northern Lights forecast");
                return StatusCode(500, "Error retrieving forecast");
            }
        }

        [HttpGet("reports/{reportId}/data")]
        public IActionResult GetReportData(string reportId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                // Generate sample data based on report type
                var reportData = reportId.ToLower() switch
                {
                    "fleet-overview" => GenerateFleetOverviewData(start, end),
                    "emissions-tracking" => GenerateEmissionsData(start, end),
                    "passenger-satisfaction" => GeneratePassengerSatisfactionData(start, end),
                    "northern-lights-forecast" => GenerateNorthernLightsData(start, end),
                    "route-efficiency" => GenerateRouteEfficiencyData(start, end),
                    "safety-incidents" => GenerateSafetyIncidentData(start, end),
                    "revenue-analysis" => GenerateRevenueAnalysisData(start, end),
                    _ => new { message = "Report data not available" }
                };

                return Ok(reportData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving report data for {ReportId}", reportId);
                return StatusCode(500, "Error retrieving report data");
            }
        }

        // Private helper methods
        private async Task<PowerBIClient> GetPowerBIClient()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var authenticationContext = new AuthenticationContext($"https://login.microsoftonline.com/{_tenantId}");
            var credential = new ClientCredential(_clientId, _clientSecret);
            var authenticationResult = await authenticationContext.AcquireTokenAsync("https://analysis.windows.net/powerbi/api", credential);
#pragma warning restore CS0618 // Type or member is obsolete

            var tokenCredentials = new TokenCredentials(authenticationResult.AccessToken, "Bearer");
            return new PowerBIClient(new Uri("https://api.powerbi.com/"), tokenCredentials);
        }

        private bool IsDashboardDataset(string datasetName, string dashboardId)
        {
            var dashboardDatasets = new Dictionary<string, string[]>
            {
                ["fleet-operations"] = new[] { "VesselTelemetry", "AISData", "RouteData" },
                ["environmental-compliance"] = new[] { "EnvironmentalSensors", "EmissionData" },
                ["passenger-experience"] = new[] { "PassengerFeedback", "BookingData", "NorthernLightsData" },
                ["route-analytics"] = new[] { "NavigationData", "WeatherData", "RouteOptimization" },
                ["safety-security"] = new[] { "SafetyIncidents", "EmergencyEquipment", "CrewData" },
                ["financial-performance"] = new[] { "BookingSystem", "FinancialData", "OperationalCosts" }
            };

            return dashboardDatasets.ContainsKey(dashboardId) && 
                   dashboardDatasets[dashboardId].Any(ds => datasetName.Contains(ds, StringComparison.OrdinalIgnoreCase));
        }

        private List<DashboardTile> GetFleetOperationsTiles()
        {
            return new List<DashboardTile>
            {
                new DashboardTile { Id = "active-vessels", Title = "Active Vessels", Value = "4", Type = "metric", Color = "#1f77b4" },
                new DashboardTile { Id = "on-time-performance", Title = "On-Time Performance", Value = "94.2%", Type = "percentage", Color = "#2ca02c" },
                new DashboardTile { Id = "fuel-efficiency", Title = "Fuel Efficiency", Value = "87.5%", Type = "percentage", Color = "#ff7f0e" },
                new DashboardTile { Id = "current-passengers", Title = "Current Passengers", Value = "1,247", Type = "metric", Color = "#d62728" }
            };
        }

        private List<DashboardTile> GetEnvironmentalTiles()
        {
            return new List<DashboardTile>
            {
                new DashboardTile { Id = "co2-reduction", Title = "CO2 Reduction", Value = "42.3%", Type = "percentage", Color = "#2ca02c" },
                new DashboardTile { Id = "hybrid-usage", Title = "Hybrid Mode Usage", Value = "67.8%", Type = "percentage", Color = "#17becf" },
                new DashboardTile { Id = "compliance-score", Title = "Environmental Compliance", Value = "98.7%", Type = "percentage", Color = "#2ca02c" },
                new DashboardTile { Id = "waste-recycling", Title = "Waste Recycling Rate", Value = "89.2%", Type = "percentage", Color = "#bcbd22" }
            };
        }

        private List<DashboardTile> GetPassengerExperienceTiles()
        {
            return new List<DashboardTile>
            {
                new DashboardTile { Id = "avg-rating", Title = "Average Rating", Value = "4.6/5", Type = "rating", Color = "#ff7f0e" },
                new DashboardTile { Id = "northern-lights-visibility", Title = "Northern Lights Visibility", Value = "78%", Type = "percentage", Color = "#9467bd" },
                new DashboardTile { Id = "recommendation-score", Title = "Recommendation Score", Value = "92.4%", Type = "percentage", Color = "#2ca02c" },
                new DashboardTile { Id = "complaint-rate", Title = "Complaint Rate", Value = "2.1%", Type = "percentage", Color = "#d62728" }
            };
        }

        private List<DashboardTile> GetRouteAnalyticsTiles()
        {
            return new List<DashboardTile>
            {
                new DashboardTile { Id = "route-efficiency", Title = "Route Efficiency", Value = "91.3%", Type = "percentage", Color = "#1f77b4" },
                new DashboardTile { Id = "weather-delays", Title = "Weather Delays", Value = "3", Type = "metric", Color = "#d62728" },
                new DashboardTile { Id = "port-efficiency", Title = "Port Turnaround Efficiency", Value = "87.6%", Type = "percentage", Color = "#2ca02c" },
                new DashboardTile { Id = "navigation-alerts", Title = "Navigation Alerts", Value = "2", Type = "metric", Color = "#ff7f0e" }
            };
        }

        private List<DashboardTile> GetSafetySecurityTiles()
        {
            return new List<DashboardTile>
            {
                new DashboardTile { Id = "incidents-month", Title = "Incidents This Month", Value = "0", Type = "metric", Color = "#2ca02c" },
                new DashboardTile { Id = "drill-compliance", Title = "Safety Drill Compliance", Value = "100%", Type = "percentage", Color = "#2ca02c" },
                new DashboardTile { Id = "equipment-readiness", Title = "Equipment Readiness", Value = "99.1%", Type = "percentage", Color = "#2ca02c" },
                new DashboardTile { Id = "crew-certification", Title = "Crew Certification Rate", Value = "100%", Type = "percentage", Color = "#2ca02c" }
            };
        }

        private List<DashboardTile> GetFinancialPerformanceTiles()
        {
            return new List<DashboardTile>
            {
                new DashboardTile { Id = "monthly-revenue", Title = "Monthly Revenue", Value = "€12.45M", Type = "currency", Color = "#2ca02c" },
                new DashboardTile { Id = "occupancy-rate", Title = "Occupancy Rate", Value = "87.3%", Type = "percentage", Color = "#1f77b4" },
                new DashboardTile { Id = "profit-margin", Title = "Profit Margin", Value = "23.8%", Type = "percentage", Color = "#ff7f0e" },
                new DashboardTile { Id = "revenue-per-passenger", Title = "Revenue per Passenger", Value = "€1,510", Type = "currency", Color = "#9467bd" }
            };
        }

        // Data generation methods (in real implementation, these would query actual data sources)
        private object GenerateFleetOverviewData(DateTime start, DateTime end)
        {
            return new
            {
                vessels = new[]
                {
                    new { name = "MS Havila Capella", status = "En Route", position = new { lat = 70.2, lng = 29.1 }, passengers = 312 },
                    new { name = "MS Havila Castor", status = "In Port", position = new { lat = 60.4, lng = 5.3 }, passengers = 298 },
                    new { name = "MS Havila Polaris", status = "En Route", position = new { lat = 68.8, lng = 33.0 }, passengers = 356 },
                    new { name = "MS Havila Pollux", status = "En Route", position = new { lat = 67.3, lng = 14.4 }, passengers = 281 }
                },
                routeProgress = new { completed = 67.3, remaining = 32.7 },
                onTimePerformance = 94.2
            };
        }

        private object GenerateEmissionsData(DateTime start, DateTime end)
        {
            return new
            {
                co2Emissions = new { current = 23.7, target = 40.0, reduction = 42.3 },
                hybridUsage = 67.8,
                energyEfficiency = 91.5,
                complianceScore = 98.7
            };
        }

        private object GeneratePassengerSatisfactionData(DateTime start, DateTime end)
        {
            return new
            {
                overallRating = 4.6,
                serviceRatings = new
                {
                    dining = 4.5,
                    accommodation = 4.7,
                    entertainment = 4.3,
                    staff = 4.8
                },
                recommendationScore = 92.4,
                complaintRate = 2.1
            };
        }

        private object GenerateNorthernLightsData(DateTime start, DateTime end)
        {
            return new
            {
                currentActivity = 6,
                visibilityPercentage = 78.0,
                bestViewingTime = "22:00-02:00",
                weatherConditions = "Partly cloudy, clearing after midnight"
            };
        }

        private object GenerateRouteEfficiencyData(DateTime start, DateTime end)
        {
            return new
            {
                efficiency = 91.3,
                fuelSavings = 12.7,
                timeOptimization = 8.4,
                weatherImpact = 3.2
            };
        }

        private object GenerateSafetyIncidentData(DateTime start, DateTime end)
        {
            return new
            {
                incidents = 0,
                drillCompliance = 100.0,
                equipmentReadiness = 99.1,
                crewCertification = 100.0
            };
        }

        private object GenerateRevenueAnalysisData(DateTime start, DateTime end)
        {
            return new
            {
                monthlyRevenue = 12450000,
                occupancyRate = 87.3,
                revenuePerPassenger = 1510,
                profitMargin = 23.8
            };
        }
    }

    // Data models for Power BI integration
    public class MaritimeDashboard
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public List<PowerBIReport> Reports { get; set; } = new();
        public List<DashboardTile> Tiles { get; set; } = new();
        public DateTime LastRefresh { get; set; }
        public string AccessLevel { get; set; } = string.Empty;
    }

    public class PowerBIReport
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string RefreshFrequency { get; set; } = string.Empty;
        public string[] DataSources { get; set; } = Array.Empty<string>();
    }

    public class DashboardTile
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }

    public class PowerBIEmbedConfig
    {
        public string Type { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string EmbedUrl { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string TokenId { get; set; } = string.Empty;
        public DateTime? Expiration { get; set; }
        public EmbedSettings Settings { get; set; } = new();
    }

    public class EmbedSettings
    {
        public bool FilterPaneEnabled { get; set; }
        public bool NavContentPaneEnabled { get; set; }
        public string Background { get; set; } = string.Empty;
    }

    public class MaritimeKPISummary
    {
        public FleetOperationsKPIs FleetOperations { get; set; } = new();
        public EnvironmentalKPIs Environmental { get; set; } = new();
        public PassengerExperienceKPIs PassengerExperience { get; set; } = new();
        public SafetyKPIs Safety { get; set; } = new();
        public FinancialKPIs Financial { get; set; } = new();
        public DateTime LastUpdated { get; set; }
    }

    public class FleetOperationsKPIs
    {
        public int ActiveVessels { get; set; }
        public double OnTimePerformance { get; set; }
        public double AverageSpeed { get; set; }
        public double FuelEfficiency { get; set; }
        public int CurrentlyAtSea { get; set; }
        public int InPort { get; set; }
        public int TotalVoyagesThisMonth { get; set; }
        public int PassengersThisMonth { get; set; }
    }

    public class EnvironmentalKPIs
    {
        public double CO2EmissionsToday { get; set; }
        public double CO2ReductionPercentage { get; set; }
        public double HybridModeUsage { get; set; }
        public double WasteRecyclingRate { get; set; }
        public double EnergyEfficiencyScore { get; set; }
        public double EnvironmentalComplianceScore { get; set; }
    }

    public class PassengerExperienceKPIs
    {
        public double AverageRating { get; set; }
        public double NorthernLightsVisibilityTonight { get; set; }
        public double ServiceComplaintRate { get; set; }
        public double RecommendationScore { get; set; }
        public double OnboardExperienceRating { get; set; }
        public double DiningServiceRating { get; set; }
    }

    public class SafetyKPIs
    {
        public int IncidentsThisMonth { get; set; }
        public double SafetyDrillCompliance { get; set; }
        public double EmergencyResponseTime { get; set; }
        public double SafetyTrainingCompletion { get; set; }
        public double EquipmentReadinessScore { get; set; }
        public double CrewSafetyCertificationRate { get; set; }
    }

    public class FinancialKPIs
    {
        public decimal RevenueThisMonth { get; set; }
        public double BookingOccupancyRate { get; set; }
        public decimal RevenuePerPassenger { get; set; }
        public decimal OperationalCostPerVoyage { get; set; }
        public double ProfitMargin { get; set; }
        public double SeasonalVariance { get; set; }
    }

    public class NorthernLightsForecast
    {
        public string CurrentActivity { get; set; } = string.Empty;
        public int ActivityLevel { get; set; }
        public double VisibilityPercentage { get; set; }
        public DateTime BestViewingTime { get; set; }
        public string WeatherConditions { get; set; } = string.Empty;
        public List<ViewingLocation> OptimalViewingLocations { get; set; } = new();
        public List<DailyForecast> Forecast7Day { get; set; } = new();
        public DateTime LastUpdated { get; set; }
    }

    public class ViewingLocation
    {
        public string Name { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double VisibilityScore { get; set; }
        public string LightPollution { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
    }

    public class DailyForecast
    {
        public DateTime Date { get; set; }
        public int ActivityLevel { get; set; }
        public double VisibilityPercentage { get; set; }
    }
}