import { useState, useEffect } from 'react'
import { useApiData, usePeriodicData } from './useCommon'

// Types for maritime data
export interface Vessel {
  id: number
  name: string
  imoNumber: string
  callSign: string
  passengerCapacity: number
  status: number
  lastUpdated: string
  latitude?: number
  longitude?: number
  speed?: number
  heading?: number
}

export interface FleetData {
  totalVessels: number
  fleetOperator: string
  routeNetwork: string
  vessels: Vessel[]
}

export interface EnvironmentalData {
  time: string
  co2: number
  nox: number
  batteryUsage: number
  fuelEfficiency: number
}

export interface EnvironmentalMetrics {
  co2Today: number
  co2Limit: number
  noxLevel: number
  noxLimit: number
  batteryMode: number
  hybridEfficiency: number
}

export interface RouteData {
  id: string
  name: string
  ports: string[]
  distance: number
  estimatedTime: number
  fuelConsumption: number
  weatherConditions: string[]
}

export interface SystemHealthData {
  overall: string
  services: {
    name: string
    status: string
    lastCheck: string
    responseTime?: number
  }[]
  infrastructure: {
    cpu: number
    memory: number
    disk: number
    network: number
  }
}

/**
 * Custom hook for fleet data
 */
export function useFleetData() {
  return usePeriodicData<FleetData>('/api/vessel', 60000) // Update every minute
}

/**
 * Custom hook for environmental data
 */
export function useEnvironmentalData() {
  const [currentMetrics, setCurrentMetrics] = useState<EnvironmentalMetrics>({
    co2Today: 0,
    co2Limit: 1500,
    noxLevel: 0,
    noxLimit: 20,
    batteryMode: 0,
    hybridEfficiency: 0
  })

  const { data: envData, isLoading: envLoading, error: envError } = usePeriodicData<any>('/api/environmental', 30000)
  const { data: iotData, isLoading: iotLoading, error: iotError } = usePeriodicData<any>('/api/iot', 30000)

  const [processedData, setProcessedData] = useState<EnvironmentalData[]>([])

  useEffect(() => {
    if (envData && iotData) {
      // Process environmental data
      const processed = processEnvironmentalData(envData, iotData)
      setProcessedData(processed)
      
      // Update current metrics
      if (processed.length > 0) {
        const latest = processed[processed.length - 1]
        setCurrentMetrics(prev => ({
          ...prev,
          co2Today: latest.co2,
          noxLevel: latest.nox,
          batteryMode: latest.batteryUsage,
          hybridEfficiency: latest.fuelEfficiency
        }))
      }
    }
  }, [envData, iotData])

  const processEnvironmentalData = (envData: any, iotData: any): EnvironmentalData[] => {
    // Mock processing logic - replace with actual data processing
    const currentTime = new Date()
    const data: EnvironmentalData[] = []

    for (let i = 0; i < 24; i++) {
      const time = new Date(currentTime.getTime() - (23 - i) * 60 * 60 * 1000)
      data.push({
        time: time.toISOString(),
        co2: Math.random() * 100 + 50,
        nox: Math.random() * 10 + 5,
        batteryUsage: Math.random() * 40 + 20,
        fuelEfficiency: Math.random() * 30 + 70
      })
    }

    return data
  }

  return {
    environmentalData: processedData,
    currentMetrics,
    isLoading: envLoading || iotLoading,
    error: envError || iotError
  }
}

/**
 * Custom hook for route optimization data
 */
export function useRouteData() {
  const [optimizedRoutes, setOptimizedRoutes] = useState<RouteData[]>([])
  
  const { data, isLoading, error } = usePeriodicData<any>('/api/routes', 120000) // Update every 2 minutes

  useEffect(() => {
    if (data) {
      // Process route data
      setOptimizedRoutes(data.routes || [])
    }
  }, [data])

  return { optimizedRoutes, isLoading, error }
}

/**
 * Custom hook for system health data
 */
export function useSystemHealth() {
  const [healthMetrics, setHealthMetrics] = useState<SystemHealthData>({
    overall: 'loading',
    services: [],
    infrastructure: {
      cpu: 0,
      memory: 0,
      disk: 0,
      network: 0
    }
  })

  const { data, isLoading, error } = usePeriodicData<any>('/api/monitoring', 15000) // Update every 15 seconds

  useEffect(() => {
    if (data) {
      setHealthMetrics({
        overall: data.overall || 'unknown',
        services: data.services || [],
        infrastructure: data.infrastructure || {
          cpu: 0,
          memory: 0,
          disk: 0,
          network: 0
        }
      })
    }
  }, [data])

  return { healthMetrics, isLoading, error }
}

/**
 * Custom hook for real-time metrics calculations
 */
export function useRealTimeMetrics(vessels: Vessel[]) {
  const [metrics, setMetrics] = useState({
    totalDistance: 0,
    averageSpeed: 0,
    fuelEfficiency: 0,
    activeVessels: 0
  })

  useEffect(() => {
    if (vessels.length > 0) {
      const activeVessels = vessels.filter(vessel => vessel.status === 1).length
      const speeds = vessels
        .filter(vessel => vessel.speed && vessel.speed > 0)
        .map(vessel => vessel.speed!)
      
      const averageSpeed = speeds.length > 0 
        ? speeds.reduce((sum, speed) => sum + speed, 0) / speeds.length 
        : 0

      // Mock calculations - replace with real calculations
      const totalDistance = vessels.length * 100 // Mock total distance
      const fuelEfficiency = 85 + Math.random() * 10 // Mock fuel efficiency

      setMetrics({
        totalDistance,
        averageSpeed,
        fuelEfficiency,
        activeVessels
      })
    }
  }, [vessels])

  return metrics
}

/**
 * Custom hook for weather and sea conditions
 */
export function useWeatherData() {
  const { data, isLoading, error } = usePeriodicData<any>('/api/environmental/weather', 300000) // Update every 5 minutes

  const [weatherConditions, setWeatherConditions] = useState({
    temperature: 0,
    windSpeed: 0,
    windDirection: 0,
    waveHeight: 0,
    visibility: 0,
    conditions: 'unknown'
  })

  useEffect(() => {
    if (data) {
      setWeatherConditions({
        temperature: data.temperature || 0,
        windSpeed: data.windSpeed || 0,
        windDirection: data.windDirection || 0,
        waveHeight: data.waveHeight || 0,
        visibility: data.visibility || 0,
        conditions: data.conditions || 'unknown'
      })
    }
  }, [data])

  return { weatherConditions, isLoading, error }
}

/**
 * Custom hook for AIS data processing
 */
export function useAISData() {
  const { data, isLoading, error } = usePeriodicData<any>('/api/ais', 10000) // Update every 10 seconds

  const [aisData, setAisData] = useState<{
    vessels: Vessel[]
    totalMessages: number
    lastUpdate: string
  }>({
    vessels: [],
    totalMessages: 0,
    lastUpdate: new Date().toISOString()
  })

  useEffect(() => {
    if (data) {
      setAisData({
        vessels: data.vessels || [],
        totalMessages: data.totalMessages || 0,
        lastUpdate: data.lastUpdate || new Date().toISOString()
      })
    }
  }, [data])

  return { aisData, isLoading, error }
}