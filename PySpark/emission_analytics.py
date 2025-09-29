"""
Maritime Emissions Analytics with PySpark
==========================================

Advanced time-series analysis of vessel emissions:
- CO2, NOx, SOx emission calculations
- Compliance monitoring and reporting
- Trend analysis and forecasting
- Fleet-wide environmental KPIs
"""

from pyspark.sql import SparkSession
from pyspark.sql.functions import *
from pyspark.sql.types import *
from pyspark.sql import Window
import argparse
import logging

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)


class EmissionAnalytics:
    """PySpark-based emissions analytics processor"""
    
    def __init__(self):
        self.spark = SparkSession.builder \
            .appName("Maritime-Emission-Analytics") \
            .config("spark.sql.adaptive.enabled", "true") \
            .getOrCreate()
        
        # IMO 2030 emission targets (simplified)
        self.IMO_TARGETS = {
            "co2_max_kg_per_hour": 50.0,
            "nox_max_kg_per_hour": 5.0,
            "sox_max_kg_per_hour": 1.5
        }
        
        logger.info("✅ Emission Analytics initialized")
    
    def load_environmental_data(self, path):
        """Load environmental sensor data"""
        logger.info(f"📥 Loading environmental data from {path}")
        df = self.spark.read.format("delta").load(path)
        logger.info(f"✅ Loaded {df.count():,} sensor readings")
        return df
    
    def calculate_compliance_metrics(self, df_env):
        """
        Calculate emissions compliance against IMO standards
        """
        logger.info("🌍 Calculating compliance metrics...")
        
        df_compliance = df_env \
            .withColumn("co2_compliant",
                when(col("CO2EmissionKg") <= self.IMO_TARGETS["co2_max_kg_per_hour"], 1).otherwise(0)
            ) \
            .withColumn("nox_compliant",
                when(col("NOxEmissionKg") <= self.IMO_TARGETS["nox_max_kg_per_hour"], 1).otherwise(0)
            ) \
            .withColumn("sox_compliant",
                when(col("SOxEmissionKg") <= self.IMO_TARGETS["sox_max_kg_per_hour"], 1).otherwise(0)
            ) \
            .withColumn("fully_compliant",
                when(
                    (col("co2_compliant") == 1) &
                    (col("nox_compliant") == 1) &
                    (col("sox_compliant") == 1), 1
                ).otherwise(0)
            ) \
            .withColumn("emission_category",
                when(col("CO2EmissionKg") < 20, "Excellent")
                .when(col("CO2EmissionKg") < 40, "Good")
                .when(col("CO2EmissionKg") < 50, "Acceptable")
                .otherwise("Needs Improvement")
            )
        
        # Calculate compliance rates
        compliance_summary = df_compliance.agg(
            (sum("co2_compliant") / count("*") * 100).alias("co2_compliance_rate"),
            (sum("nox_compliant") / count("*") * 100).alias("nox_compliance_rate"),
            (sum("sox_compliant") / count("*") * 100).alias("sox_compliance_rate"),
            (sum("fully_compliant") / count("*") * 100).alias("full_compliance_rate")
        ).collect()[0]
        
        logger.info(f"\n{'='*60}")
        logger.info("COMPLIANCE SUMMARY")
        logger.info(f"{'='*60}")
        logger.info(f"CO2 Compliance: {compliance_summary['co2_compliance_rate']:.2f}%")
        logger.info(f"NOx Compliance: {compliance_summary['nox_compliance_rate']:.2f}%")
        logger.info(f"SOx Compliance: {compliance_summary['sox_compliance_rate']:.2f}%")
        logger.info(f"Full Compliance: {compliance_summary['full_compliance_rate']:.2f}%")
        logger.info(f"{'='*60}\n")
        
        return df_compliance
    
    def calculate_rolling_averages(self, df_compliance):
        """
        Calculate 7-day and 30-day rolling emission averages
        """
        logger.info("📊 Calculating rolling averages...")
        
        # Convert to daily aggregates first
        df_daily = df_compliance \
            .withColumn("date", to_date("MeasurementTime")) \
            .groupBy("VesselId", "date") \
            .agg(
                avg("CO2EmissionKg").alias("daily_avg_co2"),
                avg("NOxEmissionKg").alias("daily_avg_nox"),
                avg("SOxEmissionKg").alias("daily_avg_sox"),
                avg("FuelConsumptionLiters").alias("daily_avg_fuel")
            )
        
        # Define windows for rolling calculations
        window_7day = Window.partitionBy("VesselId").orderBy("date").rowsBetween(-6, 0)
        window_30day = Window.partitionBy("VesselId").orderBy("date").rowsBetween(-29, 0)
        
        df_with_trends = df_daily \
            .withColumn("co2_7day_avg", avg("daily_avg_co2").over(window_7day)) \
            .withColumn("co2_30day_avg", avg("daily_avg_co2").over(window_30day)) \
            .withColumn("co2_trend",
                when(col("daily_avg_co2") > col("co2_7day_avg") * 1.15, "Significantly Increasing")
                .when(col("daily_avg_co2") > col("co2_7day_avg") * 1.05, "Increasing")
                .when(col("daily_avg_co2") < col("co2_7day_avg") * 0.85, "Significantly Decreasing")
                .when(col("daily_avg_co2") < col("co2_7day_avg") * 0.95, "Decreasing")
                .otherwise("Stable")
            )
        
        logger.info("✅ Rolling averages calculated")
        return df_with_trends
    
    def identify_high_emitters(self, df_compliance):
        """
        Identify vessels with consistently high emissions
        """
        logger.info("🔍 Identifying high emitters...")
        
        df_vessel_emissions = df_compliance \
            .groupBy("VesselId") \
            .agg(
                avg("CO2EmissionKg").alias("avg_co2"),
                max("CO2EmissionKg").alias("max_co2"),
                avg("NOxEmissionKg").alias("avg_nox"),
                avg("SOxEmissionKg").alias("avg_sox"),
                (sum("fully_compliant") / count("*") * 100).alias("compliance_rate"),
                count("*").alias("measurement_count")
            ) \
            .withColumn("emission_score",
                round(
                    (col("avg_co2") / self.IMO_TARGETS["co2_max_kg_per_hour"]) * 100,
                    2
                )
            ) \
            .withColumn("status",
                when(col("emission_score") > 120, "Critical - Action Required")
                .when(col("emission_score") > 100, "High - Monitor Closely")
                .when(col("emission_score") > 80, "Moderate")
                .otherwise("Good")
            ) \
            .orderBy(desc("emission_score"))
        
        # Show high emitters
        high_emitters = df_vessel_emissions.filter(col("emission_score") > 100)
        
        if high_emitters.count() > 0:
            logger.warning(f"⚠️  {high_emitters.count()} vessels exceed emission targets")
            logger.info("\nHigh Emitter Vessels:")
            high_emitters.select(
                "VesselId", "avg_co2", "emission_score", "status", "compliance_rate"
            ).show(truncate=False)
        else:
            logger.info("✅ All vessels within emission targets")
        
        return df_vessel_emissions
    
    def calculate_fleet_carbon_footprint(self, df_compliance):
        """
        Calculate total fleet carbon footprint with projections
        """
        logger.info("🌍 Calculating fleet carbon footprint...")
        
        # Total emissions to date
        totals = df_compliance.agg(
            sum("CO2EmissionKg").alias("total_co2_kg"),
            sum("NOxEmissionKg").alias("total_nox_kg"),
            sum("SOxEmissionKg").alias("total_sox_kg"),
            sum("FuelConsumptionLiters").alias("total_fuel_liters")
        ).collect()[0]
        
        # Convert to tons
        total_co2_tons = totals["total_co2_kg"] / 1000
        total_nox_tons = totals["total_nox_kg"] / 1000
        total_sox_tons = totals["total_sox_kg"] / 1000
        
        # Calculate CO2 equivalent of other gases (simplified conversion)
        co2_equivalent_nox = total_nox_tons * 298  # NOx has 298x GWP of CO2
        co2_equivalent_sox = total_sox_tons * 1      # SOx approximately equal
        
        total_co2_equivalent = total_co2_tons + co2_equivalent_nox + co2_equivalent_sox
        
        logger.info(f"\n{'='*60}")
        logger.info("FLEET CARBON FOOTPRINT")
        logger.info(f"{'='*60}")
        logger.info(f"Total CO2: {total_co2_tons:,.2f} tons")
        logger.info(f"Total NOx: {total_nox_tons:,.2f} tons (CO2-eq: {co2_equivalent_nox:,.2f} tons)")
        logger.info(f"Total SOx: {total_sox_tons:,.2f} tons")
        logger.info(f"Total CO2 Equivalent: {total_co2_equivalent:,.2f} tons")
        logger.info(f"Total Fuel: {totals['total_fuel_liters']:,.2f} liters")
        logger.info(f"{'='*60}\n")
        
        return {
            "co2_tons": total_co2_tons,
            "nox_tons": total_nox_tons,
            "sox_tons": total_sox_tons,
            "co2_equivalent_tons": total_co2_equivalent,
            "fuel_liters": totals["total_fuel_liters"]
        }
    
    def save_analytics_results(self, df, output_path, partition_by=None):
        """Save results to Delta Lake"""
        logger.info(f"💾 Saving results to {output_path}")
        
        writer = df.write.format("delta").mode("overwrite")
        
        if partition_by:
            writer = writer.partitionBy(partition_by)
        
        writer.save(output_path)
        logger.info(f"✅ Saved {df.count():,} records")
    
    def run_emission_analytics(self, input_path, output_base_path):
        """Execute complete emissions analytics pipeline"""
        logger.info("="*80)
        logger.info("MARITIME EMISSIONS ANALYTICS - STARTING")
        logger.info("="*80)
        
        # Load data
        df_env = self.load_environmental_data(input_path)
        
        # Calculate compliance
        df_compliance = self.calculate_compliance_metrics(df_env)
        self.save_analytics_results(
            df_compliance,
            f"{output_base_path}/compliance_details"
        )
        
        # Rolling averages
        df_trends = self.calculate_rolling_averages(df_compliance)
        self.save_analytics_results(
            df_trends,
            f"{output_base_path}/emission_trends",
            partition_by=["VesselId"]
        )
        
        # High emitters
        df_emitters = self.identify_high_emitters(df_compliance)
        self.save_analytics_results(
            df_emitters,
            f"{output_base_path}/vessel_emission_scores"
        )
        
        # Carbon footprint
        footprint = self.calculate_fleet_carbon_footprint(df_compliance)
        
        logger.info("="*80)
        logger.info("✅ EMISSION ANALYTICS COMPLETED")
        logger.info("="*80)
        
        return footprint
    
    def stop(self):
        self.spark.stop()


def main():
    parser = argparse.ArgumentParser(description="Maritime Emission Analytics")
    parser.add_argument("--input", required=True, help="Input Delta Lake path")
    parser.add_argument("--output", required=True, help="Output base path")
    args = parser.parse_args()
    
    analytics = EmissionAnalytics()
    
    try:
        result = analytics.run_emission_analytics(args.input, args.output)
        print("\n✅ Analytics complete!")
        print(f"Total CO2 Equivalent: {result['co2_equivalent_tons']:,.2f} tons")
    finally:
        analytics.stop()


if __name__ == "__main__":
    main()