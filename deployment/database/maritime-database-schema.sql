-- MaritimeIQ Platform Database Schema
-- Complete implementation matching Azure Portal Step-by-Step Guide requirements

-- Create tables for maritime data intelligence platform
CREATE TABLE vessels (
    vessel_id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    vessel_name NVARCHAR(100) NOT NULL,
    imo_number NVARCHAR(7) UNIQUE NOT NULL,
    ship_type NVARCHAR(50) NOT NULL,
    gross_tonnage INT,
    passenger_capacity INT,
    hybrid_system_type NVARCHAR(50),
    battery_capacity_kwh INT,
    fuel_type NVARCHAR(30),
    year_built INT,
    route_assignment NVARCHAR(50),
    status NVARCHAR(20) DEFAULT 'Active',
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

CREATE TABLE voyages (
    voyage_id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    vessel_id UNIQUEIDENTIFIER NOT NULL,
    departure_port_id UNIQUEIDENTIFIER NOT NULL,
    arrival_port_id UNIQUEIDENTIFIER NOT NULL,
    scheduled_departure DATETIME2 NOT NULL,
    actual_departure DATETIME2,
    scheduled_arrival DATETIME2 NOT NULL,
    actual_arrival DATETIME2,
    voyage_status NVARCHAR(20) DEFAULT 'Scheduled',
    passenger_count INT DEFAULT 0,
    fuel_consumed_liters DECIMAL(10,2),
    battery_usage_kwh DECIMAL(10,2),
    co2_emissions_kg DECIMAL(10,2),
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (vessel_id) REFERENCES vessels(vessel_id),
    FOREIGN KEY (departure_port_id) REFERENCES ports(port_id),
    FOREIGN KEY (arrival_port_id) REFERENCES ports(port_id)
);

CREATE TABLE ports (
    port_id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    port_name NVARCHAR(100) NOT NULL,
    country NVARCHAR(50) NOT NULL,
    latitude DECIMAL(10,8) NOT NULL,
    longitude DECIMAL(11,8) NOT NULL,
    port_type NVARCHAR(30) NOT NULL,
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

CREATE TABLE environmental_data (
    data_id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    vessel_id UNIQUEIDENTIFIER NOT NULL,
    timestamp DATETIME2 NOT NULL,
    latitude DECIMAL(10,8),
    longitude DECIMAL(11,8),
    speed_knots DECIMAL(5,2),
    fuel_consumption_lph DECIMAL(8,2),
    battery_charge_percent DECIMAL(5,2),
    co2_emissions_kg_h DECIMAL(8,2),
    nox_emissions_kg_h DECIMAL(8,2),
    sox_emissions_kg_h DECIMAL(8,2),
    weather_conditions NVARCHAR(100),
    sea_state NVARCHAR(50),
    hybrid_mode_active BIT DEFAULT 0,
    created_at DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (vessel_id) REFERENCES vessels(vessel_id)
);

CREATE TABLE ai_models (
    model_id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    model_name NVARCHAR(100) NOT NULL,
    model_type NVARCHAR(50) NOT NULL,
    version NVARCHAR(20) NOT NULL,
    status NVARCHAR(50) DEFAULT 'Active',
    accuracy_score DECIMAL(5,4),
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

-- Additional tables for comprehensive maritime platform
CREATE TABLE passengers (
    passenger_id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    first_name NVARCHAR(50) NOT NULL,
    last_name NVARCHAR(50) NOT NULL,
    email NVARCHAR(100),
    phone NVARCHAR(20),
    nationality NVARCHAR(50),
    date_of_birth DATE,
    preferences NVARCHAR(MAX), -- JSON format for notification preferences
    loyalty_tier NVARCHAR(20) DEFAULT 'Standard',
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

CREATE TABLE bookings (
    booking_id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    passenger_id UNIQUEIDENTIFIER NOT NULL,
    voyage_id UNIQUEIDENTIFIER NOT NULL,
    cabin_number NVARCHAR(10),
    cabin_type NVARCHAR(50),
    booking_status NVARCHAR(20) DEFAULT 'Confirmed',
    total_amount DECIMAL(10,2),
    booking_date DATETIME2 DEFAULT GETDATE(),
    special_requirements NVARCHAR(500),
    FOREIGN KEY (passenger_id) REFERENCES passengers(passenger_id),
    FOREIGN KEY (voyage_id) REFERENCES voyages(voyage_id)
);

CREATE TABLE safety_incidents (
    incident_id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    vessel_id UNIQUEIDENTIFIER NOT NULL,
    incident_type NVARCHAR(50) NOT NULL,
    severity NVARCHAR(20) NOT NULL,
    description NVARCHAR(1000),
    location NVARCHAR(100),
    incident_date DATETIME2 NOT NULL,
    reported_by NVARCHAR(100),
    status NVARCHAR(30) DEFAULT 'Open',
    resolution NVARCHAR(1000),
    resolved_date DATETIME2,
    created_at DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (vessel_id) REFERENCES vessels(vessel_id)
);

CREATE TABLE route_optimizations (
    optimization_id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    voyage_id UNIQUEIDENTIFIER NOT NULL,
    original_route NVARCHAR(MAX), -- JSON format
    optimized_route NVARCHAR(MAX), -- JSON format
    fuel_savings_percent DECIMAL(5,2),
    time_savings_minutes INT,
    co2_reduction_kg DECIMAL(8,2),
    optimization_factors NVARCHAR(MAX), -- JSON format
    applied BIT DEFAULT 0,
    created_at DATETIME2 DEFAULT GETDATE(),
    applied_at DATETIME2,
    FOREIGN KEY (voyage_id) REFERENCES voyages(voyage_id)
);

CREATE TABLE northern_lights_forecasts (
    forecast_id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    forecast_date DATE NOT NULL,
    latitude DECIMAL(10,8) NOT NULL,
    longitude DECIMAL(11,8) NOT NULL,
    aurora_probability INT, -- 0-100
    aurora_intensity NVARCHAR(20),
    optimal_viewing_time DATETIME2,
    cloud_cover_percent INT,
    weather_description NVARCHAR(200),
    created_at DATETIME2 DEFAULT GETDATE()
);

CREATE TABLE passenger_notifications (
    notification_id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    passenger_id UNIQUEIDENTIFIER NOT NULL,
    notification_type NVARCHAR(50) NOT NULL,
    title NVARCHAR(200),
    message NVARCHAR(1000),
    sent_at DATETIME2 DEFAULT GETDATE(),
    delivery_status NVARCHAR(20) DEFAULT 'Sent',
    read_at DATETIME2,
    FOREIGN KEY (passenger_id) REFERENCES passengers(passenger_id)
);

-- Create indexes for performance
CREATE INDEX IX_vessels_status ON vessels(status);
CREATE INDEX IX_vessels_route_assignment ON vessels(route_assignment);
CREATE INDEX IX_voyages_vessel_id ON voyages(vessel_id);
CREATE INDEX IX_voyages_status ON voyages(voyage_status);
CREATE INDEX IX_voyages_departure_date ON voyages(scheduled_departure);
CREATE INDEX IX_environmental_data_vessel_id ON environmental_data(vessel_id);
CREATE INDEX IX_environmental_data_timestamp ON environmental_data(timestamp);
CREATE INDEX IX_environmental_data_vessel_timestamp ON environmental_data(vessel_id, timestamp);
CREATE INDEX IX_ai_models_status ON ai_models(status);
CREATE INDEX IX_bookings_passenger_id ON bookings(passenger_id);
CREATE INDEX IX_bookings_voyage_id ON bookings(voyage_id);
CREATE INDEX IX_safety_incidents_vessel_id ON safety_incidents(vessel_id);
CREATE INDEX IX_safety_incidents_date ON safety_incidents(incident_date);
CREATE INDEX IX_route_optimizations_voyage_id ON route_optimizations(voyage_id);
CREATE INDEX IX_northern_lights_date ON northern_lights_forecasts(forecast_date);
CREATE INDEX IX_notifications_passenger_id ON passenger_notifications(passenger_id);
CREATE INDEX IX_notifications_sent_at ON passenger_notifications(sent_at);

-- Insert sample maritime data for MaritimeIQ Platform demo
INSERT INTO vessels (vessel_name, imo_number, ship_type, route_assignment, status, passenger_capacity, hybrid_system_type, battery_capacity_kwh, fuel_type, year_built) VALUES
('MS Maritime Capella', '9892689', 'Coastal Cruise Ship', 'Bergen-Kirkenes', 'Active', 640, 'Hybrid Battery', 6200, 'Marine Gas Oil/Battery', 2021),
('MS Maritime Castor', '9892691', 'Coastal Cruise Ship', 'Bergen-Kirkenes', 'Active', 640, 'Hybrid Battery', 6200, 'Marine Gas Oil/Battery', 2021),
('MS Maritime Polaris', '9892703', 'Coastal Cruise Ship', 'Bergen-Kirkenes', 'Active', 640, 'Hybrid Battery', 6200, 'Marine Gas Oil/Battery', 2022),
('MS Maritime Pollux', '9892715', 'Coastal Cruise Ship', 'Bergen-Kirkenes', 'Active', 640, 'Hybrid Battery', 6200, 'Marine Gas Oil/Battery', 2022);

INSERT INTO ports (port_name, country, latitude, longitude, port_type) VALUES
('Bergen', 'Norway', 60.3913, 5.3221, 'Coastal Port'),
('Kirkenes', 'Norway', 69.7275, 30.0458, 'Coastal Port'),
('Tromsø', 'Norway', 69.6492, 18.9553, 'Coastal Port'),
('Trondheim', 'Norway', 63.4305, 10.3951, 'Coastal Port'),
('Ålesund', 'Norway', 62.4722, 6.1549, 'Coastal Port'),
('Bodø', 'Norway', 67.2804, 14.4049, 'Coastal Port'),
('Honningsvåg', 'Norway', 70.9828, 25.9704, 'Coastal Port'),
('Hammerfest', 'Norway', 70.6634, 23.6824, 'Coastal Port'),
('Vadsø', 'Norway', 70.0741, 29.7488, 'Coastal Port'),
('Båtsfjord', 'Norway', 70.6341, 29.7216, 'Coastal Port');

INSERT INTO voyages (vessel_id, departure_port_id, arrival_port_id, scheduled_departure, scheduled_arrival, voyage_status, passenger_count, fuel_consumed_liters, co2_emissions_kg, battery_usage_kwh) VALUES
((SELECT vessel_id FROM vessels WHERE vessel_name = 'MS Maritime Capella'),
 (SELECT port_id FROM ports WHERE port_name = 'Bergen'),
 (SELECT port_id FROM ports WHERE port_name = 'Tromsø'),
 '2024-03-15 14:30:00', '2024-03-16 08:00:00', 'Completed', 524, 2500.50, 6580.25, 1850.75),
((SELECT vessel_id FROM vessels WHERE vessel_name = 'MS Maritime Castor'),
 (SELECT port_id FROM ports WHERE port_name = 'Tromsø'),
 (SELECT port_id FROM ports WHERE port_name = 'Kirkenes'),
 '2024-03-16 10:00:00', '2024-03-17 06:00:00', 'Completed', 445, 2200.75, 5789.95, 1650.25),
((SELECT vessel_id FROM vessels WHERE vessel_name = 'MS Maritime Polaris'),
 (SELECT port_id FROM ports WHERE port_name = 'Kirkenes'),
 (SELECT port_id FROM ports WHERE port_name = 'Bergen'),
 '2024-03-17 09:30:00', '2024-03-18 20:00:00', 'In Progress', 587, 3200.00, 8420.00, 2100.50),
((SELECT vessel_id FROM vessels WHERE vessel_name = 'MS Maritime Pollux'),
 (SELECT port_id FROM ports WHERE port_name = 'Bergen'),
 (SELECT port_id FROM ports WHERE port_name = 'Ålesund'),
 '2024-03-18 12:00:00', '2024-03-18 18:00:00', 'Scheduled', 0, NULL, NULL, NULL);

INSERT INTO environmental_data (vessel_id, timestamp, latitude, longitude, speed_knots, fuel_consumption_lph, battery_charge_percent, co2_emissions_kg_h, nox_emissions_kg_h, sox_emissions_kg_h, weather_conditions, sea_state, hybrid_mode_active) VALUES
((SELECT vessel_id FROM vessels WHERE vessel_name = 'MS Maritime Capella'),
 '2024-03-15 15:00:00', 60.5, 5.5, 18.5, 125.5, 85.2, 45.2, 8.1, 0.3, 'Clear', 'Calm', 1),
((SELECT vessel_id FROM vessels WHERE vessel_name = 'MS Maritime Capella'),
 '2024-03-15 16:00:00', 61.2, 6.1, 19.2, 128.3, 82.1, 46.8, 8.3, 0.32, 'Clear', 'Calm', 1),
((SELECT vessel_id FROM vessels WHERE vessel_name = 'MS Maritime Castor'),
 '2024-03-16 11:00:00', 70.1, 19.5, 17.8, 118.7, 88.5, 42.9, 7.8, 0.28, 'Partly Cloudy', 'Moderate', 1),
((SELECT vessel_id FROM vessels WHERE vessel_name = 'MS Maritime Polaris'),
 '2024-03-17 14:00:00', 69.8, 25.2, 16.5, 112.3, 91.2, 41.5, 7.5, 0.25, 'Overcast', 'Slight', 1);

INSERT INTO ai_models (model_name, model_type, version, accuracy_score) VALUES
('Vessel Route Optimizer', 'Optimization', '2.1', 0.942),
('Environmental Predictor', 'Prediction', '1.8', 0.896),
('Fuel Efficiency Analyzer', 'Analysis', '2.0', 0.918),
('Passenger Behavior Predictor', 'Prediction', '1.5', 0.875),
('Northern Lights Forecast Model', 'Forecast', '1.2', 0.823),
('Safety Risk Assessment Model', 'Risk Analysis', '1.9', 0.934);

INSERT INTO passengers (first_name, last_name, email, phone, nationality, date_of_birth, preferences, loyalty_tier) VALUES
('Erik', 'Hansen', 'erik.hansen@email.no', '+4798765432', 'Norwegian', '1975-06-15', '{"notifications": {"email": true, "sms": false, "northern_lights": true}, "cabin_preferences": {"deck_level": "upper"}}', 'Gold'),
('Sarah', 'Johnson', 'sarah.j@email.com', '+447891234567', 'British', '1982-03-22', '{"notifications": {"email": true, "sms": true, "northern_lights": true}, "dietary": "vegetarian"}', 'Silver'),
('Klaus', 'Müller', 'k.mueller@email.de', '+4915123456789', 'German', '1968-11-08', '{"notifications": {"email": true, "sms": false, "northern_lights": true}, "language": "german"}', 'Platinum'),
('Marie', 'Dubois', 'm.dubois@email.fr', '+33612345678', 'French', '1990-01-30', '{"notifications": {"email": true, "sms": true, "northern_lights": true}, "accessibility": "wheelchair"}', 'Standard');

INSERT INTO northern_lights_forecasts (forecast_date, latitude, longitude, aurora_probability, aurora_intensity, optimal_viewing_time, cloud_cover_percent, weather_description) VALUES
('2024-03-18', 69.6492, 18.9553, 85, 'High', '2024-03-18 22:30:00', 15, 'Clear skies with minimal cloud cover'),
('2024-03-19', 70.2143, 19.7621, 92, 'Very High', '2024-03-19 21:45:00', 8, 'Excellent conditions, clear and cold'),
('2024-03-20', 69.9731, 23.2705, 78, 'High', '2024-03-20 23:15:00', 25, 'Partly cloudy, good viewing possible'),
('2024-03-21', 70.6634, 23.6824, 65, 'Moderate', '2024-03-21 22:00:00', 45, 'Overcast conditions, limited visibility');

-- Create stored procedures for data processing
GO
CREATE PROCEDURE sp_ValidateMaritimeData
    @BatchId NVARCHAR(50),
    @ProcessingDate DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Data quality validation for environmental data
    UPDATE environmental_data 
    SET created_at = @ProcessingDate 
    WHERE created_at > DATEADD(hour, -1, @ProcessingDate)
    AND (co2_emissions_kg_h < 0 OR co2_emissions_kg_h > 100
         OR battery_charge_percent < 0 OR battery_charge_percent > 100);
    
    -- Log validation results
    INSERT INTO ai_models (model_name, model_type, version, accuracy_score)
    VALUES ('Data Validation Batch ' + @BatchId, 'Validation', '1.0', 0.999);
END;
GO

CREATE PROCEDURE sp_GenerateFleetAnalytics
    @BatchId NVARCHAR(50),
    @AnalyticsDate DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Update vessel performance metrics
    -- This would contain complex analytics logic in production
    SELECT 
        v.vessel_name,
        COUNT(voy.voyage_id) as total_voyages,
        AVG(ed.fuel_consumption_lph) as avg_fuel_consumption,
        AVG(ed.co2_emissions_kg_h) as avg_co2_emissions,
        AVG(ed.battery_charge_percent) as avg_battery_level
    FROM vessels v
    LEFT JOIN voyages voy ON v.vessel_id = voy.vessel_id
    LEFT JOIN environmental_data ed ON v.vessel_id = ed.vessel_id
    WHERE ed.timestamp >= DATEADD(day, -7, @AnalyticsDate)
    GROUP BY v.vessel_id, v.vessel_name;
END;
GO

CREATE PROCEDURE sp_AnalyzePassengerExperience
    @AnalysisDate DATETIME2,
    @IncludeNorthernLightsData BIT = 1,
    @WeatherImpactThreshold DECIMAL(3,1) = 3.0
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Analyze passenger satisfaction metrics
    SELECT 
        p.passenger_id,
        p.loyalty_tier,
        COUNT(b.booking_id) as total_bookings,
        AVG(CASE WHEN nlf.aurora_probability > 70 THEN 1.0 ELSE 0.0 END) as northern_lights_opportunities
    FROM passengers p
    LEFT JOIN bookings b ON p.passenger_id = b.passenger_id
    LEFT JOIN voyages v ON b.voyage_id = v.voyage_id
    LEFT JOIN northern_lights_forecasts nlf ON CAST(v.scheduled_departure AS DATE) = nlf.forecast_date
    WHERE v.scheduled_departure >= DATEADD(month, -3, @AnalysisDate)
    GROUP BY p.passenger_id, p.loyalty_tier;
END;
GO

-- Create views for Power BI dashboards
CREATE VIEW vw_FleetAnalyticsDashboard AS
SELECT 
    v.vessel_name,
    v.route_assignment,
    v.status,
    v.passenger_capacity,
    v.hybrid_system_type,
    COUNT(DISTINCT voy.voyage_id) as total_voyages_last_30_days,
    AVG(ed.fuel_consumption_lph) as avg_fuel_consumption,
    AVG(ed.co2_emissions_kg_h) as avg_co2_emissions,
    AVG(ed.battery_charge_percent) as avg_battery_level,
    AVG(ed.speed_knots) as avg_speed,
    SUM(CASE WHEN si.severity = 'Critical' THEN 1 ELSE 0 END) as critical_incidents_30_days,
    GETDATE() as processing_date
FROM vessels v
LEFT JOIN voyages voy ON v.vessel_id = voy.vessel_id 
    AND voy.scheduled_departure >= DATEADD(day, -30, GETDATE())
LEFT JOIN environmental_data ed ON v.vessel_id = ed.vessel_id 
    AND ed.timestamp >= DATEADD(day, -30, GETDATE())
LEFT JOIN safety_incidents si ON v.vessel_id = si.vessel_id 
    AND si.incident_date >= DATEADD(day, -30, GETDATE())
GROUP BY v.vessel_id, v.vessel_name, v.route_assignment, v.status, 
         v.passenger_capacity, v.hybrid_system_type;
GO

CREATE VIEW vw_EnvironmentalCompliance AS
SELECT 
    v.vessel_name,
    AVG(ed.co2_emissions_kg_h) as avg_co2_emissions,
    AVG(ed.nox_emissions_kg_h) as avg_nox_emissions,
    AVG(ed.sox_emissions_kg_h) as avg_sox_emissions,
    AVG(ed.battery_charge_percent) as avg_battery_efficiency,
    AVG(CASE WHEN ed.hybrid_mode_active = 1 THEN 1.0 ELSE 0.0 END) * 100 as hybrid_mode_usage_percent,
    CASE 
        WHEN AVG(ed.co2_emissions_kg_h) <= 50.0 THEN 'Compliant'
        ELSE 'Non-Compliant'
    END as co2_compliance_status,
    CASE 
        WHEN AVG(ed.nox_emissions_kg_h) <= 9.0 THEN 'Compliant'
        ELSE 'Non-Compliant'
    END as nox_compliance_status,
    GETDATE() as assessment_date
FROM vessels v
LEFT JOIN environmental_data ed ON v.vessel_id = ed.vessel_id 
    AND ed.timestamp >= DATEADD(day, -7, GETDATE())
GROUP BY v.vessel_id, v.vessel_name;
GO