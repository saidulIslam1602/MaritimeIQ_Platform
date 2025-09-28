// Azure Event Hubs integration for real-time maritime data ingestion
import { EventHubProducerClient, EventHubConsumerClient, type Subscription } from '@azure/event-hubs'

export interface MaritimeTelemetry {
  vesselId: number
  timestamp: string
  position: {
    latitude: number
    longitude: number
    heading: number
    speed: number
  }
  environmental: {
    co2Level: number
    noxLevel: number
    fuelConsumption: number
    batteryLevel: number
  }
  operational: {
    passengerCount: number
    crewCount: number
    routeProgress: number
    nextPort: string
    eta: string
  }
  sensors: {
    engineTemperature: number
    vibration: number
    fuelPressure: number
    batteryVoltage: number
  }
}

export class MaritimeEventHubService {
  private producerClient: EventHubProducerClient | null = null
  private consumerClient: EventHubConsumerClient | null = null
  private consumerSubscription: Subscription | null = null
  private isProducing = false
  private isConsuming = false

  constructor(
    private connectionString: string,
    private eventHubName: string,
    private consumerGroup: string = '$Default'
  ) {}

  // Initialize producer for sending telemetry data
  async initializeProducer(): Promise<void> {
    try {
      this.producerClient = new EventHubProducerClient(this.connectionString, this.eventHubName)
      console.log('Event Hub producer initialized')
    } catch (error) {
      console.error('Failed to initialize Event Hub producer:', error)
      throw error
    }
  }

  // Initialize consumer for receiving telemetry data
  async initializeConsumer(): Promise<void> {
    try {
      this.consumerClient = new EventHubConsumerClient(
        this.consumerGroup,
        this.connectionString,
        this.eventHubName
      )
      console.log('Event Hub consumer initialized')
    } catch (error) {
      console.error('Failed to initialize Event Hub consumer:', error)
      throw error
    }
  }

  // Send maritime telemetry data to Event Hub
  async sendTelemetry(telemetryData: MaritimeTelemetry[]): Promise<void> {
    if (!this.producerClient) {
      await this.initializeProducer()
    }

    try {
      const eventDataBatch = await this.producerClient!.createBatch()
      
      for (const data of telemetryData) {
        const eventData = {
          body: JSON.stringify(data),
          properties: {
            vesselId: data.vesselId.toString(),
            timestamp: data.timestamp,
            type: 'maritime-telemetry'
          }
        }
        
        if (!eventDataBatch.tryAdd(eventData)) {
          // Send current batch if it's full
          await this.producerClient!.sendBatch(eventDataBatch)
          
          // Create new batch for remaining data
          const newBatch = await this.producerClient!.createBatch()
          newBatch.tryAdd(eventData)
          await this.producerClient!.sendBatch(newBatch)
        }
      }
      
      // Send the final batch
      if (eventDataBatch.count > 0) {
        await this.producerClient!.sendBatch(eventDataBatch)
      }
      
      console.log(`Sent ${telemetryData.length} telemetry events to Event Hub`)
    } catch (error) {
      console.error('Failed to send telemetry data:', error)
      throw error
    }
  }

  // Start consuming real-time telemetry data
  async startConsuming(onData: (data: MaritimeTelemetry) => void): Promise<Subscription | null> {
    if (!this.consumerClient) {
      await this.initializeConsumer()
    }

    if (this.isConsuming && this.consumerSubscription) {
      console.log('Already consuming events')
      return this.consumerSubscription
    }

    this.isConsuming = true
    
    try {
      console.log('Starting to consume events from Event Hub...')

      const subscription = this.consumerClient!.subscribe({
        processEvents: async (events, context) => {
          for (const event of events) {
            try {
              const telemetryData: MaritimeTelemetry = JSON.parse(event.body as string)
              onData(telemetryData)
            } catch (error) {
              console.error('Failed to parse telemetry data:', error)
            }
          }
          
          // Update checkpoint
          await context.updateCheckpoint(events[events.length - 1])
        },
        processError: async (err, context) => {
          console.error('Error processing events:', err)
        }
      })

      this.consumerSubscription = subscription
      console.log('Event Hub consumer started successfully')
      return subscription
    } catch (error) {
      console.error('Failed to start consuming events:', error)
      this.isConsuming = false
      throw error
    }
  }

  // Generate simulated maritime telemetry data
  generateSimulatedTelemetry(vesselIds: number[]): MaritimeTelemetry[] {
    return vesselIds.map(vesselId => ({
      vesselId,
      timestamp: new Date().toISOString(),
      position: {
        latitude: 59.9139 + (Math.random() - 0.5) * 10, // Around Norway
        longitude: 10.7522 + (Math.random() - 0.5) * 20,
        heading: Math.random() * 360,
        speed: 12 + Math.random() * 8 // 12-20 knots typical cruise speed
      },
      environmental: {
        co2Level: 800 + Math.random() * 400, // ppm
        noxLevel: 15 + Math.random() * 10, // ppm
        fuelConsumption: 200 + Math.random() * 100, // liters/hour
        batteryLevel: 60 + Math.random() * 40 // percentage
      },
      operational: {
        passengerCount: Math.floor(Math.random() * 640), // Max capacity
        crewCount: 50 + Math.floor(Math.random() * 10),
        routeProgress: Math.random() * 100, // percentage
        nextPort: ['Bergen', 'Trondheim', 'Bodø', 'Tromsø', 'Kirkenes'][Math.floor(Math.random() * 5)],
        eta: new Date(Date.now() + Math.random() * 24 * 60 * 60 * 1000).toISOString()
      },
      sensors: {
        engineTemperature: 80 + Math.random() * 20, // Celsius
        vibration: Math.random() * 5, // mm/s
        fuelPressure: 3 + Math.random() * 2, // bar
        batteryVoltage: 48 + Math.random() * 4 // volts
      }
    }))
  }

  // Start simulated data generation
  async startDataSimulation(vesselIds: number[], intervalMs: number = 30000): Promise<void> {
    if (this.isProducing) {
      console.log('Data simulation already running')
      return
    }

    this.isProducing = true
    console.log(`Starting data simulation for vessels: ${vesselIds.join(', ')}`)

    const simulateData = async () => {
      if (!this.isProducing) return

      try {
        const telemetryData = this.generateSimulatedTelemetry(vesselIds)
        await this.sendTelemetry(telemetryData)
        console.log(`Generated and sent telemetry for ${vesselIds.length} vessels`)
      } catch (error) {
        console.error('Error in data simulation:', error)
      }

      if (this.isProducing) {
        setTimeout(simulateData, intervalMs)
      }
    }

    // Start simulation
    simulateData()
  }

  // Stop data simulation
  stopDataSimulation(): void {
    this.isProducing = false
    console.log('Data simulation stopped')
  }

  // Stop consuming events
  async stopConsuming(): Promise<void> {
    this.isConsuming = false

    if (this.consumerSubscription) {
      await this.consumerSubscription.close()
      this.consumerSubscription = null
    }

    console.log('Event consumption stopped')
  }

  // Clean up resources
  async dispose(): Promise<void> {
    this.stopDataSimulation()
    await this.stopConsuming()

    if (this.producerClient) {
      await this.producerClient.close()
      this.producerClient = null
    }

    if (this.consumerClient) {
      await this.consumerClient.close()
      this.consumerClient = null
    }

    console.log('Event Hub service disposed')
  }
}

// Azure Stream Analytics query templates for maritime data processing
export const streamAnalyticsQueries = {
  // Real-time environmental compliance monitoring
  environmentalCompliance: `
    SELECT
        vesselId,
        AVG(environmental.co2Level) as avgCO2,
        MAX(environmental.co2Level) as maxCO2,
        AVG(environmental.noxLevel) as avgNOx,
        MAX(environmental.noxLevel) as maxNOx,
        System.Timestamp() as windowEnd
    INTO [environmental-alerts]
    FROM [maritime-telemetry]
    WHERE environmental.co2Level > 1200 OR environmental.noxLevel > 25
    GROUP BY vesselId, TumblingWindow(minute, 5)
  `,

  // Fuel efficiency analytics
  fuelEfficiency: `
    SELECT
        vesselId,
        AVG(environmental.fuelConsumption / position.speed) as fuelEfficiencyRatio,
        AVG(environmental.fuelConsumption) as avgFuelConsumption,
        AVG(position.speed) as avgSpeed,
        System.Timestamp() as windowEnd
    INTO [fuel-analytics]
    FROM [maritime-telemetry]
    WHERE position.speed > 0
    GROUP BY vesselId, TumblingWindow(hour, 1)
  `,

  // Route optimization insights
  routeOptimization: `
    SELECT
        vesselId,
        operational.nextPort,
        AVG(position.speed) as avgSpeed,
        AVG(operational.routeProgress) as avgProgress,
        DATEDIFF(minute, MIN(timestamp), MAX(timestamp)) as timeWindow,
        System.Timestamp() as windowEnd
    INTO [route-optimization]
    FROM [maritime-telemetry]
    GROUP BY vesselId, operational.nextPort, TumblingWindow(hour, 1)
  `,

  // Predictive maintenance alerts
  predictiveMaintenance: `
    SELECT
        vesselId,
        AVG(sensors.engineTemperature) as avgEngineTemp,
        MAX(sensors.engineTemperature) as maxEngineTemp,
        AVG(sensors.vibration) as avgVibration,
        MAX(sensors.vibration) as maxVibration,
        System.Timestamp() as windowEnd
    INTO [maintenance-alerts]
    FROM [maritime-telemetry]
    WHERE sensors.engineTemperature > 95 OR sensors.vibration > 4
    GROUP BY vesselId, TumblingWindow(minute, 10)
  `
}

// Configuration for Azure services
export const azureConfig = {
  eventHub: {
    connectionString: process.env.AZURE_EVENTHUB_CONNECTION_STRING || '',
    eventHubName: 'maritime-telemetry',
    consumerGroup: '$Default'
  },
  streamAnalytics: {
    jobName: 'maritime-analytics',
    resourceGroup: 'maritime-platform-rg',
    subscription: process.env.AZURE_SUBSCRIPTION_ID || ''
  },
  cosmosDb: {
    endpoint: process.env.AZURE_COSMOSDB_ENDPOINT || '',
    databaseName: 'MaritimeData',
    containerName: 'TelemetryData'
  }
}