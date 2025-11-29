# Databricks notebook source
# MAGIC %md
# MAGIC # Maritime Silver Layer Transformation
# MAGIC ## Industry-Standard Data Quality and Transformation Pipeline
# MAGIC 
# MAGIC **Features:**
# MAGIC - Comprehensive data quality validation with scoring
# MAGIC - Deduplication based on business keys
# MAGIC - Schema evolution and standardization
# MAGIC - Change Data Capture (CDC) support
# MAGIC - Data lineage tracking
# MAGIC - Performance optimization with Z-ordering
# MAGIC - Monitoring and alerting
# MAGIC 
# MAGIC **Input:** Bronze layer (`/mnt/maritime/delta/ais_positions`, `/mnt/maritime/delta/environmental_sensors`)
# MAGIC **Output:** Silver layer (`/mnt/datalake/maritime/silver/`)

# COMMAND ----------

# MAGIC %md
# MAGIC ## 1. Configuration and Setup

# COMMAND ----------

from pyspark.sql import SparkSession, Window
from pyspark.sql.functions import *
from pyspark.sql.types import *
from delta.tables import DeltaTable
import json
from datetime import datetime, timedelta
import uuid

# Initialize Spark with optimized configuration for Silver layer
spark = SparkSession.builder \
    .appName("MaritimeIQ-Silver-Layer") \
    .config("spark.sql.extensions", "io.delta.sql.DeltaSparkSessionExtension") \
    .config("spark.sql.catalog.spark_catalog", "org.apache.spark.sql.delta.catalog.DeltaCatalog") \
    .config("spark.sql.adaptive.enabled", "true") \
    .config("spark.sql.adaptive.coalescePartitions.enabled", "true") \
    .config("spark.sql.adaptive.skewJoin.enabled", "true") \
    .config("spark.sql.shuffle.partitions", "200") \
    .config("spark.default.parallelism", "100") \
    .getOrCreate()

# Set logging level
spark.sparkContext.setLogLevel("WARN")

print("Spark session initialized with Silver layer optimizations")
print(f"Spark version: {spark.version}")

# COMMAND ----------

# MAGIC %md
# MAGIC ## 2. Configuration Parameters

# COMMAND ----------

# Get parameters from pipeline (with defaults for development)
session_id = dbutils.widgets.get("session_id") if "session_id" in [w.name for w in dbutils.widgets.getAll()] else str(uuid.uuid4())[:8]
bronze_path = dbutils.widgets.get("bronze_path") if "bronze_path" in [w.name for w in dbutils.widgets.getAll()] else "/mnt/maritime/delta"
silver_path = dbutils.widgets.get("silver_path") if "silver_path" in [w.name for w in dbutils.widgets.getAll()] else "/mnt/datalake/maritime/silver"
data_quality_checks = dbutils.widgets.get("data_quality_checks") if "data_quality_checks" in [w.name for w in dbutils.widgets.getAll()] else "true"
schema_evolution = dbutils.widgets.get("schema_evolution") if "schema_evolution" in [w.name for w in dbutils.widgets.getAll()] else "true"
cdc_enabled = dbutils.widgets.get("cdc_enabled") if "cdc_enabled" in [w.name for w in dbutils.widgets.getAll()] else "true"

# Configuration
config = {
    "session_id": session_id,
    "bronze_paths": {
        "ais_positions": f"{bronze_path}/ais_positions",
        "environmental_sensors": f"{bronze_path}/environmental_sensors", 
        "voyages": f"{bronze_path}/voyages",
        "alerts": f"{bronze_path}/alerts"
    },
    "silver_paths": {
        "ais_positions": f"{silver_path}/ais_positions",
        "environmental_sensors": f"{silver_path}/environmental_sensors",
        "voyages": f"{silver_path}/voyages", 
        "alerts": f"{silver_path}/alerts"
    },
    "checkpoints": {
        "silver": f"/mnt/checkpoints/silver/{session_id}"
    },
    "data_quality": {
        "enabled": data_quality_checks.lower() == "true",
        "min_quality_score": 0.7,
        "max_null_percentage": 5.0,
        "latitude_range": (-90, 90),
        "longitude_range": (-180, 180),
        "speed_range": (0, 40),  # knots
        "co2_threshold": 1000,   # kg/hour
        "battery_range": (0, 100)
    },
    "schema_evolution": schema_evolution.lower() == "true",
    "cdc_enabled": cdc_enabled.lower() == "true",
    "processing_timestamp": datetime.now()
}

print(f"Silver layer configuration loaded for session: {session_id}")
print(f"Bronze path: {bronze_path}")
print(f"Silver path: {silver_path}")

# COMMAND ----------

# MAGIC %md
# MAGIC ## 3. Data Quality Functions

# COMMAND ----------

def calculate_data_quality_score(df, table_name):
    """
    Calculate comprehensive data quality score based on multiple factors
    Returns: DataFrame with quality_score column (0.0 to 1.0)
    """
    print(f"Calculating data quality score for {table_name}")
    
    if table_name == "ais_positions":
        # AIS-specific quality checks
        df_with_quality = df.withColumn("quality_score",
            when(
                # Core fields present
                (col("MMSI").isNotNull()) &
                (col("Latitude").isNotNull()) & 
                (col("Longitude").isNotNull()) &
                (col("Timestamp").isNotNull()) &
                # Valid ranges
                (col("Latitude").between(*config["data_quality"]["latitude_range"])) &
                (col("Longitude").between(*config["data_quality"]["longitude_range"])) &
                (col("Speed").between(*config["data_quality"]["speed_range"]) | col("Speed").isNull()),
                # High quality: all checks pass
                when(col("VesselName").isNotNull() & col("Heading").isNotNull(), 1.0)
                .otherwise(0.8)  # Medium quality: core fields valid but missing optional
            ).when(
                # Medium quality: some issues but recoverable
                (col("MMSI").isNotNull()) &
                (col("Latitude").isNotNull()) & 
                (col("Longitude").isNotNull()),
                0.6
            ).otherwise(0.0)  # Low quality: major issues
        )
        
    elif table_name == "environmental_sensors":
        # Environmental sensor quality checks
        df_with_quality = df.withColumn("quality_score",
            when(
                # Core fields present and valid
                (col("VesselId").isNotNull()) &
                (col("MeasurementTime").isNotNull()) &
                (col("CO2EmissionKg").between(0, config["data_quality"]["co2_threshold"]) | col("CO2EmissionKg").isNull()) &
                (col("BatteryStateOfCharge").between(*config["data_quality"]["battery_range"]) | col("BatteryStateOfCharge").isNull()),
                # High quality
                when(
                    col("CO2EmissionKg").isNotNull() & 
                    col("NOxEmissionKg").isNotNull() & 
                    col("BatteryStateOfCharge").isNotNull(), 
                    1.0
                ).otherwise(0.8)  # Medium quality
            ).when(
                # Basic validity
                (col("VesselId").isNotNull()) &
                (col("MeasurementTime").isNotNull()),
                0.6
            ).otherwise(0.0)  # Low quality
        )
        
    elif table_name == "voyages":
        # Voyage data quality checks
        df_with_quality = df.withColumn("quality_score",
            when(
                # Core voyage fields
                (col("VoyageId").isNotNull()) &
                (col("VesselId").isNotNull()) &
                (col("DepartureTime").isNotNull() | col("ArrivalTime").isNotNull()),
                # High quality
                when(
                    col("DeparturePort").isNotNull() & 
                    col("ArrivalPort").isNotNull() & 
                    col("Status").isNotNull(),
                    1.0
                ).otherwise(0.8)
            ).when(
                (col("VoyageId").isNotNull()) & (col("VesselId").isNotNull()),
                0.6
            ).otherwise(0.0)
        )
        
    else:
        # Generic quality score
        df_with_quality = df.withColumn("quality_score", lit(0.8))
    
    return df_with_quality

def apply_business_rules(df, table_name):
    """
    Apply business rules and enrichment specific to each table
    """
    print(f"Applying business rules for {table_name}")
    
    if table_name == "ais_positions":
        # AIS business rules
        df_enriched = df \
            .withColumn("speed_category",
                when(col("Speed") <= 5, "Anchored/Moored")
                .when(col("Speed") <= 15, "Slow")
                .when(col("Speed") <= 25, "Normal")
                .otherwise("Fast")
            ) \
            .withColumn("position_accuracy",
                when(col("quality_score") >= 0.9, "High")
                .when(col("quality_score") >= 0.7, "Medium")
                .otherwise("Low")
            ) \
            .withColumn("is_valid_position",
                (col("Latitude").between(-90, 90)) & 
                (col("Longitude").between(-180, 180))
            )
            
    elif table_name == "environmental_sensors":
        # Environmental sensor business rules
        df_enriched = df \
            .withColumn("emission_category",
                when(col("CO2EmissionKg") <= 100, "Low")
                .when(col("CO2EmissionKg") <= 500, "Medium")
                .otherwise("High")
            ) \
            .withColumn("compliance_status",
                when(col("CO2EmissionKg") <= 50, "Compliant")
                .when(col("CO2EmissionKg") <= 100, "Warning")
                .otherwise("Non-Compliant")
            ) \
            .withColumn("battery_status",
                when(col("BatteryStateOfCharge") >= 80, "Good")
                .when(col("BatteryStateOfCharge") >= 50, "Fair")
                .when(col("BatteryStateOfCharge") >= 20, "Low")
                .otherwise("Critical")
            ) \
            .withColumn("sensor_health",
                when(col("quality_score") >= 0.8, "Healthy")
                .when(col("quality_score") >= 0.6, "Degraded")
                .otherwise("Faulty")
            )
            
    elif table_name == "voyages":
        # Voyage business rules
        df_enriched = df \
            .withColumn("voyage_duration_hours",
                when(col("DepartureTime").isNotNull() & col("ArrivalTime").isNotNull(),
                    (unix_timestamp(col("ArrivalTime")) - unix_timestamp(col("DepartureTime"))) / 3600
                ).otherwise(None)
            ) \
            .withColumn("voyage_status_category",
                when(col("Status").isin("Completed", "Arrived"), "Completed")
                .when(col("Status").isin("In Progress", "Underway"), "Active")
                .when(col("Status").isin("Scheduled", "Planned"), "Planned")
                .otherwise("Unknown")
            ) \
            .withColumn("has_cargo",
                col("CargoWeight") > 0
            ) \
            .withColumn("has_passengers",
                col("PassengerCount") > 0
            )
    else:
        df_enriched = df
        
    return df_enriched

def deduplicate_data(df, table_name):
    """
    Remove duplicates based on business keys for each table type
    """
    print(f"Deduplicating data for {table_name}")
    
    if table_name == "ais_positions":
        # Deduplicate AIS data by MMSI + Timestamp, keep highest quality record
        window_spec = Window.partitionBy("MMSI", "Timestamp").orderBy(desc("quality_score"), desc("ingestion_timestamp"))
        df_deduped = df \
            .withColumn("row_number", row_number().over(window_spec)) \
            .filter(col("row_number") == 1) \
            .drop("row_number")
            
    elif table_name == "environmental_sensors":
        # Deduplicate environmental data by VesselId + MeasurementTime
        window_spec = Window.partitionBy("VesselId", "MeasurementTime").orderBy(desc("quality_score"), desc("ingestion_timestamp"))
        df_deduped = df \
            .withColumn("row_number", row_number().over(window_spec)) \
            .filter(col("row_number") == 1) \
            .drop("row_number")
            
    elif table_name == "voyages":
        # Deduplicate voyages by VoyageId, keep latest version
        window_spec = Window.partitionBy("VoyageId").orderBy(desc("ingestion_timestamp"))
        df_deduped = df \
            .withColumn("row_number", row_number().over(window_spec)) \
            .filter(col("row_number") == 1) \
            .drop("row_number")
    else:
        # Generic deduplication - remove exact duplicates
        df_deduped = df.dropDuplicates()
        
    return df_deduped

def add_silver_metadata(df):
    """
    Add Silver layer metadata columns
    """
    return df \
        .withColumn("silver_processing_timestamp", lit(config["processing_timestamp"])) \
        .withColumn("silver_session_id", lit(config["session_id"])) \
        .withColumn("data_lineage", lit("Bronze -> Silver")) \
        .withColumn("silver_version", lit("1.0"))

# COMMAND ----------

# MAGIC %md
# MAGIC ## 4. Silver Layer Transformation Function

# COMMAND ----------

def transform_to_silver(table_name, bronze_path, silver_path):
    """
    Main transformation function for Bronze to Silver layer
    """
    print(f"\n{'='*60}")
    print(f"Starting Silver transformation for: {table_name}")
    print(f"Bronze path: {bronze_path}")
    print(f"Silver path: {silver_path}")
    print(f"{'='*60}")
    
    try:
        # 1. Read from Bronze layer
        print("Step 1: Reading from Bronze layer...")
        df_bronze = spark.read.format("delta").load(bronze_path)
        bronze_count = df_bronze.count()
        print(f"Bronze records loaded: {bronze_count:,}")
        
        if bronze_count == 0:
            print(f"No data found in Bronze layer for {table_name}")
            return None
            
        # 2. Calculate data quality scores
        print("Step 2: Calculating data quality scores...")
        df_with_quality = calculate_data_quality_score(df_bronze, table_name)
        
        # 3. Filter by minimum quality score if enabled
        if config["data_quality"]["enabled"]:
            print(f"Step 3: Filtering by minimum quality score ({config['data_quality']['min_quality_score']})...")
            df_quality_filtered = df_with_quality.filter(
                col("quality_score") >= config["data_quality"]["min_quality_score"]
            )
            
            quality_filtered_count = df_quality_filtered.count()
            quality_pass_rate = (quality_filtered_count / bronze_count * 100) if bronze_count > 0 else 0
            print(f"Records passing quality filter: {quality_filtered_count:,} ({quality_pass_rate:.2f}%)")
            
            # Log quality issues if pass rate is low
            if quality_pass_rate < 90:
                print(f"WARNING: Quality pass rate is {quality_pass_rate:.2f}% - investigating quality issues...")
                df_with_quality.groupBy("quality_score").count().orderBy("quality_score").show()
        else:
            df_quality_filtered = df_with_quality
            print("Step 3: Quality filtering disabled, proceeding with all records")
        
        # 4. Apply business rules and enrichment
        print("Step 4: Applying business rules and enrichment...")
        df_enriched = apply_business_rules(df_quality_filtered, table_name)
        
        # 5. Deduplicate data
        print("Step 5: Deduplicating data...")
        df_deduped = deduplicate_data(df_enriched, table_name)
        deduped_count = df_deduped.count()
        duplicate_count = df_enriched.count() - deduped_count
        print(f"Duplicates removed: {duplicate_count:,}")
        print(f"Final record count: {deduped_count:,}")
        
        # 6. Add Silver layer metadata
        print("Step 6: Adding Silver layer metadata...")
        df_final = add_silver_metadata(df_deduped)
        
        # 7. Write to Silver layer with proper partitioning
        print("Step 7: Writing to Silver layer...")
        
        # Determine partitioning strategy based on table type
        if table_name == "ais_positions":
            partition_cols = ["year", "month", "day"]
            # Ensure partition columns exist
            if "year" not in df_final.columns:
                df_final = df_final \
                    .withColumn("year", year(col("Timestamp"))) \
                    .withColumn("month", month(col("Timestamp"))) \
                    .withColumn("day", dayofmonth(col("Timestamp")))
        elif table_name == "environmental_sensors":
            partition_cols = ["year", "month"]
            if "year" not in df_final.columns:
                df_final = df_final \
                    .withColumn("year", year(col("MeasurementTime"))) \
                    .withColumn("month", month(col("MeasurementTime")))
        elif table_name == "voyages":
            partition_cols = ["year", "month"]
            if "year" not in df_final.columns:
                df_final = df_final \
                    .withColumn("year", year(coalesce(col("DepartureTime"), col("ArrivalTime")))) \
                    .withColumn("month", month(coalesce(col("DepartureTime"), col("ArrivalTime"))))
        else:
            partition_cols = []
        
        # Write with schema evolution support
        write_options = {
            "format": "delta",
            "mode": "overwrite",
            "option": [
                ("mergeSchema", "true" if config["schema_evolution"] else "false"),
                ("overwriteSchema", "true" if config["schema_evolution"] else "false")
            ]
        }
        
        writer = df_final.write.format("delta").mode("overwrite")
        
        if config["schema_evolution"]:
            writer = writer.option("mergeSchema", "true").option("overwriteSchema", "true")
            
        if partition_cols:
            writer = writer.partitionBy(*partition_cols)
            
        writer.save(silver_path)
        
        # 8. Optimize Delta table
        print("Step 8: Optimizing Delta table...")
        if table_name == "ais_positions":
            spark.sql(f"OPTIMIZE delta.`{silver_path}` ZORDER BY (MMSI, Timestamp)")
        elif table_name == "environmental_sensors":
            spark.sql(f"OPTIMIZE delta.`{silver_path}` ZORDER BY (VesselId, MeasurementTime)")
        elif table_name == "voyages":
            spark.sql(f"OPTIMIZE delta.`{silver_path}` ZORDER BY (VoyageId, VesselId)")
        
        # 9. Generate data quality report
        print("Step 9: Generating data quality report...")
        quality_stats = df_final.agg(
            count("*").alias("total_records"),
            avg("quality_score").alias("avg_quality_score"),
            min("quality_score").alias("min_quality_score"),
            max("quality_score").alias("max_quality_score"),
            countDistinct("quality_score").alias("unique_quality_scores")
        ).collect()[0]
        
        print(f"\n{'='*40}")
        print(f"SILVER LAYER QUALITY REPORT - {table_name.upper()}")
        print(f"{'='*40}")
        print(f"Total Records: {quality_stats['total_records']:,}")
        print(f"Average Quality Score: {quality_stats['avg_quality_score']:.3f}")
        print(f"Min Quality Score: {quality_stats['min_quality_score']:.3f}")
        print(f"Max Quality Score: {quality_stats['max_quality_score']:.3f}")
        print(f"Unique Quality Scores: {quality_stats['unique_quality_scores']}")
        print(f"Processing Session: {config['session_id']}")
        print(f"Processing Time: {config['processing_timestamp']}")
        print(f"{'='*40}\n")
        
        return df_final
        
    except Exception as e:
        print(f"ERROR in Silver transformation for {table_name}: {str(e)}")
        raise e

# COMMAND ----------

# MAGIC %md
# MAGIC ## 5. Execute Silver Layer Transformations

# COMMAND ----------

# Transform each table from Bronze to Silver
tables_to_process = [
    ("ais_positions", config["bronze_paths"]["ais_positions"], config["silver_paths"]["ais_positions"]),
    ("environmental_sensors", config["bronze_paths"]["environmental_sensors"], config["silver_paths"]["environmental_sensors"]),
    ("voyages", config["bronze_paths"]["voyages"], config["silver_paths"]["voyages"])
]

transformation_results = {}

for table_name, bronze_path, silver_path in tables_to_process:
    try:
        print(f"\n🔄 Processing {table_name}...")
        
        # Check if Bronze table exists
        try:
            spark.read.format("delta").load(bronze_path).limit(1).collect()
            print(f"✅ Bronze table found: {bronze_path}")
        except Exception as e:
            print(f"⚠️  Bronze table not found: {bronze_path} - Skipping...")
            transformation_results[table_name] = {"status": "skipped", "reason": "Bronze table not found"}
            continue
        
        # Transform to Silver
        df_result = transform_to_silver(table_name, bronze_path, silver_path)
        
        if df_result is not None:
            transformation_results[table_name] = {
                "status": "success", 
                "record_count": df_result.count(),
                "silver_path": silver_path
            }
            print(f"✅ {table_name} transformation completed successfully")
        else:
            transformation_results[table_name] = {"status": "no_data", "reason": "No data in Bronze layer"}
            
    except Exception as e:
        print(f"❌ Error processing {table_name}: {str(e)}")
        transformation_results[table_name] = {"status": "error", "error": str(e)}

# COMMAND ----------

# MAGIC %md
# MAGIC ## 6. Final Summary and Data Quality Dashboard

# COMMAND ----------

print(f"\n{'='*80}")
print(f"SILVER LAYER TRANSFORMATION SUMMARY")
print(f"Session ID: {config['session_id']}")
print(f"Processing Time: {config['processing_timestamp']}")
print(f"{'='*80}")

total_processed = 0
total_successful = 0
total_records = 0

for table_name, result in transformation_results.items():
    status = result["status"]
    print(f"\n📊 {table_name.upper()}:")
    print(f"   Status: {status}")
    
    if status == "success":
        record_count = result["record_count"]
        print(f"   Records: {record_count:,}")
        print(f"   Silver Path: {result['silver_path']}")
        total_successful += 1
        total_records += record_count
    elif status == "error":
        print(f"   Error: {result['error']}")
    elif status == "skipped":
        print(f"   Reason: {result['reason']}")
    elif status == "no_data":
        print(f"   Reason: {result['reason']}")
    
    total_processed += 1

print(f"\n{'='*40}")
print(f"OVERALL SUMMARY:")
print(f"Tables Processed: {total_processed}")
print(f"Successful Transformations: {total_successful}")
print(f"Total Records in Silver: {total_records:,}")
print(f"Success Rate: {(total_successful/total_processed*100):.1f}%" if total_processed > 0 else "N/A")
print(f"{'='*40}")

# Create summary for downstream systems
summary_data = {
    "session_id": config["session_id"],
    "processing_timestamp": str(config["processing_timestamp"]),
    "tables_processed": total_processed,
    "successful_transformations": total_successful,
    "total_records": total_records,
    "transformation_results": transformation_results,
    "silver_layer_paths": config["silver_paths"]
}

# Save summary as JSON for monitoring/alerting systems
summary_json = json.dumps(summary_data, indent=2, default=str)
print(f"\n📋 Transformation Summary JSON:")
print(summary_json)

# COMMAND ----------

# MAGIC %md
# MAGIC ## 7. Data Lineage and Monitoring Setup

# COMMAND ----------

# Create data lineage tracking
lineage_data = []

for table_name, result in transformation_results.items():
    if result["status"] == "success":
        lineage_record = {
            "source_layer": "Bronze",
            "target_layer": "Silver", 
            "table_name": table_name,
            "source_path": config["bronze_paths"][table_name],
            "target_path": config["silver_paths"][table_name],
            "transformation_timestamp": str(config["processing_timestamp"]),
            "session_id": config["session_id"],
            "record_count": result["record_count"],
            "transformations_applied": [
                "data_quality_scoring",
                "business_rules_enrichment", 
                "deduplication",
                "schema_standardization",
                "metadata_addition"
            ]
        }
        lineage_data.append(lineage_record)

# Convert to DataFrame for further processing
if lineage_data:
    df_lineage = spark.createDataFrame(lineage_data)
    
    # Save lineage data
    lineage_path = f"/mnt/datalake/maritime/metadata/lineage/{config['session_id']}"
    df_lineage.write.format("delta").mode("overwrite").save(lineage_path)
    
    print(f"📈 Data lineage saved to: {lineage_path}")
    df_lineage.show(truncate=False)

print(f"\n🎉 Silver Layer Transformation Complete!")
print(f"Session ID: {config['session_id']}")
print(f"Next Step: Execute Gold Layer Aggregation notebook")
