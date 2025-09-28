-- Stream Analytics Query for Real-time Maritime Data Processing
-- This query demonstrates advanced real-time analytics for interview showcase

-- Input: Event Hub 'maritime-data'
-- Outputs: SQL Database, Power BI, Blob Storage

-- ==============================================
-- REAL-TIME VESSEL TRACKING & ANALYTICS
-- ==============================================

-- 1. Real-time vessel position updates with anomaly detection
WITH VesselPositions AS (
    SELECT 
        VesselId,
        VesselName,
        Latitude,
        Longitude,
        Speed,
        Heading,
        Timestamp,
        System.Timestamp() AS WindowEnd
    FROM MaritimeDataInput TIMESTAMP BY Timestamp
    WHERE Latitude IS NOT NULL AND Longitude IS NOT NULL
),

-- 2. Detect speed anomalies (vessels going too fast/slow)
SpeedAnomalies AS (
    SELECT 
        VesselId,
        VesselName,
        Speed,
        AVG(Speed) OVER (PARTITION BY VesselId LIMIT DURATION(minute, 10)) AS AvgSpeed,
        STDEV(Speed) OVER (PARTITION BY VesselId LIMIT DURATION(minute, 10)) AS StdDevSpeed,
        Timestamp,
        System.Timestamp() AS WindowEnd
    FROM VesselPositions
),

-- 3. Real-time fleet performance metrics
FleetMetrics AS (
    SELECT 
        COUNT(*) AS ActiveVessels,
        AVG(Speed) AS AverageFleetSpeed,
        MAX(Speed) AS MaxSpeed,
        MIN(Speed) AS MinSpeed,
        System.Timestamp() AS WindowEnd
    FROM VesselPositions
    GROUP BY TumblingWindow(minute, 1)
),

-- 4. Geofencing alerts (vessels entering/leaving specific areas)
GeofenceAlerts AS (
    SELECT 
        VesselId,
        VesselName,
        Latitude,
        Longitude,
        CASE 
            WHEN Latitude BETWEEN 58.0 AND 72.0 AND Longitude BETWEEN 4.0 AND 32.0 
            THEN 'Norwegian Waters'
            WHEN Latitude BETWEEN 54.0 AND 66.0 AND Longitude BETWEEN -12.0 AND 3.0 
            THEN 'North Sea'
            ELSE 'International Waters'
        END AS Zone,
        Timestamp,
        System.Timestamp() AS WindowEnd
    FROM VesselPositions
),

-- 5. Engine performance monitoring
EngineMetrics AS (
    SELECT 
        VesselId,
        VesselName,
        EngineTemp,
        FuelConsumption,
        RPM,
        AVG(EngineTemp) OVER (PARTITION BY VesselId LIMIT DURATION(minute, 5)) AS AvgEngineTemp,
        SUM(FuelConsumption) OVER (PARTITION BY VesselId LIMIT DURATION(hour, 1)) AS HourlyFuelConsumption,
        Timestamp,
        System.Timestamp() AS WindowEnd
    FROM MaritimeDataInput TIMESTAMP BY Timestamp
    WHERE EngineTemp IS NOT NULL
),

-- 6. Predictive maintenance alerts
MaintenanceAlerts AS (
    SELECT 
        VesselId,
        VesselName,
        'HIGH_ENGINE_TEMP' AS AlertType,
        'Engine temperature above normal threshold' AS AlertMessage,
        EngineTemp AS Value,
        System.Timestamp() AS AlertTime
    FROM EngineMetrics
    WHERE EngineTemp > 85.0
    
    UNION ALL
    
    SELECT 
        VesselId,
        VesselName,
        'ABNORMAL_SPEED' AS AlertType,
        'Vessel speed deviation detected' AS AlertMessage,
        Speed AS Value,
        System.Timestamp() AS AlertTime
    FROM SpeedAnomalies
    WHERE ABS(Speed - AvgSpeed) > (2 * StdDevSpeed) AND StdDevSpeed > 0
),

-- 7. Environmental impact monitoring
EnvironmentalMetrics AS (
    SELECT 
        VesselId,
        VesselName,
        CO2Emissions,
        NOxEmissions,
        SulfurContent,
        SUM(CO2Emissions) OVER (PARTITION BY VesselId LIMIT DURATION(day, 1)) AS DailyCO2,
        AVG(NOxEmissions) OVER (LIMIT DURATION(hour, 1)) AS HourlyAvgNOx,
        System.Timestamp() AS WindowEnd
    FROM MaritimeDataInput TIMESTAMP BY Timestamp
    WHERE CO2Emissions IS NOT NULL
)

-- ==============================================
-- OUTPUTS FOR DIFFERENT SYSTEMS
-- ==============================================

-- Output 1: Real-time dashboard data to Power BI
SELECT 
    VesselId,
    VesselName,
    Latitude,
    Longitude,
    Speed,
    Heading,
    WindowEnd
INTO PowerBIOutput
FROM VesselPositions;

-- Output 2: Fleet metrics to SQL Database for historical analysis
SELECT 
    ActiveVessels,
    AverageFleetSpeed,
    MaxSpeed,
    MinSpeed,
    WindowEnd
INTO SQLDatabaseOutput
FROM FleetMetrics;

-- Output 3: Critical alerts to Service Bus for immediate action
SELECT 
    VesselId,
    VesselName,
    AlertType,
    AlertMessage,
    Value,
    AlertTime
INTO ServiceBusOutput
FROM MaintenanceAlerts;

-- Output 4: Environmental data to Blob Storage for compliance reporting
SELECT 
    VesselId,
    VesselName,
    DailyCO2,
    HourlyAvgNOx,
    WindowEnd
INTO BlobStorageOutput
FROM EnvironmentalMetrics;

-- Output 5: Geofence events to Event Hub for downstream processing
SELECT 
    VesselId,
    VesselName,
    Zone,
    Latitude,
    Longitude,
    WindowEnd
INTO EventHubOutput
FROM GeofenceAlerts;

-- ==============================================
-- INTERVIEW DEMO POINTS
-- ==============================================
/*
This Stream Analytics query demonstrates:

1. REAL-TIME PROCESSING:
   - Sub-second latency for vessel tracking
   - Continuous processing of maritime data streams
   - Window-based aggregations for fleet metrics

2. ADVANCED ANALYTICS:
   - Anomaly detection using statistical methods
   - Predictive maintenance alerts
   - Environmental compliance monitoring
   - Geofencing and zone management

3. MULTIPLE OUTPUTS:
   - Power BI for real-time dashboards
   - SQL Database for historical analysis
   - Service Bus for alert management
   - Blob Storage for compliance data
   - Event Hub for event-driven architecture

4. BUSINESS VALUE:
   - Operational efficiency improvement
   - Predictive maintenance cost savings
   - Environmental compliance automation
   - Safety and security monitoring
   - Fleet optimization insights

5. SCALABILITY:
   - Handles thousands of vessels simultaneously
   - Auto-scales based on data volume
   - Fault-tolerant processing
   - Built-in retry mechanisms
*/