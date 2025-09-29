-- MaritimeIQ Platform Advanced Stream Analytics
-- Complex Event Processing with ML Integration and Real-time Data Quality Monitoring
-- Demonstrates industry-ready streaming analytics capabilities

-- =============================================================================
-- 1. ADVANCED AIS DATA PROCESSING WITH GEOSPATIAL ANALYTICS
-- =============================================================================

WITH AISDataEnriched AS (
    SELECT 
        VesselId,
        VesselName,
        MMSINumber,
        CAST(Latitude AS float) AS Lat,
        CAST(Longitude AS float) AS Lon,
        CAST(Speed AS float) AS Speed,
        CAST(Heading AS float) AS Heading,
        NavigationStatus,
        EventEnqueuedUtcTime AS SourceTimestamp,
        System.Timestamp() AS ProcessingTime,
        -- Geospatial calculations
        ABS(LAG(CAST(Latitude AS float)) OVER (PARTITION BY VesselId ORDER BY EventEnqueuedUtcTime) - CAST(Latitude AS float)) AS LatDelta,
        ABS(LAG(CAST(Longitude AS float)) OVER (PARTITION BY VesselId ORDER BY EventEnqueuedUtcTime) - CAST(Longitude AS float)) AS LonDelta,
        -- Data quality indicators
        CASE 
            WHEN Latitude BETWEEN 58.0 AND 71.0 AND Longitude BETWEEN 4.0 AND 31.0 THEN 1  -- Norwegian waters
            ELSE 0 
        END AS InExpectedRegion,
        CASE 
            WHEN Speed > 0 AND Speed < 40 THEN 1  -- Reasonable speed range
            ELSE 0 
        END AS ValidSpeed
    FROM AISInputStream TIMESTAMP BY EventEnqueuedUtcTime
    WHERE VesselId IS NOT NULL 
      AND Latitude IS NOT NULL 
      AND Longitude IS NOT NULL
      AND Latitude BETWEEN -90 AND 90 
      AND Longitude BETWEEN -180 AND 180
),

-- =============================================================================
-- 2. REAL-TIME VESSEL PERFORMANCE ANALYTICS WITH SLIDING WINDOWS
-- =============================================================================

VesselPerformanceMetrics AS (
    SELECT 
        VesselId,
        VesselName,
        -- Performance KPIs with multiple time windows
        AVG(Speed) AS AvgSpeed_5min,
        MAX(Speed) AS MaxSpeed_5min,
        STDEV(Speed) AS SpeedVariability_5min,
        COUNT(*) AS DataPoints_5min,
        -- Fuel efficiency proxy (speed consistency indicates efficiency)
        (MAX(Speed) - MIN(Speed)) / NULLIF(AVG(Speed), 0) AS SpeedInconsistencyRatio,
        -- Route deviation detection using statistical analysis
        CASE 
            WHEN STDEV(LatDelta) > 0.01 OR STDEV(LonDelta) > 0.01 THEN 'HIGH_DEVIATION'
            WHEN STDEV(LatDelta) > 0.005 OR STDEV(LonDelta) > 0.005 THEN 'MODERATE_DEVIATION'
            ELSE 'ON_COURSE'
        END AS RouteStabilityStatus,
        -- Data quality score
        AVG(CAST(InExpectedRegion AS float)) AS LocationQualityScore,
        AVG(CAST(ValidSpeed AS float)) AS SpeedQualityScore,
        System.Timestamp() AS WindowEnd
    FROM AISDataEnriched
    GROUP BY VesselId, VesselName, TumblingWindow(minute, 5)
),

-- =============================================================================
-- 3. ADVANCED ENVIRONMENTAL MONITORING WITH ANOMALY DETECTION
-- =============================================================================

EnvironmentalAnomalyDetection AS (
    SELECT 
        e.VesselId,
        e.VesselName,
        e.CO2EmissionKg,
        e.FuelConsumptionLiters,
        e.BatteryStateOfCharge,
        e.WaterTemperature,
        e.AirTemperature,
        -- Moving averages for anomaly detection
        AVG(e.CO2EmissionKg) OVER (PARTITION BY e.VesselId ORDER BY EventEnqueuedUtcTime RANGE INTERVAL '1' HOUR PRECEDING) AS CO2_MovingAvg_1h,
        AVG(e.FuelConsumptionLiters) OVER (PARTITION BY e.VesselId ORDER BY EventEnqueuedUtcTime RANGE INTERVAL '1' HOUR PRECEDING) AS Fuel_MovingAvg_1h,
        -- Z-score calculation for anomaly detection
        (e.CO2EmissionKg - AVG(e.CO2EmissionKg) OVER (PARTITION BY e.VesselId ORDER BY EventEnqueuedUtcTime RANGE INTERVAL '24' HOUR PRECEDING)) / 
        NULLIF(STDEV(e.CO2EmissionKg) OVER (PARTITION BY e.VesselId ORDER BY EventEnqueuedUtcTime RANGE INTERVAL '24' HOUR PRECEDING), 0) AS CO2_ZScore,
        -- Complex alert classification using ML-style scoring
        CASE 
            WHEN e.CO2EmissionKg > 55.0 OR e.FuelConsumptionLiters > 1200.0 THEN 'CRITICAL_ENVIRONMENTAL_ALERT'
            WHEN e.CO2EmissionKg > 45.0 OR e.FuelConsumptionLiters > 1000.0 THEN 'HIGH_ENVIRONMENTAL_ALERT'
            WHEN e.BatteryStateOfCharge < 20.0 THEN 'LOW_BATTERY_ALERT'
            WHEN ABS(e.CO2EmissionKg - AVG(e.CO2EmissionKg) OVER (PARTITION BY e.VesselId ORDER BY EventEnqueuedUtcTime RANGE INTERVAL '1' HOUR PRECEDING)) > 15.0 THEN 'ANOMALY_DETECTED'
            ELSE 'NORMAL'
        END AS EnvironmentalAlertLevel,
        System.Timestamp() AS ProcessingTime
    FROM EnvironmentalInputStream e TIMESTAMP BY EventEnqueuedUtcTime
    WHERE e.VesselId IS NOT NULL
),

-- =============================================================================
-- 4. COMPLEX EVENT PROCESSING - VESSEL INTERACTION ANALYTICS
-- =============================================================================

VesselProximityAnalysis AS (
    SELECT 
        a1.VesselId AS Vessel1Id,
        a1.VesselName AS Vessel1Name,
        a2.VesselId AS Vessel2Id,
        a2.VesselName AS Vessel2Name,
        -- Simplified distance calculation (Haversine formula approximation)
        SQRT(POWER(a1.Lat - a2.Lat, 2) + POWER(a1.Lon - a2.Lon, 2)) * 111.0 AS ApproximateDistanceKm,
        -- Collision risk assessment
        CASE 
            WHEN SQRT(POWER(a1.Lat - a2.Lat, 2) + POWER(a1.Lon - a2.Lon, 2)) * 111.0 < 2.0 AND 
                 ABS(a1.Speed - a2.Speed) < 3.0 AND
                 ABS(a1.Heading - a2.Heading) < 30.0
            THEN 'HIGH_COLLISION_RISK'
            WHEN SQRT(POWER(a1.Lat - a2.Lat, 2) + POWER(a1.Lon - a2.Lon, 2)) * 111.0 < 5.0
            THEN 'PROXIMITY_ALERT'
            ELSE 'SAFE_DISTANCE'
        END AS CollisionRiskLevel,
        System.Timestamp() AS AnalysisTime
    FROM AISDataEnriched a1 TIMESTAMP BY SourceTimestamp
    JOIN AISDataEnriched a2 TIMESTAMP BY SourceTimestamp
        ON DATEDIFF(second, a1, a2) BETWEEN -30 AND 30  -- 30-second time window
        AND a1.VesselId != a2.VesselId
    WHERE a1.Speed > 5.0 AND a2.Speed > 5.0  -- Only analyze moving vessels
),

-- =============================================================================
-- 5. PREDICTIVE ANALYTICS WITH REAL-TIME ML SCORING
-- =============================================================================

PredictiveMaintenanceScoring AS (
    SELECT 
        e.VesselId,
        e.VesselName,
        -- Feature engineering for ML model
        e.FuelConsumptionLiters AS Current_FuelConsumption,
        AVG(e.FuelConsumptionLiters) OVER (PARTITION BY e.VesselId ORDER BY EventEnqueuedUtcTime RANGE INTERVAL '24' HOUR PRECEDING) AS Avg_FuelConsumption_24h,
        e.BatteryStateOfCharge AS Current_BatterySOC,
        AVG(e.BatteryStateOfCharge) OVER (PARTITION BY e.VesselId ORDER BY EventEnqueuedUtcTime RANGE INTERVAL '24' HOUR PRECEDING) AS Avg_BatterySOC_24h,
        -- Simple maintenance prediction score (0-100)
        CASE 
            WHEN e.BatteryStateOfCharge < 30.0 OR e.FuelConsumptionLiters > AVG(e.FuelConsumptionLiters) OVER (PARTITION BY e.VesselId ORDER BY EventEnqueuedUtcTime RANGE INTERVAL '7' DAY PRECEDING) * 1.2
            THEN 25.0  -- High maintenance risk
            WHEN e.BatteryStateOfCharge < 50.0 OR e.FuelConsumptionLiters > AVG(e.FuelConsumptionLiters) OVER (PARTITION BY e.VesselId ORDER BY EventEnqueuedUtcTime RANGE INTERVAL '7' DAY PRECEDING) * 1.1
            THEN 50.0  -- Medium maintenance risk
            ELSE 85.0  -- Low maintenance risk
        END AS MaintenancePredictionScore,
        System.Timestamp() AS ScoringTime
    FROM EnvironmentalInputStream e TIMESTAMP BY EventEnqueuedUtcTime
    WHERE e.VesselId IS NOT NULL
),

-- =============================================================================
-- 6. REAL-TIME DATA QUALITY MONITORING
-- =============================================================================

DataQualityMetrics AS (
    SELECT 
        'AIS_Data' AS DataSource,
        COUNT(*) AS TotalRecords,
        COUNT(CASE WHEN InExpectedRegion = 1 THEN 1 END) AS ValidLocationRecords,
        COUNT(CASE WHEN ValidSpeed = 1 THEN 1 END) AS ValidSpeedRecords,
        CAST(COUNT(CASE WHEN InExpectedRegion = 1 THEN 1 END) AS float) / CAST(COUNT(*) AS float) AS LocationQualityPercentage,
        CAST(COUNT(CASE WHEN ValidSpeed = 1 THEN 1 END) AS float) / CAST(COUNT(*) AS float) AS SpeedQualityPercentage,
        System.Timestamp() AS QualityCheckTime
    FROM AISDataEnriched
    GROUP BY TumblingWindow(minute, 5)
    
    UNION ALL
    
    SELECT 
        'Environmental_Data' AS DataSource,
        COUNT(*) AS TotalRecords,
        COUNT(CASE WHEN CO2EmissionKg IS NOT NULL AND CO2EmissionKg >= 0 THEN 1 END) AS ValidCO2Records,
        COUNT(CASE WHEN FuelConsumptionLiters IS NOT NULL AND FuelConsumptionLiters >= 0 THEN 1 END) AS ValidFuelRecords,
        CAST(COUNT(CASE WHEN CO2EmissionKg IS NOT NULL AND CO2EmissionKg >= 0 THEN 1 END) AS float) / CAST(COUNT(*) AS float) AS CO2QualityPercentage,
        CAST(COUNT(CASE WHEN FuelConsumptionLiters IS NOT NULL AND FuelConsumptionLiters >= 0 THEN 1 END) AS float) / CAST(COUNT(*) AS float) AS FuelQualityPercentage,
        System.Timestamp() AS QualityCheckTime
    FROM EnvironmentalInputStream TIMESTAMP BY EventEnqueuedUtcTime
    GROUP BY TumblingWindow(minute, 5)
)

-- =============================================================================
-- OUTPUT STREAMS TO DIFFERENT SINKS FOR MULTI-LAYER ARCHITECTURE
-- =============================================================================

-- Real-time vessel performance for operational dashboards
SELECT 
    VesselId,
    VesselName,
    AvgSpeed_5min,
    MaxSpeed_5min,
    SpeedVariability_5min,
    RouteStabilityStatus,
    LocationQualityScore,
    SpeedQualityScore,
    WindowEnd
INTO VesselPerformanceOutputStream
FROM VesselPerformanceMetrics
WHERE LocationQualityScore > 0.8;  -- Only high-quality data

-- Environmental alerts for immediate action
SELECT 
    VesselId,
    VesselName,
    EnvironmentalAlertLevel,
    CO2EmissionKg,
    FuelConsumptionLiters,
    BatteryStateOfCharge,
    CO2_ZScore,
    ProcessingTime
INTO EnvironmentalAlertsOutputStream
FROM EnvironmentalAnomalyDetection
WHERE EnvironmentalAlertLevel != 'NORMAL';

-- Collision risk alerts for safety systems
SELECT 
    Vessel1Id,
    Vessel1Name,
    Vessel2Id,
    Vessel2Name,
    ApproximateDistanceKm,
    CollisionRiskLevel,
    AnalysisTime
INTO CollisionRiskOutputStream
FROM VesselProximityAnalysis
WHERE CollisionRiskLevel IN ('HIGH_COLLISION_RISK', 'PROXIMITY_ALERT');

-- Predictive maintenance alerts for fleet management
SELECT 
    VesselId,
    VesselName,
    MaintenancePredictionScore,
    Current_FuelConsumption,
    Avg_FuelConsumption_24h,
    Current_BatterySOC,
    Avg_BatterySOC_24h,
    ScoringTime
INTO PredictiveMaintenanceOutputStream
FROM PredictiveMaintenanceScoring
WHERE MaintenancePredictionScore < 60.0;  -- Alert when maintenance risk is high

-- Data quality metrics for monitoring dashboards
SELECT 
    DataSource,
    TotalRecords,
    LocationQualityPercentage,
    SpeedQualityPercentage,
    CASE 
        WHEN LocationQualityPercentage < 0.95 OR SpeedQualityPercentage < 0.95 THEN 'QUALITY_ISSUE'
        WHEN LocationQualityPercentage < 0.98 OR SpeedQualityPercentage < 0.98 THEN 'QUALITY_WARNING'
        ELSE 'QUALITY_GOOD'
    END AS OverallQualityStatus,
    QualityCheckTime
INTO DataQualityOutputStream
FROM DataQualityMetrics;

-- =============================================================================
-- 7. ADVANCED AGGREGATIONS FOR LAKEHOUSE SILVER LAYER
-- =============================================================================

-- Hourly vessel operational summary with advanced metrics
SELECT 
    VesselId,
    VesselName,
    -- Operational efficiency metrics
    AVG(Speed) AS AvgSpeed_1h,
    STDEV(Speed) AS SpeedConsistency_1h,
    MAX(Speed) AS PeakSpeed_1h,
    -- Distance and navigation metrics
    SUM(CASE WHEN Speed > 0 THEN Speed * 5.0 / 60.0 ELSE 0 END) AS EstimatedDistanceNM_1h,  -- Approximate distance in nautical miles
    COUNT(DISTINCT CONCAT(ROUND(Lat, 2), ',', ROUND(Lon, 2))) AS UniquePositions_1h,
    -- Route adherence scoring
    AVG(CAST(InExpectedRegion AS float)) * 100 AS RouteAdherenceScore_1h,
    -- Data completeness and quality
    COUNT(*) AS TotalDataPoints_1h,
    AVG(LocationQualityScore) * 100 AS DataQualityScore_1h,
    System.Timestamp() AS AggregationTime
INTO VesselOperationalSummaryOutputStream
FROM AISDataEnriched
GROUP BY VesselId, VesselName, TumblingWindow(hour, 1);

-- =============================================================================
-- 8. FLEET-WIDE ANALYTICS FOR STRATEGIC INSIGHTS
-- =============================================================================

-- Fleet performance comparison and benchmarking
SELECT 
    'FleetWide' AS AggregationLevel,
    COUNT(DISTINCT VesselId) AS ActiveVessels,
    AVG(Speed) AS FleetAvgSpeed,
    MAX(Speed) AS FleetMaxSpeed,
    STDEV(Speed) AS FleetSpeedVariability,
    -- Fleet efficiency indicators
    AVG(CASE WHEN Speed > 10 THEN Speed ELSE NULL END) AS AvgCruisingSpeed,
    COUNT(CASE WHEN Speed = 0 THEN 1 END) AS VesselsStationary,
    COUNT(CASE WHEN Speed > 20 THEN 1 END) AS VesselsHighSpeed,
    -- Quality and reliability metrics
    AVG(LocationQualityScore) * 100 AS FleetDataQuality,
    COUNT(CASE WHEN RouteStabilityStatus = 'HIGH_DEVIATION' THEN 1 END) AS VesselsOffCourse,
    System.Timestamp() AS FleetAnalysisTime
INTO FleetWideAnalyticsOutputStream
FROM VesselPerformanceMetrics
GROUP BY TumblingWindow(hour, 1);

-- =============================================================================
-- 9. REAL-TIME ML FEATURE STORE UPDATES
-- =============================================================================

-- Real-time features for ML models (route optimization, fuel prediction)
SELECT 
    a.VesselId,
    a.VesselName,
    -- Temporal features
    DATEPART(hour, System.Timestamp()) AS HourOfDay,
    DATEPART(weekday, System.Timestamp()) AS DayOfWeek,
    -- Operational features
    a.Speed AS CurrentSpeed,
    AVG(a.Speed) OVER (PARTITION BY a.VesselId ORDER BY a.SourceTimestamp RANGE INTERVAL '30' MINUTE PRECEDING) AS Speed_30min_avg,
    LAG(a.Speed, 1) OVER (PARTITION BY a.VesselId ORDER BY a.SourceTimestamp) AS Speed_Previous,
    -- Environmental context features
    e.WaterTemperature,
    e.AirTemperature,
    e.BatteryStateOfCharge,
    -- Performance ratios
    CASE WHEN AVG(a.Speed) OVER (PARTITION BY a.VesselId ORDER BY a.SourceTimestamp RANGE INTERVAL '1' HOUR PRECEDING) > 0
         THEN a.Speed / AVG(a.Speed) OVER (PARTITION BY a.VesselId ORDER BY a.SourceTimestamp RANGE INTERVAL '1' HOUR PRECEDING)
         ELSE 1.0 
    END AS SpeedPerformanceRatio,
    System.Timestamp() AS FeatureTimestamp
INTO MLFeatureStoreOutputStream
FROM AISDataEnriched a TIMESTAMP BY SourceTimestamp
LEFT JOIN EnvironmentalInputStream e TIMESTAMP BY EventEnqueuedUtcTime
    ON a.VesselId = e.VesselId AND DATEDIFF(second, a, e) BETWEEN -60 AND 60
WHERE a.Speed IS NOT NULL AND a.Lat IS NOT NULL AND a.Lon IS NOT NULL;