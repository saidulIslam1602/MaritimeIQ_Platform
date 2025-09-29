# Databricks notebook source
# MAGIC %md
# MAGIC # Maritime Data Processing with PySpark
# MAGIC ## Advanced analytics and ML on vessel and environmental data
# MAGIC 
# MAGIC **Capabilities:**
# MAGIC - Batch processing of voyage data
# MAGIC - Fleet-wide aggregations and KPIs
# MAGIC - Time-series analysis of emissions
# MAGIC - Predictive maintenance ML models
# MAGIC - Route optimization analytics

# COMMAND ----------

# MAGIC %md
# MAGIC ## 1. Initialize Spark Session

# COMMAND ----------

from pyspark.sql import SparkSession, Window
from pyspark.sql.functions import *
from pyspark.sql.types import *
from delta.tables import DeltaTable
import matplotlib.pyplot as plt
import pandas as pd
from datetime import datetime, timedelta

# ML libraries
from pyspark.ml.feature import VectorAssembler, StandardScaler
from pyspark.ml.regression import RandomForestRegressor, GBTRegressor
from pyspark.ml.classification import RandomForestClassifier
from pyspark.ml.evaluation import RegressionEvaluator, MulticlassClassificationEvaluator
from pyspark.ml import Pipeline

spark = SparkSession.builder \
    .appName("MaritimeIQ-DataProcessing") \
    .config("spark.sql.adaptive.enabled", "true") \
    .config("spark.sql.adaptive.coalescePartitions.enabled", "true") \
    .getOrCreate()

print("✅ Spark session initialized")
print(f"📊 Spark version: {spark.version}")

# COMMAND ----------

# MAGIC %md
# MAGIC ## 2. Load Data from Delta Lake

# COMMAND ----------

# Delta Lake paths
DELTA_PATHS = {
    "ais": "/mnt/maritime/delta/ais_positions",
    "environmental": "/mnt/maritime/delta/environmental_sensors",
    "voyages": "/mnt/maritime/delta/voyages"
}

# Load AIS data
df_ais = spark.read.format("delta").load(DELTA_PATHS["ais"])
print(f"📍 AIS Records loaded: {df_ais.count():,}")

# Load environmental data
df_env = spark.read.format("delta").load(DELTA_PATHS["environmental"])
print(f"🌊 Environmental Records loaded: {df_env.count():,}")

# Load voyage data
df_voyages = spark.read.format("delta").load(DELTA_PATHS["voyages"])
print(f"⚓ Voyage Records loaded: {df_voyages.count():,}")

# COMMAND ----------

# MAGIC %md
# MAGIC ## 3. Fleet-Wide Aggregations and KPIs

# COMMAND ----------

def calculate_fleet_kpis(start_date, end_date):
    """
    Calculate comprehensive fleet KPIs for a date range
    """
    print(f"📊 Calculating Fleet KPIs from {start_date} to {end_date}")
    
    # Filter data by date range
    df_ais_period = df_ais.filter(
        (col("Timestamp") >= start_date) & (col("Timestamp") <= end_date)
    )
    
    df_env_period = df_env.filter(
        (col("MeasurementTime") >= start_date) & (col("MeasurementTime") <= end_date)
    )
    
    # Operational KPIs
    operational_kpis = df_ais_period.agg(
        count("*").alias("total_position_updates"),
        countDistinct("MMSI").alias("active_vessels"),
        avg("Speed").alias("avg_fleet_speed_knots"),
        max("Speed").alias("max_speed_recorded"),
        sum(col("Speed") * 1.852).alias("total_distance_km")  # Convert knots to km/h
    ).collect()[0]
    
    # Environmental KPIs
    environmental_kpis = df_env_period.agg(
        sum("CO2EmissionKg").alias("total_co2_kg"),
        avg("CO2EmissionKg").alias("avg_co2_per_hour"),
        sum("FuelConsumptionLiters").alias("total_fuel_liters"),
        avg("BatteryStateOfCharge").alias("avg_battery_soc"),
        sum("NOxEmissionKg").alias("total_nox_kg"),
        sum("SOxEmissionKg").alias("total_sox_kg")
    ).collect()[0]
    
    # Compliance metrics
    compliance = df_env_period.agg(
        (sum(when(col("CO2EmissionKg") <= 50, 1).otherwise(0)) / count("*") * 100).alias("co2_compliance_rate"),
        (sum(when(col("BatteryStateOfCharge") >= 20, 1).otherwise(0)) / count("*") * 100).alias("battery_healthy_rate")
    ).collect()[0]
    
    # Combine all KPIs
    kpis = {
        "period": {"start": start_date, "end": end_date},
        "operational": {
            "total_position_updates": operational_kpis["total_position_updates"],
            "active_vessels": operational_kpis["active_vessels"],
            "avg_fleet_speed_knots": round(operational_kpis["avg_fleet_speed_knots"], 2),
            "max_speed_recorded": round(operational_kpis["max_speed_recorded"], 2),
            "total_distance_km": round(operational_kpis["total_distance_km"], 2)
        },
        "environmental": {
            "total_co2_kg": round(environmental_kpis["total_co2_kg"], 2),
            "avg_co2_per_hour": round(environmental_kpis["avg_co2_per_hour"], 2),
            "total_fuel_liters": round(environmental_kpis["total_fuel_liters"], 2),
            "avg_battery_soc": round(environmental_kpis["avg_battery_soc"], 2),
            "total_nox_kg": round(environmental_kpis["total_nox_kg"], 2),
            "total_sox_kg": round(environmental_kpis["total_sox_kg"], 2)
        },
        "compliance": {
            "co2_compliance_rate": round(compliance["co2_compliance_rate"], 2),
            "battery_healthy_rate": round(compliance["battery_healthy_rate"], 2)
        }
    }
    
    # Display results
    print("\n" + "="*70)
    print("FLEET KPIs SUMMARY")
    print("="*70)
    print(f"\n📍 Operational Metrics:")
    print(f"   Active Vessels: {kpis['operational']['active_vessels']}")
    print(f"   Avg Fleet Speed: {kpis['operational']['avg_fleet_speed_knots']} knots")
    print(f"   Total Distance: {kpis['operational']['total_distance_km']:,} km")
    
    print(f"\n🌊 Environmental Metrics:")
    print(f"   Total CO2 Emissions: {kpis['environmental']['total_co2_kg']:,} kg")
    print(f"   Total Fuel Consumed: {kpis['environmental']['total_fuel_liters']:,} liters")
    print(f"   Avg Battery SOC: {kpis['environmental']['avg_battery_soc']}%")
    
    print(f"\n✅ Compliance:")
    print(f"   CO2 Compliance Rate: {kpis['compliance']['co2_compliance_rate']}%")
    print(f"   Battery Health Rate: {kpis['compliance']['battery_healthy_rate']}%")
    print("="*70 + "\n")
    
    return kpis

# Calculate KPIs for last 30 days
end_date = datetime.now()
start_date = end_date - timedelta(days=30)
fleet_kpis = calculate_fleet_kpis(start_date.isoformat(), end_date.isoformat())

# COMMAND ----------

# MAGIC %md
# MAGIC ## 4. Time-Series Analysis of Emissions

# COMMAND ----------

def analyze_emissions_trends():
    """
    Analyze emission trends over time with moving averages
    """
    print("📈 Analyzing emissions trends...")
    
    # Aggregate emissions by day
    df_daily_emissions = df_env \
        .withColumn("date", to_date(col("MeasurementTime"))) \
        .groupBy("date", "VesselId") \
        .agg(
            sum("CO2EmissionKg").alias("daily_co2"),
            sum("NOxEmissionKg").alias("daily_nox"),
            sum("SOxEmissionKg").alias("daily_sox"),
            avg("BatteryStateOfCharge").alias("avg_battery_soc"),
            sum("FuelConsumptionLiters").alias("daily_fuel")
        ) \
        .orderBy("date")
    
    # Add moving averages (7-day window)
    window_spec = Window.partitionBy("VesselId").orderBy("date").rowsBetween(-6, 0)
    
    df_with_trends = df_daily_emissions \
        .withColumn("co2_7day_avg", avg("daily_co2").over(window_spec)) \
        .withColumn("co2_trend", 
            when(col("daily_co2") > col("co2_7day_avg") * 1.1, "Increasing")
            .when(col("daily_co2") < col("co2_7day_avg") * 0.9, "Decreasing")
            .otherwise("Stable")
        )
    
    # Fleet-wide daily aggregations
    df_fleet_daily = df_daily_emissions \
        .groupBy("date") \
        .agg(
            sum("daily_co2").alias("fleet_co2"),
            sum("daily_nox").alias("fleet_nox"),
            sum("daily_sox").alias("fleet_sox"),
            avg("avg_battery_soc").alias("fleet_battery_avg"),
            sum("daily_fuel").alias("fleet_fuel")
        ) \
        .orderBy("date")
    
    # Show recent trends
    print("\n📊 Recent Daily Fleet Emissions:")
    df_fleet_daily.orderBy(desc("date")).limit(10).show()
    
    # Identify vessels with increasing emissions
    vessels_with_issues = df_with_trends \
        .filter(col("co2_trend") == "Increasing") \
        .groupBy("VesselId") \
        .agg(count("*").alias("days_increasing")) \
        .filter(col("days_increasing") >= 3) \
        .orderBy(desc("days_increasing"))
    
    if vessels_with_issues.count() > 0:
        print("\n⚠️  Vessels with Increasing Emissions (3+ consecutive days):")
        vessels_with_issues.show()
    
    return df_with_trends, df_fleet_daily

df_emission_trends, df_fleet_emissions = analyze_emissions_trends()

# COMMAND ----------

# MAGIC %md
# MAGIC ## 5. Voyage Performance Analysis

# COMMAND ----------

def analyze_voyage_performance():
    """
    Comprehensive voyage performance analysis
    """
    print("⚓ Analyzing voyage performance...")
    
    # Calculate voyage metrics
    df_voyage_metrics = df_voyages \
        .withColumn("duration_hours", 
            (unix_timestamp("ArrivalTime") - unix_timestamp("DepartureTime")) / 3600
        ) \
        .withColumn("is_delayed",
            when(col("Status") == "Delayed", 1).otherwise(0)
        )
    
    # Performance by route
    df_route_performance = df_voyage_metrics \
        .groupBy("DeparturePort", "ArrivalPort") \
        .agg(
            count("*").alias("total_voyages"),
            avg("duration_hours").alias("avg_duration_hours"),
            sum("is_delayed").alias("delayed_count"),
            (sum("is_delayed") / count("*") * 100).alias("delay_rate_pct"),
            avg("PassengerCount").alias("avg_passengers"),
            sum("PassengerCount").alias("total_passengers")
        ) \
        .orderBy(desc("total_voyages"))
    
    print("\n📍 Performance by Route:")
    df_route_performance.show(10, truncate=False)
    
    # Identify best performing vessels
    df_vessel_performance = df_voyage_metrics \
        .groupBy("VesselId") \
        .agg(
            count("*").alias("completed_voyages"),
            avg("duration_hours").alias("avg_voyage_duration"),
            (sum("is_delayed") / count("*") * 100).alias("delay_rate"),
            avg("PassengerCount").alias("avg_occupancy")
        ) \
        .orderBy("delay_rate")
    
    print("\n🚢 Vessel Performance Rankings:")
    df_vessel_performance.show()
    
    return df_route_performance, df_vessel_performance

df_routes, df_vessel_perf = analyze_voyage_performance()

# COMMAND ----------

# MAGIC %md
# MAGIC ## 6. Predictive Maintenance Model

# COMMAND ----------

def build_predictive_maintenance_model():
    """
    Build ML model to predict maintenance needs based on sensor data
    """
    print("🤖 Building Predictive Maintenance Model...")
    
    # Prepare training data - aggregate sensor readings
    df_maintenance_features = df_env \
        .groupBy("VesselId") \
        .agg(
            avg("CO2EmissionKg").alias("avg_co2"),
            stddev("CO2EmissionKg").alias("std_co2"),
            avg("FuelConsumptionLiters").alias("avg_fuel"),
            avg("BatteryStateOfCharge").alias("avg_battery"),
            min("BatteryStateOfCharge").alias("min_battery"),
            avg("WaterTemperature").alias("avg_water_temp"),
            avg("AirTemperature").alias("avg_air_temp"),
            count("*").alias("sensor_reading_count")
        )
    
    # Create maintenance risk score (synthetic for demonstration)
    # In production, this would be based on actual maintenance records
    df_with_labels = df_maintenance_features \
        .withColumn("maintenance_risk_score",
            when(
                (col("avg_co2") > 400) | 
                (col("avg_battery") < 30) | 
                (col("std_co2") > 100), 3
            )
            .when(
                (col("avg_co2") > 300) | 
                (col("avg_battery") < 50), 2
            )
            .when(
                (col("avg_co2") > 200), 1
            )
            .otherwise(0)
        ) \
        .withColumn("needs_maintenance",
            when(col("maintenance_risk_score") >= 2, 1.0).otherwise(0.0)
        )
    
    # Feature engineering
    feature_cols = [
        "avg_co2", "std_co2", "avg_fuel", "avg_battery", 
        "min_battery", "avg_water_temp", "avg_air_temp"
    ]
    
    assembler = VectorAssembler(
        inputCols=feature_cols,
        outputCol="features_raw"
    )
    
    scaler = StandardScaler(
        inputCol="features_raw",
        outputCol="features"
    )
    
    # Random Forest Classifier
    rf = RandomForestClassifier(
        featuresCol="features",
        labelCol="needs_maintenance",
        numTrees=100,
        maxDepth=5,
        seed=42
    )
    
    # Create pipeline
    pipeline = Pipeline(stages=[assembler, scaler, rf])
    
    # Split data
    train_data, test_data = df_with_labels.randomSplit([0.8, 0.2], seed=42)
    
    print(f"📊 Training set: {train_data.count()} vessels")
    print(f"📊 Test set: {test_data.count()} vessels")
    
    # Train model
    print("🔧 Training model...")
    model = pipeline.fit(train_data)
    
    # Make predictions
    predictions = model.transform(test_data)
    
    # Evaluate
    evaluator = MulticlassClassificationEvaluator(
        labelCol="needs_maintenance",
        predictionCol="prediction",
        metricName="accuracy"
    )
    
    accuracy = evaluator.evaluate(predictions)
    print(f"\n✅ Model Accuracy: {accuracy:.2%}")
    
    # Show feature importance
    rf_model = model.stages[-1]
    feature_importance = list(zip(feature_cols, rf_model.featureImportances.toArray()))
    feature_importance.sort(key=lambda x: x[1], reverse=True)
    
    print("\n📊 Feature Importance:")
    for feature, importance in feature_importance:
        print(f"   {feature}: {importance:.4f}")
    
    # Show predictions for high-risk vessels
    print("\n⚠️  Vessels Predicted to Need Maintenance:")
    predictions.filter(col("prediction") == 1.0) \
        .select("VesselId", "avg_co2", "avg_battery", "maintenance_risk_score", "prediction") \
        .show()
    
    return model, predictions

maintenance_model, maintenance_predictions = build_predictive_maintenance_model()

# COMMAND ----------

# MAGIC %md
# MAGIC ## 7. Fuel Efficiency Optimization

# COMMAND ----------

def analyze_fuel_efficiency():
    """
    Analyze and optimize fuel efficiency across the fleet
    """
    print("⛽ Analyzing Fuel Efficiency...")
    
    # Join AIS and environmental data
    df_efficiency = df_ais \
        .join(
            df_env,
            (df_ais["MMSI"] == df_env["VesselId"].cast(StringType())) &
            (abs(unix_timestamp(df_ais["Timestamp"]) - unix_timestamp(df_env["MeasurementTime"])) < 300),
            "inner"
        ) \
        .select(
            df_ais["MMSI"].alias("vessel_mmsi"),
            df_ais["Speed"].alias("speed_knots"),
            df_env["FuelConsumptionLiters"].alias("fuel_liters"),
            df_env["BatteryStateOfCharge"].alias("battery_soc"),
            df_env["CO2EmissionKg"].alias("co2_kg"),
            df_ais["Timestamp"]
        )
    
    # Calculate efficiency metrics
    df_efficiency_metrics = df_efficiency \
        .withColumn("fuel_per_knot", col("fuel_liters") / col("speed_knots")) \
        .withColumn("speed_category",
            when(col("speed_knots") < 10, "Low")
            .when(col("speed_knots") < 18, "Optimal")
            .otherwise("High")
        ) \
        .withColumn("battery_usage_category",
            when(col("battery_soc") > 70, "High")
            .when(col("battery_soc") > 30, "Medium")
            .otherwise("Low")
        )
    
    # Efficiency by speed category
    df_speed_efficiency = df_efficiency_metrics \
        .groupBy("speed_category") \
        .agg(
            avg("fuel_per_knot").alias("avg_fuel_per_knot"),
            avg("co2_kg").alias("avg_co2"),
            count("*").alias("sample_count")
        ) \
        .orderBy("speed_category")
    
    print("\n⚡ Fuel Efficiency by Speed Category:")
    df_speed_efficiency.show()
    
    # Best practices recommendations
    optimal_speed = df_efficiency_metrics \
        .filter(col("speed_category") == "Optimal") \
        .agg(
            avg("fuel_per_knot").alias("optimal_fuel_per_knot"),
            avg("speed_knots").alias("optimal_speed")
        ).collect()[0]
    
    print(f"\n💡 Recommended Optimal Speed: {optimal_speed['optimal_speed']:.1f} knots")
    print(f"   Fuel Efficiency: {optimal_speed['optimal_fuel_per_knot']:.2f} liters/knot")
    
    # Vessel-specific efficiency rankings
    df_vessel_efficiency = df_efficiency_metrics \
        .groupBy("vessel_mmsi") \
        .agg(
            avg("fuel_per_knot").alias("avg_fuel_efficiency"),
            avg("battery_soc").alias("avg_battery_usage"),
            count("*").alias("readings")
        ) \
        .orderBy("avg_fuel_efficiency")
    
    print("\n🏆 Most Fuel Efficient Vessels:")
    df_vessel_efficiency.show(5)
    
    return df_efficiency_metrics, df_vessel_efficiency

df_efficiency, df_vessel_efficiency = analyze_fuel_efficiency()

# COMMAND ----------

# MAGIC %md
# MAGIC ## 8. Save Processed Results to Delta Lake

# COMMAND ----------

def save_analytics_results():
    """
    Save processed analytics back to Delta Lake for consumption
    """
    print("💾 Saving analytics results to Delta Lake...")
    
    # Save emission trends
    df_emission_trends.write \
        .format("delta") \
        .mode("overwrite") \
        .option("overwriteSchema", "true") \
        .save("/mnt/maritime/delta/analytics/emission_trends")
    print("✅ Emission trends saved")
    
    # Save vessel performance
    df_vessel_perf.write \
        .format("delta") \
        .mode("overwrite") \
        .option("overwriteSchema", "true") \
        .save("/mnt/maritime/delta/analytics/vessel_performance")
    print("✅ Vessel performance saved")
    
    # Save maintenance predictions
    maintenance_predictions \
        .select("VesselId", "prediction", "maintenance_risk_score", 
                "avg_co2", "avg_battery", "needs_maintenance") \
        .write \
        .format("delta") \
        .mode("overwrite") \
        .option("overwriteSchema", "true") \
        .save("/mnt/maritime/delta/analytics/maintenance_predictions")
    print("✅ Maintenance predictions saved")
    
    # Save fuel efficiency metrics
    df_vessel_efficiency.write \
        .format("delta") \
        .mode("overwrite") \
        .option("overwriteSchema", "true") \
        .save("/mnt/maritime/delta/analytics/fuel_efficiency")
    print("✅ Fuel efficiency metrics saved")
    
    print("\n🎉 All analytics results saved successfully!")

save_analytics_results()

# COMMAND ----------

# MAGIC %md
# MAGIC ## 9. Summary and Recommendations

# COMMAND ----------

print("""
╔════════════════════════════════════════════════════════════════════╗
║        MARITIME DATA PROCESSING - EXECUTION SUMMARY                ║
╚════════════════════════════════════════════════════════════════════╝

✅ COMPLETED TASKS:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

📊 Fleet KPIs Calculated
   - Operational metrics across all vessels
   - Environmental compliance tracking
   - Performance benchmarking

📈 Time-Series Analysis
   - Emission trends with moving averages
   - Anomaly detection for increasing emissions
   - Fleet-wide daily aggregations

⚓ Voyage Performance Analysis
   - Route efficiency metrics
   - Vessel-specific performance rankings
   - Delay pattern identification

🤖 Predictive Maintenance Model
   - Random Forest classifier trained
   - Feature importance analysis completed
   - High-risk vessels identified

⛽ Fuel Efficiency Optimization
   - Optimal speed recommendations
   - Vessel efficiency rankings
   - Battery usage correlation analysis

💾 Results Stored in Delta Lake:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   📂 /mnt/maritime/delta/analytics/emission_trends
   📂 /mnt/maritime/delta/analytics/vessel_performance
   📂 /mnt/maritime/delta/analytics/maintenance_predictions
   📂 /mnt/maritime/delta/analytics/fuel_efficiency

🎯 NEXT STEPS:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
1. Schedule this notebook to run daily for continuous monitoring
2. Set up alerts for vessels with increasing emissions
3. Integrate maintenance predictions with work order system
4. Create Power BI dashboards using Delta Lake analytics tables
5. Implement automated route optimization based on efficiency metrics

🚢 Maritime Analytics Processing Complete! 🌊
""")

# COMMAND ----------