-- Havila Kystruten Stream Analytics Query
-- Processes real-time AIS data from Event Hubs for maritime intelligence

-- AIS Data Stream Processing
WITH AISDataNormalized AS (
    SELECT 
        VesselId,
        VesselName,
        MMSINumber,
        CAST(Latitude AS float) AS Latitude,
        CAST(Longitude AS float) AS Longitude,
        CAST(Speed AS float) AS Speed,
        CAST(Heading AS float) AS Heading,
        NavigationStatus,
        EventEnqueuedUtcTime AS Timestamp,
        System.Timestamp() AS ProcessingTime
    FROM AISInputStream TIMESTAMP BY EventEnqueuedUtcTime
    WHERE VesselId IS NOT NULL 
      AND Latitude BETWEEN -90 AND 90 
      AND Longitude BETWEEN -180 AND 180
),

-- Vessel Speed Analysis
VesselSpeedAnalysis AS (
    SELECT 
        VesselId,
        VesselName,
        AVG(Speed) AS AvgSpeed,
        MAX(Speed) AS MaxSpeed,
        MIN(Speed) AS MinSpeed,
        COUNT(*) AS DataPoints,
        System.Timestamp() AS WindowEnd
    FROM AISDataNormalized
    GROUP BY VesselId, VesselName, TumblingWindow(minute, 5)
),

-- Environmental Compliance Monitoring
EnvironmentalAlerts AS (
    SELECT 
        e.VesselId,
        e.VesselName,
        e.CO2EmissionKg,
        e.FuelConsumptionLiters,
        e.WaterTemperature,
        e.AirTemperature,
        CASE 
            WHEN e.CO2EmissionKg > 50.0 THEN 'HIGH_CO2_EMISSION'
            WHEN e.FuelConsumptionLiters > 1000.0 THEN 'HIGH_FUEL_CONSUMPTION'
            WHEN e.WaterTemperature < 0 THEN 'FREEZING_CONDITIONS'
            ELSE 'NORMAL'
        END AS AlertType,
        System.Timestamp() AS AlertTime
    FROM EnvironmentalInputStream e TIMESTAMP BY EventEnqueuedUtcTime
    WHERE e.CO2EmissionKg > 40.0 OR e.FuelConsumptionLiters > 800.0 OR e.WaterTemperature < 2.0
),

-- Route Deviation Detection
RouteDeviationAlerts AS (
    SELECT 
        a.VesselId,
        a.VesselName,
        a.Latitude,
        a.Longitude,
        -- Simple distance calculation from expected Bergen-Kirkenes route
        CASE 
            WHEN ABS(a.Latitude - 65.0) > 5.0 OR ABS(a.Longitude - 15.0) > 10.0 
            THEN 'ROUTE_DEVIATION'
            ELSE 'ON_ROUTE'
        END AS RouteStatus,
        System.Timestamp() AS DetectionTime
    FROM AISDataNormalized a
    WHERE a.Speed > 5.0 -- Only check when vessel is moving
),

-- Passenger Safety Monitoring
SafetyAlerts AS (
    SELECT 
        VesselId,
        VesselName,
        'EMERGENCY_BROADCAST' AS AlertType,
        NavigationStatus,
        Speed,
        System.Timestamp() AS AlertTime
    FROM AISDataNormalized
    WHERE NavigationStatus IN ('Not under command', 'Restricted manoeuvrability', 'Constrained by her draught')
       OR Speed = 0 -- Vessel stopped unexpectedly
)

-- Output to different sinks
SELECT * INTO VesselSpeedOutput FROM VesselSpeedAnalysis;
SELECT * INTO EnvironmentalAlertsOutput FROM EnvironmentalAlerts;
SELECT * INTO RouteDeviationOutput FROM RouteDeviationAlerts WHERE RouteStatus = 'ROUTE_DEVIATION';
SELECT * INTO SafetyAlertsOutput FROM SafetyAlerts;

-- Aggregate data for Power BI dashboard
SELECT 
    VesselId,
    VesselName,
    AVG(Speed) AS AvgSpeed,
    COUNT(*) AS TotalReadings,
    System.Timestamp() AS WindowEnd
INTO PowerBIDashboardOutput
FROM AISDataNormalized
GROUP BY VesselId, VesselName, TumblingWindow(minute, 15);