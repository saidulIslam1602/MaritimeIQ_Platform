// Environmental monitoring type definitions

export interface EnvironmentalData {
  time: string
  co2: number
  nox: number
  batteryUsage: number
  fuelEfficiency?: number
  fuelConsumption?: number
  complianceStatus?: ComplianceStatus
}

export interface ComplianceStatus {
  imo2020: boolean
  norwegianWaters: boolean
  emissionZone: boolean
}

export interface EmissionData {
  co2Emissions: number
  noxLevels: number
  soxLevels: number
  particulates: number
  timestamp: string
  location: {
    latitude: number
    longitude: number
  }
}

export interface EnvironmentalMetrics {
  // Current metrics
  co2Today: number
  co2Limit: number
  noxLevel: number
  noxLimit: number
  batteryMode: number
  hybridEfficiency: number
  // Legacy metrics (optional)
  totalEmissions?: number
  emissionReduction?: number
  complianceScore?: number
  greenTechUsage?: number
}

export interface BatteryStatus {
  charge: number
  capacity: number
  temperature: number
  cycleCount: number
  health: 'excellent' | 'good' | 'fair' | 'poor'
}

export interface FuelData {
  type: 'marine_gas_oil' | 'marine_diesel' | 'lng' | 'battery'
  consumption: number
  efficiency: number
  cost: number
  co2Factor: number
}

export type EmissionZone = 'neca' | 'seca' | 'eca' | 'standard'
export type ComplianceLevel = 'compliant' | 'warning' | 'violation'