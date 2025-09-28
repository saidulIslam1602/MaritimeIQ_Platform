using HavilaKystruten.Maritime.Models;
using MaritimeRoute = HavilaKystruten.Maritime.Models.Route;

namespace HavilaKystruten.Maritime.Data
{
    public static class HavilaFleetData
    {
        /// <summary>
        /// Havila Kystruten's hybrid coastal vessels
        /// </summary>
        public static List<Vessel> GetHavilaFleet()
        {
            return new List<Vessel>
            {
                new Vessel
                {
                    Id = 1,
                    Name = "Havila Capella",
                    IMONumber = "9850123",
                    CallSign = "LAJK",
                    Type = VesselType.PassengerFerry,
                    PassengerCapacity = 640,
                    CrewCapacity = 55,
                    LengthMeters = 123.3,
                    BeamMeters = 19.5,
                    DraftMeters = 5.3,
                    GrossTonnage = 15300,
                    Status = VesselStatus.InService,
                    LastUpdated = DateTime.UtcNow
                },
                new Vessel
                {
                    Id = 2,
                    Name = "Havila Castor",
                    IMONumber = "9850124",
                    CallSign = "LAJL",
                    Type = VesselType.PassengerFerry,
                    PassengerCapacity = 640,
                    CrewCapacity = 55,
                    LengthMeters = 123.3,
                    BeamMeters = 19.5,
                    DraftMeters = 5.3,
                    GrossTonnage = 15300,
                    Status = VesselStatus.InService,
                    LastUpdated = DateTime.UtcNow
                },
                new Vessel
                {
                    Id = 3,
                    Name = "Havila Polaris",
                    IMONumber = "9850125",
                    CallSign = "LAJM",
                    Type = VesselType.PassengerFerry,
                    PassengerCapacity = 640,
                    CrewCapacity = 55,
                    LengthMeters = 123.3,
                    BeamMeters = 19.5,
                    DraftMeters = 5.3,
                    GrossTonnage = 15300,
                    Status = VesselStatus.InService,
                    LastUpdated = DateTime.UtcNow
                },
                new Vessel
                {
                    Id = 4,
                    Name = "Havila Pollux",
                    IMONumber = "9850126",
                    CallSign = "LAJN",
                    Type = VesselType.PassengerFerry,
                    PassengerCapacity = 640,
                    CrewCapacity = 55,
                    LengthMeters = 123.3,
                    BeamMeters = 19.5,
                    DraftMeters = 5.3,
                    GrossTonnage = 15300,
                    Status = VesselStatus.InService,
                    LastUpdated = DateTime.UtcNow
                }
            };
        }

        /// <summary>
        /// Norwegian coastal ports for Bergen-Kirkenes route
        /// </summary>
        public static List<EnhancedPort> GetNorwegianCoastalPorts()
        {
            return new List<EnhancedPort>
            {
                new EnhancedPort
                {
                    Id = 1,
                    Name = "Bergen",
                    UNLocode = "NOBGO",
                    Latitude = 60.3913M,
                    Longitude = 5.3221M,
                    Region = "Western Norway",
                    HasPassengerFacilities = true,
                    HasCargoFacilities = true,
                    HasFuelStation = true,
                    HasMaintenance = true,
                    MaxVesselLengthM = 200M,
                    MaxDraftM = 8M,
                    IsNorthernLightsPort = false,
                    IsMidnightSunPort = false,
                    Attractions = "Bryggen, Fløibanen, Fish Market, Mount Ulriken",
                    AverageStopDurationMinutes = 30
                },
                new EnhancedPort
                {
                    Id = 2,
                    Name = "Ålesund",
                    UNLocode = "NOALE",
                    Latitude = 62.4722M,
                    Longitude = 6.1492M,
                    Region = "Western Norway",
                    HasPassengerFacilities = true,
                    HasCargoFacilities = true,
                    HasFuelStation = true,
                    HasMaintenance = false,
                    MaxVesselLengthM = 150M,
                    MaxDraftM = 6M,
                    IsNorthernLightsPort = false,
                    IsMidnightSunPort = false,
                    Attractions = "Art Nouveau architecture, Aksla viewpoint, Atlantic Sea Park",
                    AverageStopDurationMinutes = 90
                },
                new EnhancedPort
                {
                    Id = 3,
                    Name = "Trondheim",
                    UNLocode = "NOTRD",
                    Latitude = 63.4305M,
                    Longitude = 10.3951M,
                    Region = "Central Norway",
                    HasPassengerFacilities = true,
                    HasCargoFacilities = true,
                    HasFuelStation = true,
                    HasMaintenance = true,
                    MaxVesselLengthM = 180M,
                    MaxDraftM = 7M,
                    IsNorthernLightsPort = false,
                    IsMidnightSunPort = false,
                    Attractions = "Nidaros Cathedral, Bakklandet, Kristiansten Fortress",
                    AverageStopDurationMinutes = 120
                },
                new EnhancedPort
                {
                    Id = 4,
                    Name = "Bodø",
                    UNLocode = "NOBOO",
                    Latitude = 67.2804M,
                    Longitude = 14.4040M,
                    Region = "Northern Norway",
                    HasPassengerFacilities = true,
                    HasCargoFacilities = true,
                    HasFuelStation = true,
                    HasMaintenance = false,
                    MaxVesselLengthM = 150M,
                    MaxDraftM = 6M,
                    IsNorthernLightsPort = true,
                    IsMidnightSunPort = true,
                    Attractions = "Saltstraumen maelstrom, Norwegian Aviation Museum, Kjerringøy",
                    AverageStopDurationMinutes = 90
                },
                new EnhancedPort
                {
                    Id = 5,
                    Name = "Tromsø",
                    UNLocode = "NOTOS",
                    Latitude = 69.6496M,
                    Longitude = 18.9560M,
                    Region = "Northern Norway",
                    HasPassengerFacilities = true,
                    HasCargoFacilities = true,
                    HasFuelStation = true,
                    HasMaintenance = true,
                    MaxVesselLengthM = 160M,
                    MaxDraftM = 7M,
                    IsNorthernLightsPort = true,
                    IsMidnightSunPort = true,
                    Attractions = "Arctic Cathedral, Polaria, Cable car, Northern Lights Observatory",
                    AverageStopDurationMinutes = 180
                },
                new EnhancedPort
                {
                    Id = 6,
                    Name = "Hammerfest",
                    UNLocode = "NOHFT",
                    Latitude = 70.6634M,
                    Longitude = 23.6821M,
                    Region = "Northern Norway",
                    HasPassengerFacilities = true,
                    HasCargoFacilities = true,
                    HasFuelStation = true,
                    HasMaintenance = false,
                    MaxVesselLengthM = 120M,
                    MaxDraftM = 5M,
                    IsNorthernLightsPort = true,
                    IsMidnightSunPort = true,
                    Attractions = "Royal and Ancient Polar Bear Society, Sami culture, Arctic nature",
                    AverageStopDurationMinutes = 60
                },
                new EnhancedPort
                {
                    Id = 7,
                    Name = "Kirkenes",
                    UNLocode = "NOKRK",
                    Latitude = 69.7258M,
                    Longitude = 30.0426M,
                    Region = "Northern Norway",
                    HasPassengerFacilities = true,
                    HasCargoFacilities = true,
                    HasFuelStation = true,
                    HasMaintenance = false,
                    MaxVesselLengthM = 140M,
                    MaxDraftM = 6M,
                    IsNorthernLightsPort = true,
                    IsMidnightSunPort = true,
                    Attractions = "Russian border, Snowhotel, King crab safari, Grenselandmuseet",
                    AverageStopDurationMinutes = 90
                }
            };
        }

        /// <summary>
        /// Bergen-Kirkenes route definition
        /// </summary>
        public static List<MaritimeRoute> GetHavilaRoutes()
        {
            return new List<MaritimeRoute>
            {
                new MaritimeRoute
                {
                    Id = 1,
                    Name = "Bergen-Kirkenes (Northbound)",
                    Description = "Classic Norwegian coastal voyage from Bergen to Kirkenes, covering 2500km through 34 ports",
                    EstimatedDuration = TimeSpan.FromHours(134), // 5.5 days
                    DistanceNauticalMiles = 1350,
                    IsActive = true,
                    Type = RouteType.CoastalRoute
                },
                new MaritimeRoute
                {
                    Id = 2,
                    Name = "Kirkenes-Bergen (Southbound)",
                    Description = "Return journey from Kirkenes to Bergen, experiencing the Norwegian coast from a different perspective",
                    EstimatedDuration = TimeSpan.FromHours(134), // 5.5 days
                    DistanceNauticalMiles = 1350,
                    IsActive = true,
                    Type = RouteType.CoastalRoute
                }
            };
        }

        /// <summary>
        /// Sample voyage data for demonstration
        /// </summary>
        public static List<Voyage> GetSampleVoyages()
        {
            var baseDate = DateTime.UtcNow.Date;
            return new List<Voyage>
            {
                new Voyage
                {
                    Id = 1,
                    VesselId = 1, // Havila Capella
                    RouteId = 1, // Bergen-Kirkenes Northbound
                    DepartureTime = baseDate.AddDays(1).AddHours(20).AddMinutes(30), // Tomorrow 20:30
                    DeparturePort = "Bergen",
                    ArrivalPort = "Kirkenes",
                    PassengerCount = 487,
                    CrewCount = 52,
                    FuelConsumptionLiters = 45000M,
                    DistanceCoveredNM = 1350M,
                    Status = VoyageStatus.Scheduled
                },
                new Voyage
                {
                    Id = 2,
                    VesselId = 2, // Havila Castor
                    RouteId = 2, // Kirkenes-Bergen Southbound
                    DepartureTime = baseDate.AddDays(2).AddHours(12).AddMinutes(30), // Day after tomorrow 12:30
                    DeparturePort = "Kirkenes",
                    ArrivalPort = "Bergen",
                    PassengerCount = 521,
                    CrewCount = 54,
                    FuelConsumptionLiters = 47000M,
                    DistanceCoveredNM = 1350M,
                    Status = VoyageStatus.Scheduled
                }
            };
        }

        /// <summary>
        /// Sample environmental data for vessels
        /// </summary>
        public static List<EnvironmentalData> GetSampleEnvironmentalData()
        {
            var now = DateTime.UtcNow;
            var data = new List<EnvironmentalData>();

            static decimal Wave(decimal @base, decimal amplitude, int index)
            {
                var radians = (index % 24) * Math.PI / 12;
                return Math.Round(@base + amplitude * (decimal)Math.Sin(radians), 2);
            }

            for (int vesselId = 1; vesselId <= 4; vesselId++)
            {
                for (int hour = 0; hour < 24; hour++)
                {
                    var index = vesselId * 100 + hour;

                    data.Add(new EnvironmentalData
                    {
                        VesselId = vesselId,
                        MeasurementTime = now.AddHours(-hour),
                        CO2EmissionKg = Wave(175M, 18M, index),
                        FuelConsumptionLiters = Wave(2000M, 150M, index + 3),
                        PowerConsumptionKWh = Wave(2700M, 180M, index + 6),
                        BatteryStateOfCharge = Wave(82M, 8M, index + 9),
                        WaterTemperature = Wave(9.5M, 1.2M, index + 12),
                        AirTemperature = Wave(7.5M, 3.5M, index + 15),
                        WindSpeedKnots = Wave(16M, 6M, index + 18),
                        WaveHeightMeters = Wave(2.1M, 0.8M, index + 21),
                        ComplianceStatus = true
                    });
                }
            }

            return data;
        }

        /// <summary>
        /// AI Models for maritime operations
        /// </summary>
        public static List<AIModel> GetMaritimeAIModels()
        {
            return new List<AIModel>
            {
                new AIModel
                {
                    Id = 1,
                    ModelName = "RouteOptimizer_V2.1",
                    ModelType = "RouteOptimization",
                    Version = "2.1",
                    TrainingDate = DateTime.UtcNow.AddDays(-30),
                    Accuracy = 0.94M,
                    ModelPath = "/models/route-optimization/v2.1/model.pkl",
                    Description = "Advanced route optimization model considering weather, fuel efficiency, and passenger comfort",
                    IsActive = true,
                    CreatedBy = "Maritime AI Team",
                    ProcessingTimeMs = 250M,
                    UsageCount = 1847,
                    ConfigurationJson = "{\"learning_rate\": 0.001, \"epochs\": 100, \"batch_size\": 32}"
                },
                new AIModel
                {
                    Id = 2,
                    ModelName = "WeatherPredictor_Arctic",
                    ModelType = "WeatherPrediction",
                    Version = "3.0",
                    TrainingDate = DateTime.UtcNow.AddDays(-15),
                    Accuracy = 0.89M,
                    ModelPath = "/models/weather/arctic/v3.0/model.pkl",
                    Description = "Specialized weather prediction model for Arctic and Norwegian coastal waters",
                    IsActive = true,
                    CreatedBy = "Climate Data Team",
                    ProcessingTimeMs = 180M,
                    UsageCount = 3241,
                    ConfigurationJson = "{\"time_horizon\": 72, \"spatial_resolution\": \"5km\", \"features\": [\"temperature\", \"wind\", \"waves\"]}"
                },
                new AIModel
                {
                    Id = 3,
                    ModelName = "FuelOptimizer_Hybrid",
                    ModelType = "FuelOptimization",
                    Version = "1.5",
                    TrainingDate = DateTime.UtcNow.AddDays(-45),
                    Accuracy = 0.92M,
                    ModelPath = "/models/fuel/hybrid/v1.5/model.pkl",
                    Description = "Hybrid vessel fuel and battery optimization for maximum efficiency and environmental compliance",
                    IsActive = true,
                    CreatedBy = "Green Maritime Team",
                    ProcessingTimeMs = 320M,
                    UsageCount = 892,
                    ConfigurationJson = "{\"battery_threshold\": 0.2, \"fuel_types\": [\"diesel\", \"battery\"], \"optimization_target\": \"emissions\"}"
                }
            };
        }

        /// <summary>
        /// Sample real-time vessel positions
        /// </summary>
        public static List<VesselPositionData> GetCurrentVesselPositions()
        {
            return new List<VesselPositionData>
            {
                new VesselPositionData
                {
                    Id = 1,
                    VesselId = 1, // Havila Capella
                    Latitude = 62.4722M, // Near Ålesund
                    Longitude = 6.1492M,
                    SpeedKnots = 16.5M,
                    HeadingDegrees = 045M,
                    Timestamp = DateTime.UtcNow,
                    CurrentPort = "Ålesund",
                    NextPort = "Trondheim",
                    EstimatedArrival = DateTime.UtcNow.AddHours(8),
                    WindSpeedKnots = 12M,
                    WaveHeightM = 2.1M,
                    VisibilityKm = 8.5M,
                    WeatherCondition = "Partly cloudy",
                    MMSINumber = "257123456",
                    NavigationStatus = "Under way",
                    IsEmergency = false
                },
                new VesselPositionData
                {
                    Id = 2,
                    VesselId = 2, // Havila Castor
                    Latitude = 69.6496M, // Near Tromsø
                    Longitude = 18.9560M,
                    SpeedKnots = 14.2M,
                    HeadingDegrees = 095M,
                    Timestamp = DateTime.UtcNow,
                    CurrentPort = "Tromsø",
                    NextPort = "Hammerfest",
                    EstimatedArrival = DateTime.UtcNow.AddHours(6),
                    WindSpeedKnots = 18M,
                    WaveHeightM = 3.2M,
                    VisibilityKm = 12M,
                    WeatherCondition = "Clear sky, Northern Lights visible",
                    MMSINumber = "257123457",
                    NavigationStatus = "Under way",
                    IsEmergency = false
                }
            };
        }
    }
}