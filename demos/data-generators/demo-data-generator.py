#!/usr/bin/env python3
"""
Maritime Data Generator for Interview Demo
Generates realistic maritime vessel data for real-time processing demonstration
"""

import json
import random
import time
import asyncio
from datetime import datetime, timedelta
from azure.eventhub.aio import EventHubProducerClient
from azure.eventhub import EventData
import math

class MaritimeDataGenerator:
    def __init__(self, connection_string, event_hub_name):
        self.connection_string = connection_string
        self.event_hub_name = event_hub_name
        self.vessels = self._initialize_vessels()
        
    def _initialize_vessels(self):
        """Initialize fleet of vessels with realistic data"""
        vessel_types = ['Container Ship', 'Tanker', 'Bulk Carrier', 'Cruise Ship', 'Ferry']
        norwegian_ports = [
            {'name': 'Oslo', 'lat': 59.9139, 'lon': 10.7522},
            {'name': 'Bergen', 'lat': 60.3913, 'lon': 5.3221},
            {'name': 'Trondheim', 'lat': 63.4305, 'lon': 10.3951},
            {'name': 'Stavanger', 'lat': 58.9700, 'lon': 5.7331},
            {'name': 'Tromsø', 'lat': 69.6492, 'lon': 18.9553}
        ]
        
        vessels = []
        for i in range(1, 21):  # 20 vessels for demo
            port = random.choice(norwegian_ports)
            vessel = {
                'vessel_id': f'V{i:03d}',
                'vessel_name': f'MV Maritime {i}',
                'vessel_type': random.choice(vessel_types),
                'mmsi': f'25{i:07d}',
                'imo': f'IMO{i:07d}',
                'current_lat': port['lat'] + random.uniform(-0.5, 0.5),
                'current_lon': port['lon'] + random.uniform(-0.5, 0.5),
                'speed': random.uniform(8, 15),  # knots
                'heading': random.uniform(0, 360),  # degrees
                'draft': random.uniform(8, 12),  # meters
                'length': random.uniform(150, 300),  # meters
                'width': random.uniform(20, 40),  # meters
                'destination_port': random.choice(norwegian_ports)['name'],
                'eta': datetime.now() + timedelta(hours=random.randint(6, 48))
            }
            vessels.append(vessel)
        return vessels
    
    def generate_vessel_position(self, vessel):
        """Generate realistic vessel position data"""
        # Simulate vessel movement
        speed_kmh = vessel['speed'] * 1.852  # Convert knots to km/h
        distance_per_second = speed_kmh / 3600  # km per second
        
        # Convert to degrees (approximate)
        lat_change = (distance_per_second / 111.32) * math.cos(math.radians(vessel['heading']))
        lon_change = (distance_per_second / (111.32 * math.cos(math.radians(vessel['current_lat']))))
        
        vessel['current_lat'] += lat_change
        vessel['current_lon'] += lon_change
        
        # Add some random variation
        vessel['speed'] += random.uniform(-0.5, 0.5)
        vessel['speed'] = max(0, min(25, vessel['speed']))  # Keep reasonable speed
        
        vessel['heading'] += random.uniform(-5, 5)
        vessel['heading'] = vessel['heading'] % 360
        
        return {
            'VesselId': vessel['vessel_id'],
            'VesselName': vessel['vessel_name'],
            'VesselType': vessel['vessel_type'],
            'MMSI': vessel['mmsi'],
            'IMO': vessel['imo'],
            'Latitude': round(vessel['current_lat'], 6),
            'Longitude': round(vessel['current_lon'], 6),
            'Speed': round(vessel['speed'], 2),
            'Heading': round(vessel['heading'], 1),
            'Draft': vessel['draft'],
            'Length': vessel['length'],
            'Width': vessel['width'],
            'DestinationPort': vessel['destination_port'],
            'ETA': vessel['eta'].isoformat(),
            'Timestamp': datetime.utcnow().isoformat() + 'Z'
        }
    
    def generate_engine_data(self, vessel):
        """Generate realistic engine performance data"""
        base_temp = 75  # Base engine temperature
        temp_variation = random.uniform(-5, 15)  # Temperature variation
        
        # Simulate engine issues occasionally
        if random.random() < 0.05:  # 5% chance of high temperature
            temp_variation += random.uniform(15, 25)
        
        fuel_consumption = vessel['speed'] * random.uniform(0.8, 1.2)  # Fuel based on speed
        
        return {
            'VesselId': vessel['vessel_id'],
            'VesselName': vessel['vessel_name'],
            'EngineTemp': round(base_temp + temp_variation, 1),
            'FuelConsumption': round(fuel_consumption, 2),
            'RPM': round(vessel['speed'] * 50 + random.uniform(-100, 100)),
            'OilPressure': round(random.uniform(4.5, 6.5), 1),
            'CoolantLevel': round(random.uniform(85, 100), 1),
            'Timestamp': datetime.utcnow().isoformat() + 'Z'
        }
    
    def generate_environmental_data(self, vessel):
        """Generate environmental monitoring data"""
        # Base emissions based on vessel size and speed
        base_co2 = vessel['length'] * vessel['speed'] * 0.1
        base_nox = vessel['length'] * vessel['speed'] * 0.05
        
        return {
            'VesselId': vessel['vessel_id'],
            'VesselName': vessel['vessel_name'],
            'CO2Emissions': round(base_co2 + random.uniform(-5, 5), 2),
            'NOxEmissions': round(base_nox + random.uniform(-2, 2), 2),
            'SulfurContent': round(random.uniform(0.1, 0.5), 3),
            'WaterDischarge': round(random.uniform(0, 2), 2),
            'WasteGenerated': round(random.uniform(10, 50), 1),
            'Timestamp': datetime.utcnow().isoformat() + 'Z'
        }
    
    def generate_weather_data(self, vessel):
        """Generate weather conditions around vessel"""
        return {
            'VesselId': vessel['vessel_id'],
            'Latitude': round(vessel['current_lat'], 6),
            'Longitude': round(vessel['current_lon'], 6),
            'WindSpeed': round(random.uniform(0, 25), 1),  # m/s
            'WindDirection': round(random.uniform(0, 360), 1),
            'WaveHeight': round(random.uniform(0.5, 4.0), 1),  # meters
            'Visibility': round(random.uniform(1, 10), 1),  # nautical miles
            'AirTemperature': round(random.uniform(-5, 20), 1),  # Celsius
            'SeaTemperature': round(random.uniform(4, 15), 1),  # Celsius
            'Barometer': round(random.uniform(980, 1030), 1),  # hPa
            'Timestamp': datetime.utcnow().isoformat() + 'Z'
        }
    
    async def send_data_to_event_hub(self, data_batch):
        """Send data to Azure Event Hub"""
        producer = EventHubProducerClient.from_connection_string(
            conn_str=self.connection_string,
            eventhub_name=self.event_hub_name
        )
        
        try:
            async with producer:
                event_data_batch = await producer.create_batch()
                
                for data in data_batch:
                    event_data = EventData(json.dumps(data))
                    try:
                        event_data_batch.add(event_data)
                    except ValueError:
                        # Batch is full, send and create new batch
                        await producer.send_batch(event_data_batch)
                        event_data_batch = await producer.create_batch()
                        event_data_batch.add(event_data)
                
                # Send remaining events
                if len(event_data_batch) > 0:
                    await producer.send_batch(event_data_batch)
                    
        except Exception as e:
            print(f"Error sending data to Event Hub: {e}")
    
    async def generate_continuous_data(self, duration_minutes=60):
        """Generate continuous maritime data for demo"""
        print(f"🚢 Starting Maritime Data Generator for {duration_minutes} minutes...")
        print(f"📊 Generating data for {len(self.vessels)} vessels")
        
        end_time = datetime.now() + timedelta(minutes=duration_minutes)
        
        while datetime.now() < end_time:
            data_batch = []
            
            for vessel in self.vessels:
                # Generate different types of data
                position_data = self.generate_vessel_position(vessel)
                engine_data = self.generate_engine_data(vessel)
                environmental_data = self.generate_environmental_data(vessel)
                weather_data = self.generate_weather_data(vessel)
                
                # Combine all data
                combined_data = {**position_data, **engine_data, **environmental_data, **weather_data}
                data_batch.append(combined_data)
                
                # Occasionally generate alerts
                if random.random() < 0.02:  # 2% chance
                    alert_data = {
                        'MessageType': 'ALERT',
                        'VesselId': vessel['vessel_id'],
                        'AlertType': random.choice(['MAINTENANCE', 'NAVIGATION', 'SAFETY', 'ENVIRONMENT']),
                        'Severity': random.choice(['LOW', 'MEDIUM', 'HIGH', 'CRITICAL']),
                        'Message': 'Demo alert for interview presentation',
                        'Timestamp': datetime.utcnow().isoformat() + 'Z'
                    }
                    data_batch.append(alert_data)
            
            # Send batch to Event Hub
            await self.send_data_to_event_hub(data_batch)
            
            print(f"📡 Sent {len(data_batch)} maritime data points at {datetime.now().strftime('%H:%M:%S')}")
            
            # Wait before next batch (adjust for desired throughput)
            await asyncio.sleep(5)  # Send data every 5 seconds
        
        print("✅ Maritime data generation completed!")

def main():
    """Main function for demo data generation"""
    # These would be your actual Azure Event Hub connection details
    # For interview demo, you'll replace these with your actual values
    
    connection_string = "Endpoint=sb://your-eventhub-namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=your-key"
    event_hub_name = "maritime-data"
    
    print("🌊 Maritime Platform - Interview Demo Data Generator 🌊")
    print("=" * 60)
    
    generator = MaritimeDataGenerator(connection_string, event_hub_name)
    
    # Run the data generator
    try:
        asyncio.run(generator.generate_continuous_data(duration_minutes=30))
    except KeyboardInterrupt:
        print("\n⏹️  Data generation stopped by user")
    except Exception as e:
        print(f"❌ Error: {e}")

if __name__ == "__main__":
    main()