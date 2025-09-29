"""
Maritime Voyage Batch Processing with PySpark
==============================================

This script processes historical voyage data in batch mode:
- Aggregates voyage performance metrics
- Calculates route efficiency
- Generates fleet-wide statistics
- Produces daily/weekly/monthly reports

Can be scheduled to run via cron, Airflow, or Databricks Jobs
"""

from pyspark.sql import SparkSession
from pyspark.sql.functions import *
from pyspark.sql.types import *
from pyspark.sql import Window
from datetime import datetime, timedelta
import argparse
import logging

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)


class MaritimeVoyageBatchProcessor:
    """
    Batch processor for maritime voyage data using PySpark
    """
    
    def __init__(self, spark_master="local[*]", app_name="Maritime-Voyage-Batch"):
        """Initialize Spark session with optimized configuration"""
        logger.info("Initializing Spark session...")
        
        self.spark = SparkSession.builder \
            .appName(app_name) \
            .master(spark_master) \
            .config("spark.sql.adaptive.enabled", "true") \
            .config("spark.sql.adaptive.coalescePartitions.enabled", "true") \
            .config("spark.sql.shuffle.partitions", "200") \
            .config("spark.default.parallelism", "100") \
            .config("spark.sql.extensions", "io.delta.sql.DeltaSparkSessionExtension") \
            .config("spark.sql.catalog.spark_catalog", "org.apache.spark.sql.delta.catalog.DeltaCatalog") \
            .getOrCreate()
        
        self.spark.sparkContext.setLogLevel("WARN")
        logger.info(f"✅ Spark session initialized - Version: {self.spark.version}")
    
    def load_voyage_data(self, input_path, start_date=None, end_date=None):
        """
        Load voyage data from Delta Lake or Parquet
        """
        logger.info(f"📥 Loading voyage data from {input_path}")
        
        # Read data
        df = self.spark.read.format("delta").load(input_path)
        
        # Apply date filter if provided
        if start_date:
            df = df.filter(col("DepartureTime") >= start_date)
        if end_date:
            df = df.filter(col("DepartureTime") <= end_date)
        
        record_count = df.count()
        logger.info(f"✅ Loaded {record_count:,} voyage records")
        
        return df
    
    def calculate_voyage_metrics(self, df_voyages):
        """
        Calculate comprehensive voyage performance metrics
        """
        logger.info("📊 Calculating voyage metrics...")
        
        # Calculate voyage duration
        df_with_metrics = df_voyages \
            .withColumn("duration_hours",
                (unix_timestamp("ArrivalTime") - unix_timestamp("DepartureTime")) / 3600
            ) \
            .withColumn("voyage_date", to_date("DepartureTime")) \
            .withColumn("week_of_year", weekofyear("DepartureTime")) \
            .withColumn("month", month("DepartureTime")) \
            .withColumn("year", year("DepartureTime")) \
            .withColumn("is_delayed",
                when(col("Status").isin(["Delayed", "Late Arrival"]), 1).otherwise(0)
            ) \
            .withColumn("passenger_load_factor",
                (col("PassengerCount") / 600.0 * 100).cast("double")  # Assuming 600 capacity
            )
        
        logger.info("✅ Voyage metrics calculated")
        return df_with_metrics
    
    def aggregate_route_performance(self, df_metrics):
        """
        Aggregate performance metrics by route
        """
        logger.info("🛤️  Aggregating route performance...")
        
        df_route_stats = df_metrics \
            .groupBy("DeparturePort", "ArrivalPort") \
            .agg(
                count("*").alias("total_voyages"),
                avg("duration_hours").alias("avg_duration_hours"),
                min("duration_hours").alias("min_duration_hours"),
                max("duration_hours").alias("max_duration_hours"),
                sum("is_delayed").alias("delayed_count"),
                (sum("is_delayed") / count("*") * 100).alias("delay_rate_pct"),
                avg("PassengerCount").alias("avg_passengers"),
                sum("PassengerCount").alias("total_passengers"),
                avg("passenger_load_factor").alias("avg_load_factor"),
                sum("CargoWeight").alias("total_cargo_kg")
            ) \
            .withColumn("route_name",
                concat(col("DeparturePort"), lit(" → "), col("ArrivalPort"))
            ) \
            .orderBy(desc("total_voyages"))
        
        logger.info(f"✅ Route aggregation complete - {df_route_stats.count()} routes analyzed")
        return df_route_stats
    
    def aggregate_vessel_performance(self, df_metrics):
        """
        Aggregate performance metrics by vessel
        """
        logger.info("🚢 Aggregating vessel performance...")
        
        df_vessel_stats = df_metrics \
            .groupBy("VesselId") \
            .agg(
                count("*").alias("total_voyages"),
                avg("duration_hours").alias("avg_voyage_duration"),
                sum("is_delayed").alias("total_delays"),
                (sum("is_delayed") / count("*") * 100).alias("delay_rate"),
                avg("PassengerCount").alias("avg_occupancy"),
                sum("PassengerCount").alias("total_passengers_carried"),
                avg("passenger_load_factor").alias("avg_load_factor")
            ) \
            .withColumn("performance_score",
                round((100 - col("delay_rate")) * (col("avg_load_factor") / 100), 2)
            ) \
            .orderBy(desc("performance_score"))
        
        logger.info(f"✅ Vessel aggregation complete - {df_vessel_stats.count()} vessels analyzed")
        return df_vessel_stats
    
    def generate_time_series_aggregations(self, df_metrics):
        """
        Generate daily, weekly, and monthly time-series aggregations
        """
        logger.info("📅 Generating time-series aggregations...")
        
        # Daily aggregations
        df_daily = df_metrics \
            .groupBy("voyage_date") \
            .agg(
                count("*").alias("voyages_count"),
                sum("PassengerCount").alias("total_passengers"),
                avg("duration_hours").alias("avg_duration"),
                (sum("is_delayed") / count("*") * 100).alias("delay_rate"),
                avg("passenger_load_factor").alias("avg_load_factor")
            ) \
            .withColumn("period_type", lit("daily")) \
            .orderBy("voyage_date")
        
        # Weekly aggregations
        df_weekly = df_metrics \
            .groupBy("year", "week_of_year") \
            .agg(
                count("*").alias("voyages_count"),
                sum("PassengerCount").alias("total_passengers"),
                avg("duration_hours").alias("avg_duration"),
                (sum("is_delayed") / count("*") * 100).alias("delay_rate"),
                avg("passenger_load_factor").alias("avg_load_factor")
            ) \
            .withColumn("period_type", lit("weekly")) \
            .orderBy("year", "week_of_year")
        
        # Monthly aggregations
        df_monthly = df_metrics \
            .groupBy("year", "month") \
            .agg(
                count("*").alias("voyages_count"),
                sum("PassengerCount").alias("total_passengers"),
                avg("duration_hours").alias("avg_duration"),
                (sum("is_delayed") / count("*") * 100).alias("delay_rate"),
                avg("passenger_load_factor").alias("avg_load_factor")
            ) \
            .withColumn("period_type", lit("monthly")) \
            .orderBy("year", "month")
        
        logger.info("✅ Time-series aggregations complete")
        return df_daily, df_weekly, df_monthly
    
    def identify_anomalies(self, df_metrics):
        """
        Identify voyage anomalies using statistical methods
        """
        logger.info("🔍 Identifying anomalies...")
        
        # Calculate statistical bounds
        stats = df_metrics.agg(
            avg("duration_hours").alias("mean_duration"),
            stddev("duration_hours").alias("std_duration")
        ).collect()[0]
        
        mean_duration = stats["mean_duration"]
        std_duration = stats["std_duration"]
        
        # Flag anomalies (>3 standard deviations)
        df_with_anomalies = df_metrics \
            .withColumn("duration_zscore",
                abs((col("duration_hours") - lit(mean_duration)) / lit(std_duration))
            ) \
            .withColumn("is_anomaly",
                when(col("duration_zscore") > 3, 1).otherwise(0)
            )
        
        anomaly_count = df_with_anomalies.filter(col("is_anomaly") == 1).count()
        logger.info(f"⚠️  Identified {anomaly_count} anomalous voyages")
        
        return df_with_anomalies
    
    def save_results(self, dataframe, output_path, mode="overwrite"):
        """
        Save results to Delta Lake
        """
        logger.info(f"💾 Saving results to {output_path}")
        
        dataframe.write \
            .format("delta") \
            .mode(mode) \
            .option("overwriteSchema", "true") \
            .save(output_path)
        
        logger.info(f"✅ Results saved successfully - {dataframe.count():,} records")
    
    def run_batch_processing(self, input_path, output_base_path, start_date=None, end_date=None):
        """
        Execute complete batch processing pipeline
        """
        logger.info("="*80)
        logger.info("MARITIME VOYAGE BATCH PROCESSING - STARTING")
        logger.info("="*80)
        
        try:
            # Load data
            df_voyages = self.load_voyage_data(input_path, start_date, end_date)
            
            # Calculate metrics
            df_metrics = self.calculate_voyage_metrics(df_voyages)
            
            # Aggregate by route
            df_route_stats = self.aggregate_route_performance(df_metrics)
            self.save_results(df_route_stats, f"{output_base_path}/route_performance")
            
            # Aggregate by vessel
            df_vessel_stats = self.aggregate_vessel_performance(df_metrics)
            self.save_results(df_vessel_stats, f"{output_base_path}/vessel_performance")
            
            # Generate time-series
            df_daily, df_weekly, df_monthly = self.generate_time_series_aggregations(df_metrics)
            self.save_results(df_daily, f"{output_base_path}/daily_metrics")
            self.save_results(df_weekly, f"{output_base_path}/weekly_metrics")
            self.save_results(df_monthly, f"{output_base_path}/monthly_metrics")
            
            # Identify anomalies
            df_anomalies = self.identify_anomalies(df_metrics)
            df_anomaly_records = df_anomalies.filter(col("is_anomaly") == 1)
            if df_anomaly_records.count() > 0:
                self.save_results(df_anomaly_records, f"{output_base_path}/anomalies")
            
            logger.info("="*80)
            logger.info("✅ BATCH PROCESSING COMPLETED SUCCESSFULLY")
            logger.info("="*80)
            
            # Summary statistics
            print("\n📊 PROCESSING SUMMARY:")
            print(f"   Total Voyages Processed: {df_voyages.count():,}")
            print(f"   Routes Analyzed: {df_route_stats.count()}")
            print(f"   Vessels Analyzed: {df_vessel_stats.count()}")
            print(f"   Anomalies Detected: {df_anomaly_records.count()}")
            print(f"   Date Range: {start_date or 'All'} to {end_date or 'All'}")
            
            return {
                "status": "success",
                "records_processed": df_voyages.count(),
                "routes": df_route_stats.count(),
                "vessels": df_vessel_stats.count(),
                "anomalies": df_anomaly_records.count()
            }
            
        except Exception as e:
            logger.error(f"❌ Batch processing failed: {str(e)}", exc_info=True)
            return {"status": "failed", "error": str(e)}
    
    def stop(self):
        """Stop Spark session"""
        self.spark.stop()
        logger.info("🛑 Spark session stopped")


def main():
    """Main entry point for batch processing"""
    parser = argparse.ArgumentParser(description="Maritime Voyage Batch Processing")
    parser.add_argument("--input", required=True, help="Input Delta Lake path")
    parser.add_argument("--output", required=True, help="Output base path")
    parser.add_argument("--start-date", help="Start date (YYYY-MM-DD)")
    parser.add_argument("--end-date", help="End date (YYYY-MM-DD)")
    parser.add_argument("--spark-master", default="local[*]", help="Spark master URL")
    
    args = parser.parse_args()
    
    # Initialize processor
    processor = MaritimeVoyageBatchProcessor(spark_master=args.spark_master)
    
    try:
        # Run processing
        result = processor.run_batch_processing(
            input_path=args.input,
            output_base_path=args.output,
            start_date=args.start_date,
            end_date=args.end_date
        )
        
        # Print result
        print("\n" + "="*80)
        print("BATCH PROCESSING RESULT:")
        print(f"Status: {result['status'].upper()}")
        if result['status'] == 'success':
            print(f"Records Processed: {result['records_processed']:,}")
        print("="*80)
        
    finally:
        # Clean up
        processor.stop()


if __name__ == "__main__":
    main()