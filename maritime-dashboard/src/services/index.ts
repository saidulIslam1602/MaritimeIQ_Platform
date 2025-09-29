// Service exports for easier imports

// Analytics services
export { analyticsService } from './analytics/analytics'

// Messaging services  
export { 
  MaritimeWebSocketService,
  useRealTimeData,
  useRealTimeVesselData 
} from './messaging/websocket'

// Event Hub services
export { default as eventHubService } from './messaging/eventhub'