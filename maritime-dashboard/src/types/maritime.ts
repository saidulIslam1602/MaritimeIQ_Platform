// Maritime domain type definitions

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

export interface RouteData {
  routeId: string
  name: string
  origin: string
  destination: string
  distance: number
  estimatedDuration: number
  optimizationScore: number
  waypoints: Waypoint[]
}

export interface Waypoint {
  latitude: number
  longitude: number
  name?: string
  estimatedArrival?: string
}

export interface VesselMetrics {
  totalDistance: number
  averageSpeed: number
  fuelEfficiency: number
  activeVessels: number
}

export interface NavigationData {
  vesselId: number
  latitude: number
  longitude: number
  course: number
  speed: number
  timestamp: string
  weather?: WeatherData
}

export interface WeatherData {
  temperature: number
  windSpeed: number
  windDirection: number
  visibility: number
  seaState: number
}

export type VesselStatus = 'active' | 'maintenance' | 'docked' | 'emergency'
export type RouteStatus = 'active' | 'delayed' | 'on-time' | 'completed'