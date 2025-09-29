// Analytics and metrics type definitions

export interface AnalyticsMetrics {
  kpis: {
    fuelEfficiency: number
    emissionReduction: number
    onTimePerformance: number
    passengerSatisfaction: number
  }
  trends: TrendData[]
  predictions: PredictionData
}

export interface TrendData {
  name: string
  value: number
  change: number
  trend: 'up' | 'down' | 'stable'
}

export interface PredictionData {
  fuelConsumption: number[]
  co2Emissions: number[]
  routeOptimization: number[]
}

export interface TimeSeriesPoint {
  timestamp: string
  value: number
  metadata?: any
}

export interface PerformanceMetrics {
  cpu: number
  memory: number
  requestsPerSecond: number
  responseTime: number
  errorRate: number
}

export interface SystemHealthData {
  overall: 'healthy' | 'warning' | 'critical'
  services: ServiceStatus[]
  metrics: PerformanceMetrics
  alerts: Alert[]
}

export interface ServiceStatus {
  name: string
  status: 'online' | 'offline' | 'degraded'
  uptime: number
  lastCheck: string
}

export interface Alert {
  id: string
  type: 'info' | 'warning' | 'error' | 'critical'
  message: string
  timestamp: string
  resolved: boolean
}

export interface RealTimeData {
  type: 'vessel' | 'ais' | 'environmental' | 'system'
  data: any
  timestamp: string
}