// Maritime calculation utilities

import { Vessel, EnvironmentalData } from '../types/maritime'
import { AnalyticsMetrics } from '../types/analytics'

export const calculateFuelEfficiency = (
  fuelConsumed: number, 
  distanceTraveled: number
): number => {
  if (distanceTraveled === 0) return 0
  return fuelConsumed / distanceTraveled
}

export const calculateEmissionReduction = (
  currentEmissions: number,
  baselineEmissions: number
): number => {
  if (baselineEmissions === 0) return 0
  return ((baselineEmissions - currentEmissions) / baselineEmissions) * 100
}

export const calculateOptimizationSavings = (
  originalFuel: number,
  optimizedFuel: number
): { savings: number; percentage: number } => {
  const savings = originalFuel - optimizedFuel
  const percentage = (savings / originalFuel) * 100
  return { savings, percentage }
}

export const calculateAverageSpeed = (vessels: Vessel[]): number => {
  const activeVessels = vessels.filter(v => v.speed && v.speed > 0)
  if (activeVessels.length === 0) return 0
  
  const totalSpeed = activeVessels.reduce((sum, v) => sum + (v.speed || 0), 0)
  return totalSpeed / activeVessels.length
}

export const calculateFleetUtilization = (vessels: Vessel[]): number => {
  const activeVessels = vessels.filter(v => v.status === 1).length
  return (activeVessels / vessels.length) * 100
}

export const calculateComplianceScore = (data: EnvironmentalData[]): number => {
  if (data.length === 0) return 100
  
  // Simple compliance calculation - can be enhanced
  const compliantReadings = data.filter(d => 
    d.co2 < 500 && // Daily limit
    d.nox < 2000   // NOx limit
  ).length
  
  return (compliantReadings / data.length) * 100
}

export const calculateTrendDirection = (
  current: number,
  previous: number
): 'up' | 'down' | 'stable' => {
  const threshold = 0.1 // 0.1% threshold for stability
  const change = ((current - previous) / previous) * 100
  
  if (Math.abs(change) <= threshold) return 'stable'
  return change > 0 ? 'up' : 'down'
}

export const calculateDistanceBetweenPoints = (
  lat1: number, lon1: number,
  lat2: number, lon2: number
): number => {
  const R = 3440.065 // Nautical miles radius of Earth
  const dLat = toRadians(lat2 - lat1)
  const dLon = toRadians(lon2 - lon1)
  
  const a = Math.sin(dLat / 2) * Math.sin(dLat / 2) +
    Math.cos(toRadians(lat1)) * Math.cos(toRadians(lat2)) *
    Math.sin(dLon / 2) * Math.sin(dLon / 2)
  
  const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a))
  
  return R * c
}

export const calculateETA = (
  currentLat: number, currentLon: number,
  destLat: number, destLon: number,
  speed: number // knots
): Date => {
  const distance = calculateDistanceBetweenPoints(currentLat, currentLon, destLat, destLon)
  const timeHours = distance / speed
  const eta = new Date()
  eta.setHours(eta.getHours() + timeHours)
  
  return eta
}

const toRadians = (degrees: number): number => {
  return degrees * (Math.PI / 180)
}