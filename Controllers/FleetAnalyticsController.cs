using Microsoft.AspNetCore.Mvc;
using MaritimeIQ.Platform.Models;
using MaritimeIQ.Platform.Data;

namespace MaritimeIQ.Platform.Controllers
{
    /// <summary>
    /// Fleet Analytics Service - Provides comprehensive analytics, reporting, 
    /// and business intelligence for maritime operations
    /// </summary>
    [ApiController]
    [Route("api/fleet-analytics")]
    [Tags("Fleet Analytics")]
    public class FleetAnalyticsController : ControllerBase
    {
        private readonly ILogger<FleetAnalyticsController> _logger;

        public FleetAnalyticsController(ILogger<FleetAnalyticsController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Get comprehensive fleet performance dashboard data
        /// </summary>
        [HttpGet("dashboard")]
        public ActionResult<object> GetFleetDashboard()
        {
            _logger.LogInformation("Generating fleet analytics dashboard data");

        var fleet = MaritimeFleetData.GetMaritimeFleet();
        var positions = MaritimeFleetData.GetCurrentVesselPositions();
        var environmental = MaritimeFleetData.GetSampleEnvironmentalData();
        var voyages = MaritimeFleetData.GetSampleVoyages();

            var dashboard = new
            {
                LastUpdated = DateTime.UtcNow,
                DashboardPeriod = "Real-time + Last 30 days",
                FleetOverview = new
                {
                    TotalVessels = fleet.Count(),
                    VesselsInService = fleet.Count(v => v.Status == VesselStatus.InService),
                    TotalPassengerCapacity = fleet.Sum(v => v.PassengerCapacity),
                FleetUtilization = CalculateFleetUtilization(voyages, fleet),
                AverageVesselAge = CalculateAverageVesselAge(fleet),
                FleetValue = CalculateFleetValue(fleet)
                },
                OperationalMetrics = new
                {
                    OnTimePerformance = new
                    {
                        CurrentMonth = "94.7%",
                        LastMonth = "92.1%",
                        YearToDate = "93.8%",
                        Trend = "Improving"
                    },
                    RouteCompletion = new
                    {
                        Completed = voyages.Count(v => v.Status == VoyageStatus.Completed),
                        InProgress = voyages.Count(v => v.Status == VoyageStatus.InProgress),
                        Scheduled = voyages.Count(v => v.Status == VoyageStatus.Scheduled),
                        CompletionRate = "98.9%"
                    },
                    PassengerMetrics = new
                    {
                        AverageOccupancy = "76.8%",
                        TotalPassengersYTD = 847630,
                        CustomerSatisfaction = 4.7,
                        RepeatCustomerRate = "42%"
                    },
                    SafetyMetrics = new
                    {
                        IncidentFreeRunning = "347 days",
                        SafetyRating = "A+",
                        ComplianceScore = "100%",
                        EmergencyDrills = "All current, next due in 45 days"
                    }
                },
                EnvironmentalPerformance = new
                {
                    CO2Emissions = new
                    {
                        Daily = $"{environmental.Where(e => e.MeasurementTime > DateTime.UtcNow.AddDays(-1)).Sum(e => e.CO2EmissionKg):F1} kg",
                        Monthly = "2,847 tons",
                        ReductionVsTarget = "18% below industry average",
                        ComplianceStatus = "Exceeds IMO 2030 requirements"
                    },
                    FuelEfficiency = new
                    {
                        AverageConsumption = $"{environmental.Average(e => e.FuelConsumptionLiters):F1} L/hour",
                        EfficiencyImprovement = "15% better than previous fleet",
                        HybridUtilization = $"{environmental.Average(e => e.BatteryStateOfCharge):F1}% battery SOC",
                        BestPerformingVessel = "MS Nordic Spirit (19% above target)"
                    },
                    SustainabilityScore = new
                    {
                        Overall = "9.2/10",
                        WasteManagement = "A+",
                        EnergyEfficiency = "A",
                        WaterConservation = "A+",
                        LocalSourcing = "B+"
                    }
                },
                FinancialMetrics = new
                {
                    Revenue = new
                    {
                        Monthly = "€12.4M",
                        YearToDate = "€89.7M",
                        GrowthRate = "+8.3% vs last year",
                        Forecast = "€112M annual target - on track"
                    },
                    OperationalCosts = new
                    {
                        FuelCosts = "€2.1M/month",
                        MaintenanceCosts = "€890K/month",
                        CrewCosts = "€3.2M/month",
                        TotalOpex = "€7.8M/month"
                    },
                    Profitability = new
                    {
                        GrossMargin = "38.2%",
                        EBITDA = "€4.2M/month",
                        ROI = "12.8%",
                        BreakEvenOccupancy = "58%"
                    }
                },
                RouteAnalytics = new
                {
                    BergenKirkenes = new
                    {
                        RouteName = "Northbound Classic Route",
                        AverageOccupancy = "81.3%",
                        OnTimePerformance = "95.1%",
                        PopularSeasons = new[] { "Northern Lights (Oct-Mar)", "Midnight Sun (May-Aug)" },
                        RevenueContribution = "67%"
                    },
                    KirkenesBergen = new
                    {
                        RouteName = "Southbound Return Route",
                        AverageOccupancy = "73.7%",
                        OnTimePerformance = "93.8%",
                        ExperienceRating = 4.8,
                        RevenueContribution = "33%"
                    },
                    MostPopularPorts = new[]
                    {
                        new { Port = "Tromsø", Popularity = "98%", Attraction = "Northern Lights Capital" },
                        new { Port = "Bergen", Popularity = "95%", Attraction = "UNESCO World Heritage" },
                        new { Port = "Ålesund", Popularity = "87%", Attraction = "Art Nouveau Architecture" },
                        new { Port = "Kirkenes", Popularity = "82%", Attraction = "Russian Border Experience" }
                    }
                },
                WeatherImpact = new
                {
                    WeatherDelays = "2.3% of total sailing time",
                    MostAffectedSeason = "Winter storms (Nov-Feb)",
                    AuroraVisibility = new
                    {
                        Last30Days = "Excellent (87% clear nights)",
                        CustomerSatisfaction = "4.9/5 for Northern Lights viewing",
                        OptimalPorts = new[] { "Tromsø", "Alta", "Kirkenes" }
                    },
                    SeasonalTrends = new
                    {
                        Summer = "99.2% schedule reliability",
                        Winter = "91.7% schedule reliability", 
                        Aurora = "Peak season Dec-Feb with 95% viewing success"
                    }
                }
            };

            return Ok(dashboard);
        }

        /// <summary>
        /// Get detailed vessel performance comparison
        /// </summary>
        [HttpGet("vessel-comparison")]
        public ActionResult<object> GetVesselComparison()
        {
            _logger.LogInformation("Generating vessel performance comparison");

        var fleet = MaritimeFleetData.GetMaritimeFleet();
        var environmental = MaritimeFleetData.GetSampleEnvironmentalData();
        var voyages = MaritimeFleetData.GetSampleVoyages();

            var comparison = fleet.Select(vessel => new
            {
                VesselId = vessel.Id,
                VesselName = vessel.Name,
                Status = vessel.Status.ToString(),
                Performance = new
                {
                    FuelEfficiency = $"{environmental.Where(e => e.VesselId == vessel.Id).Average(e => e.FuelConsumptionLiters):F1} L/h",
                    CO2Emissions = $"{environmental.Where(e => e.VesselId == vessel.Id).Average(e => e.CO2EmissionKg):F1} kg/h",
                    BatteryUsage = $"{environmental.Where(e => e.VesselId == vessel.Id).Average(e => e.BatteryStateOfCharge):F1}%",
                    OperationalHours = CalculateOperationalHours(vessel.Id, voyages),
                    MaintenanceScore = CalculateMaintenanceScore(vessel.Id, environmental) + "%"
                },
                Utilization = new
                {
                    Occupancy = CalculateOccupancyRate(vessel.Id, voyages) + "%",
                    RevenuePerDay = "€" + CalculateDailyRevenue(vessel.Id, voyages).ToString("N0"),
                    SailingDays = CalculateSailingDays(vessel.Id, voyages),
                    PortDays = CalculatePortDays(vessel.Id, voyages)
                },
                CustomerSatisfaction = new
                {
                    Rating = CalculateCustomerRating(vessel.Id, voyages),
                    WeatherViewing = CalculateWeatherViewingRating(vessel.Id, voyages),
                    ServiceQuality = CalculateServiceQuality(vessel.Id, voyages),
                    CabinComfort = CalculateCabinComfortRating(vessel.Id)
                }
            }).ToList();

            var summary = new
            {
                ComparisonDate = DateTime.UtcNow,
                FleetAverage = new
                {
                    FuelEfficiency = comparison.Average(v => double.Parse(v.Performance.FuelEfficiency.Replace(" L/h", ""))),
                    CustomerRating = comparison.Average(v => v.CustomerSatisfaction.Rating),
                    Occupancy = comparison.Average(v => int.Parse(v.Utilization.Occupancy.Replace("%", "")))
                },
                TopPerformer = new
                {
                    Vessel = comparison.OrderByDescending(v => v.CustomerSatisfaction.Rating).First().VesselName,
                    Category = "Highest customer satisfaction",
                    Score = comparison.Max(v => v.CustomerSatisfaction.Rating)
                },
                VesselDetails = comparison
            };

            return Ok(summary);
        }

        /// <summary>
        /// Get environmental compliance and sustainability report
        /// </summary>
        [HttpGet("environmental-report")]
        public ActionResult<object> GetEnvironmentalReport()
        {
            _logger.LogInformation("Generating environmental compliance report");

            var environmental = MaritimeFleetData.GetSampleEnvironmentalData();
            
            var report = new
            {
                ReportPeriod = "Last 30 days",
                GeneratedDate = DateTime.UtcNow,
                ComplianceStatus = "FULLY COMPLIANT",
                CertificationLevel = "Green Marine Certified Level 5",
                
                EmissionsData = new
                {
                    CO2Emissions = new
                    {
                        Total = $"{environmental.Sum(e => e.CO2EmissionKg):F0} kg",
                        Daily = $"{environmental.Where(e => e.MeasurementTime > DateTime.UtcNow.AddDays(-1)).Sum(e => e.CO2EmissionKg):F0} kg",
                        PerPassengerKm = "42g CO2/passenger/km",
                        IndustryComparison = "28% below industry average",
                        TargetCompliance = "Exceeds IMO 2030 targets by 15%"
                    },
                    NOxEmissions = new
                    {
                        Reduction = "45% below MARPOL Tier III limits",
                        Technology = "Selective Catalytic Reduction (SCR)",
                        ComplianceStatus = "Excellent"
                    },
                    SOxEmissions = new
                    {
                        Reduction = "78% below limits",
                        FuelType = "Marine Gas Oil (MGO) + Battery hybrid",
                        SulfurContent = "0.1% (well below 0.5% limit)"
                    }
                },
                
                EnergyEfficiency = new
                {
                    HybridTechnology = new
                    {
                        BatteryCapacity = "6.1 MWh per vessel",
                        ElectricRange = "4 hours pure electric operation",
                        BatteryUtilization = $"{environmental.Average(e => e.BatteryStateOfCharge):F1}% average SOC",
                        EnergyRecovery = "Regenerative systems active"
                    },
                    FuelConsumption = new
                    {
                        Average = $"{environmental.Average(e => e.FuelConsumptionLiters):F1} L/hour",
                        Efficiency = "15% better than previous generation vessels",
                        OptimizationSavings = "€1.2M annually through AI optimization"
                    },
                    RenewableEnergy = new
                    {
                        ShoreConnection = "100% renewable Norwegian grid power in ports",
                        SolarPanels = "Auxiliary power generation",
                        EnergyStorage = "Advanced lithium-ion battery systems"
                    }
                },
                
                WasteManagement = new
                {
                    WasteRecycling = "87% waste diverted from landfill",
                    FoodWaste = "34% reduction through AI-driven meal planning",
                    PlasticReduction = "Single-use plastics eliminated",
                    WaterTreatment = "Advanced biological treatment systems"
                },
                
                BiodiversityProtection = new
                {
                    WildlifeObservation = "Certified marine mammal observation protocols",
                    RoutePlanning = "Dynamic routing to avoid sensitive areas",
                    NoiseReduction = "Propeller design optimized for marine life",
                    BallastWater = "UV treatment system - no invasive species risk"
                },
                
                Certifications = new object[]
                {
                    new { Name = "Green Marine", Level = "Level 5", ValidUntil = "2025-12-31" },
                    new { Name = "ISO 14001", Status = "Certified", ValidUntil = "2024-08-15" },
                    new { Name = "Clean Shipping Index", Score = "A+ Rating", ValidUntil = "2024-12-31" },
                    new { Name = "Norwegian Eco-Lighthouse", Status = "Certified", ValidUntil = "2025-06-30" }
                },
                
                SustainabilityTargets = new
                {
                    CO2Reduction = new { Target = "50% by 2030", Progress = "32% completed", Status = "On track" },
                    ZeroWaste = new { Target = "95% waste diversion by 2025", Progress = "87%", Status = "Ahead of schedule" },
                    LocalSourcing = new { Target = "80% Norwegian suppliers", Progress = "73%", Status = "On track" },
                    CrewWelfare = new { Target = "Best maritime employer 2025", Progress = "Industry leading", Status = "Excellent" }
                },
                
                NextActions = new[]
                {
                    "Install additional battery capacity (Q2 2024)",
                    "Implement AI-driven waste sorting (Q3 2024)",
                    "Complete shore power connections at all ports (Q4 2024)",
                    "Achieve net-zero emissions pilot route (2025)"
                }
            };

            return Ok(report);
        }

        /// <summary>
        /// Get real-time fleet performance metrics for monitoring
        /// </summary>
        [HttpGet("real-time-metrics")]
        public ActionResult<object> GetRealTimeMetrics()
        {
            _logger.LogInformation("Fetching real-time fleet performance metrics");

        var positions = MaritimeFleetData.GetCurrentVesselPositions();
        var environmental = MaritimeFleetData.GetSampleEnvironmentalData()
                                              .Where(e => e.MeasurementTime > DateTime.UtcNow.AddHours(-1));

            var metrics = new
            {
                Timestamp = DateTime.UtcNow,
                RefreshRate = "Every 30 seconds",
                DataSources = new[] { "AIS", "Engine sensors", "Environmental monitors", "Navigation systems" },
                
                FleetStatus = new
                {
                    VesselsOperational = positions.Count(),
                    AverageSpeed = $"{positions.Average(p => p.SpeedKnots):F1} knots",
                    TotalDistanceCovered = "1,247 nautical miles (last 24h)",
                    FuelConsumed = $"{environmental.Sum(e => e.FuelConsumptionLiters):F0} liters (last hour)"
                },
                
                LiveVesselData = positions.Select(pos => new
                {
                    VesselName = pos.Vessel?.Name ?? $"Vessel {pos.VesselId}",
                    CurrentPosition = new
                    {
                        Latitude = pos.Latitude,
                        Longitude = pos.Longitude,
                        NearestPort = pos.CurrentPort,
                        DistanceToNext = $"{new Random().Next(15, 85)} nm to {pos.NextPort}"
                    },
                    Movement = new
                    {
                        Speed = $"{pos.SpeedKnots} knots",
                        Heading = $"{pos.HeadingDegrees}°",
                        Status = pos.NavigationStatus,
                        ETA = pos.EstimatedArrival?.ToString("HH:mm dd/MM")
                    },
                    Conditions = new
                    {
                        Weather = pos.WeatherCondition,
                        WindSpeed = $"{pos.WindSpeedKnots} knots",
                        WaveHeight = $"{pos.WaveHeightM}m",
                        Visibility = $"{pos.VisibilityKm}km"
                    },
                    Alerts = pos.IsEmergency ? new[] { "EMERGENCY SITUATION - COAST GUARD NOTIFIED" } : new string[] { }
                }).ToArray(),
                
                AggregatedMetrics = new
                {
                    FleetEfficiency = new
                    {
                        AverageFuelRate = $"{environmental.Average(e => e.FuelConsumptionLiters):F1} L/h",
                        CO2Rate = $"{environmental.Average(e => e.CO2EmissionKg):F1} kg/h",
                        BatteryUtilization = $"{environmental.Average(e => e.BatteryStateOfCharge):F1}%",
                        EfficiencyScore = "94.2/100"
                    },
                    SafetyMetrics = new
                    {
                        AllVesselsReporting = true,
                        EmergencyAlerts = 0,
                        WeatherWarnings = 0,
                        NavigationAlerts = 0,
                        LastSafetyDrill = "3 days ago"
                    },
                    EnvironmentalCompliance = new
                    {
                        ComplianceStatus = "100% compliant",
                        EmissionLimits = "Within all regulatory limits",
                        WasteDischarge = "Zero discharge - all systems operational",
                        NoiseLevels = "Below marine life impact thresholds"
                    }
                },
                
                PerformanceAlerts = new[]
                {
                    new { Type = "INFO", Message = "All vessels operating within normal parameters", Timestamp = DateTime.UtcNow },
                    new { Type = "SUCCESS", Message = "Fuel efficiency 5% above target for this route segment", Timestamp = DateTime.UtcNow.AddMinutes(-15) },
                    new { Type = "INFO", Message = "Excellent Northern Lights visibility conditions tonight", Timestamp = DateTime.UtcNow.AddMinutes(-23) }
                }
            };

            return Ok(metrics);
        }

        /// <summary>
        /// Get predictive analytics and forecasting for fleet operations
        /// </summary>
        [HttpGet("predictive-analytics")]
        public ActionResult<object> GetPredictiveAnalytics()
        {
            _logger.LogInformation("Generating predictive analytics for fleet operations");

            var analytics = new
            {
                ForecastPeriod = "Next 30 days",
                GeneratedDate = DateTime.UtcNow,
                ConfidenceLevel = "94.7%",
                ModelVersion = "FleetPredict v3.2",
                
                DemandForecasting = new
                {
                    ExpectedOccupancy = new
                    {
                        Week1 = "78% (Peak Northern Lights season)",
                        Week2 = "85% (Christmas holidays)",
                        Week3 = "71% (Post-holiday dip)",
                        Week4 = "82% (New Year recovery)"
                    },
                    BookingTrends = new
                    {
                        EarlyBookings = "+12% vs last year",
                        LastMinute = "Stable demand",
                        InternationalGuests = "+18% (German, UK markets)",
                        RepeatCustomers = "43% of bookings"
                    },
                    RevenuePrediction = new
                    {
                        OptimisticScenario = "€13.2M (+8.5%)",
                        MostLikely = "€12.8M (+5.2%)",
                        ConservativeScenario = "€11.9M (-2.1%)",
                        ConfidenceRange = "€12.4M - €13.1M"
                    }
                },
                
                OperationalPredictions = new
                {
                    MaintenanceScheduling = new[]
                    {
                        new { Vessel = "MS Nordic Aurora", NextMaintenance = "2024-01-15", Type = "Engine overhaul", Duration = "3 days" },
                        new { Vessel = "MS Arctic Explorer", NextMaintenance = "2024-01-28", Type = "Battery service", Duration = "1 day" },
                        new { Vessel = "MS Coastal Voyager", NextMaintenance = "2024-02-05", Type = "Hull inspection", Duration = "2 days" }
                    },
                    FuelCosts = new
                    {
                        PredictedConsumption = "187,500 liters",
                        CostEstimate = "€165,000 (+3.2% fuel price increase)",
                        SavingsOpportunity = "€8,400 through route optimization",
                        HedgingRecommendation = "Consider 70% hedge for Q1 2024"
                    },
                    CrewScheduling = new
                    {
                        RequiredCrew = 220,
                        TrainingNeeds = "12 officers, 8 engineers",
                        LeaveScheduling = "Peak leave period: Feb 15-28",
                        RecruitmentPlan = "4 new hires recommended Q1"
                    }
                },
                
                RiskAssessment = new
                {
                    WeatherRisks = new
                    {
                        StormProbability = "15% chance of significant delays",
                        IceConditions = "Normal for season - no route disruptions expected",
                        AuroraActivity = "High activity predicted - excellent for tourism"
                    },
                    OperationalRisks = new
                    {
                        EquipmentFailure = "Low risk (2.3% probability)",
                        CyberSecurity = "Enhanced monitoring active",
                        Regulatory = "New EU emissions standards - fully compliant",
                        Competition = "New entrant unlikely to affect market position"
                    },
                    FinancialRisks = new
                    {
                        CurrencyFluctuation = "NOK/EUR hedge 85% covered",
                        FuelPriceVolatility = "Moderate risk - hedging recommended",
                        InsuranceClaims = "Below industry average",
                        CreditRisk = "Excellent customer payment history"
                    }
                },
                
                OptimizationRecommendations = new[]
                {
                    new { 
                        Category = "Route Optimization", 
                        Recommendation = "Adjust departure times by 30 minutes to optimize tide conditions",
                        PotentialSaving = "€12,000/month fuel savings",
                        Implementation = "Automatic via AI routing system",
                        Priority = "High"
                    },
                    new { 
                        Category = "Crew Efficiency", 
                        Recommendation = "Cross-train crew for hybrid roles to reduce total staff needs",
                        PotentialSaving = "€45,000/quarter",
                        Implementation = "Q1 2024 training program",
                        Priority = "Medium"
                    },
                    new { 
                        Category = "Customer Experience", 
                        Recommendation = "Extend aurora viewing sessions during high activity periods",
                        PotentialSaving = "+€8,500 ancillary revenue/month",
                        Implementation = "Dynamic scheduling based on aurora forecasts",
                        Priority = "High"
                    }
                }
            };

            return Ok(analytics);
        }

        #region Calculation Helper Methods

        private string CalculateFleetUtilization(IReadOnlyList<Voyage> voyages, IReadOnlyList<Vessel> fleet)
        {
            if (!voyages.Any() || !fleet.Any()) return "0%";
            
            var activeVoyages = voyages.Count(v => v.Status == VoyageStatus.InProgress || v.Status == VoyageStatus.Scheduled);
            var totalCapacity = fleet.Sum(f => f.PassengerCapacity);
            var utilizationRate = (double)activeVoyages / totalCapacity * 100;
            
            return $"{utilizationRate:F1}%";
        }

        private double CalculateAverageVesselAge(IReadOnlyList<Vessel> fleet)
        {
            // Assuming vessels are modern hybrid vessels built around 2021-2022
            var currentYear = DateTime.UtcNow.Year;
            var averageConstructionYear = 2021.5; // Midpoint
            return Math.Round(currentYear - averageConstructionYear, 1);
        }

        private string CalculateFleetValue(IReadOnlyList<Vessel> fleet)
        {
            // Estimate based on modern hybrid ferry values (~€112M per vessel)
            var estimatedValuePerVessel = 112_000_000;
            var totalValue = fleet.Count * estimatedValuePerVessel;
            return $"€{totalValue / 1_000_000}M";
        }

        private int CalculateOperationalHours(int vesselId, IReadOnlyList<Voyage> voyages)
        {
            var vesselVoyages = voyages.Where(v => v.VesselId == vesselId);
            var totalHours = vesselVoyages.Sum(v => ((v.ArrivalTime ?? v.DepartureTime.AddDays(1)) - v.DepartureTime).TotalHours);
            
            // Estimate annual hours based on current data, with minimum baseline
            var annualEstimate = Math.Max(totalHours * (365.0 / 30.0), 6500); // Extrapolate monthly to yearly
            return (int)Math.Round(annualEstimate);
        }

        private int CalculateMaintenanceScore(int vesselId, IReadOnlyList<EnvironmentalData> environmental)
        {
            var vesselData = environmental.Where(e => e.VesselId == vesselId);
            if (!vesselData.Any()) return 90; // Default good score

            // Calculate based on environmental performance as proxy for maintenance
            var avgBatteryHealth = vesselData.Average(e => (double)e.BatteryStateOfCharge);
            var avgEfficiency = vesselData.Average(e => Math.Max(0, 100 - (double)e.FuelConsumptionLiters * 2)); // Inverse of fuel consumption
            
            return (int)Math.Round((avgBatteryHealth * 0.4 + avgEfficiency * 0.6));
        }

        private int CalculateOccupancyRate(int vesselId, IReadOnlyList<Voyage> voyages)
        {
            var vesselVoyages = voyages.Where(v => v.VesselId == vesselId);
            if (!vesselVoyages.Any()) return 75; // Default

            var avgOccupancy = vesselVoyages.Average(v => (double)(v.PassengerCount));
            var vessel = MaritimeFleetData.GetMaritimeFleet().FirstOrDefault(f => f.Id == vesselId);
            if (vessel == null) return 75;

            var occupancyRate = (avgOccupancy / vessel.PassengerCapacity) * 100;
            return Math.Max(60, Math.Min(95, (int)Math.Round(occupancyRate)));
        }

        private decimal CalculateDailyRevenue(int vesselId, IReadOnlyList<Voyage> voyages)
        {
            var vesselVoyages = voyages.Where(v => v.VesselId == vesselId);
            if (!vesselVoyages.Any()) return 38000; // Default

            // Estimate based on passenger count and average ticket price
            var avgPassengers = vesselVoyages.Average(v => (double)v.PassengerCount);
            var estimatedTicketPrice = 450; // Average coastal voyage ticket
            
            return (decimal)(avgPassengers * estimatedTicketPrice);
        }

        private int CalculateSailingDays(int vesselId, IReadOnlyList<Voyage> voyages)
        {
            var vesselVoyages = voyages.Where(v => v.VesselId == vesselId && 
                                              (v.Status == VoyageStatus.Completed || v.Status == VoyageStatus.InProgress));
            
            // Estimate annual sailing days
            var totalVoyageDays = vesselVoyages.Sum(v => ((v.ArrivalTime ?? v.DepartureTime.AddDays(1)) - v.DepartureTime).TotalDays);
            var annualEstimate = totalVoyageDays * (365.0 / 30.0); // Extrapolate to year
            
            return Math.Max(300, Math.Min(350, (int)Math.Round(annualEstimate)));
        }

        private int CalculatePortDays(int vesselId, IReadOnlyList<Voyage> voyages)
        {
            return 365 - CalculateSailingDays(vesselId, voyages);
        }

        private double CalculateCustomerRating(int vesselId, IReadOnlyList<Voyage> voyages)
        {
            // Base rating with variation based on vessel performance
            var baseRating = 4.5;
            var vesselVoyages = voyages.Where(v => v.VesselId == vesselId);
            
            if (vesselVoyages.Any())
            {
                var onTimePerformance = vesselVoyages.Count(v => v.Status == VoyageStatus.Completed) / (double)vesselVoyages.Count();
                var ratingModifier = (onTimePerformance - 0.8) * 0.5; // Adjust based on performance
                baseRating += ratingModifier;
            }
            
            return Math.Max(4.0, Math.Min(5.0, Math.Round(baseRating, 1)));
        }

        private double CalculateWeatherViewingRating(int vesselId, IReadOnlyList<Voyage> voyages)
        {
            // Weather viewing depends on routes and seasonal factors
            var currentMonth = DateTime.UtcNow.Month;
            var seasonalFactor = currentMonth >= 10 || currentMonth <= 3 ? 4.7 : 4.2; // Winter months better for aurora viewing
            
            return Math.Round(seasonalFactor, 1);
        }

        private double CalculateServiceQuality(int vesselId, IReadOnlyList<Voyage> voyages)
        {
            // Service quality based on vessel operational performance
            var vesselVoyages = voyages.Where(v => v.VesselId == vesselId);
            var baseQuality = 4.4;
            
            if (vesselVoyages.Any())
            {
                var completedVoyages = vesselVoyages.Count(v => v.Status == VoyageStatus.Completed);
                var totalVoyages = vesselVoyages.Count();
                var reliabilityFactor = totalVoyages > 0 ? (double)completedVoyages / totalVoyages : 0.9;
                baseQuality += (reliabilityFactor - 0.9) * 0.4;
            }
            
            return Math.Max(4.0, Math.Min(5.0, Math.Round(baseQuality, 1)));
        }

        private double CalculateCabinComfortRating(int vesselId)
        {
            // Modern hybrid vessels have high comfort ratings
            // Slight variation based on vessel ID to simulate differences
            var baseComfort = 4.3;
            var vesselVariation = (vesselId % 4) * 0.1; // Small variation between vessels
            
            return Math.Round(baseComfort + vesselVariation, 1);
        }

        #endregion
    }
}