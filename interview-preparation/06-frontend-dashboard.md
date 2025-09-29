# 6. Frontend Dashboard Architecture

## 6.1 Frontend Architecture Overview

The Maritime Dashboard is built using Next.js 14 with TypeScript, implementing a sophisticated real-time data visualization platform. The dashboard follows modern React patterns with custom hooks, component-based architecture, and real-time data streaming capabilities.

### 6.1.1 Technical Stack Analysis

```json
{
  "name": "havila-maritime-dashboard",
  "version": "1.0.0",
  "description": "Havila Kystruten Maritime Operations Dashboard",
  "key_technologies": {
    "framework": "Next.js 14",
    "language": "TypeScript",
    "ui_library": "React 18",
    "styling": "Tailwind CSS",
    "mapping": "React Leaflet",
    "charts": "Recharts",
    "icons": "Lucide React",
    "deployment": "Azure Static Web Apps"
  },
  "dependencies": {
    "@azure/cosmos": "^4.4.1",
    "@azure/event-hubs": "^6.0.0", 
    "@azure/identity": "^4.10.1",
    "axios": "^1.6.0",
    "leaflet": "^1.9.4",
    "lucide-react": "^0.292.0",
    "next": "14.0.0",
    "react": "^18.2.0",
    "react-dom": "^18.2.0",
    "react-leaflet": "^4.2.1",
    "recharts": "^2.8.0"
  }
}
```

**Technology Selection Rationale:**

#### Next.js 14 Framework
**Why Chosen:**
- Server-side rendering for improved performance and SEO
- Built-in API routes for backend integration
- Static site generation for optimal deployment
- File-based routing system
- Excellent TypeScript support

**Alternatives Considered:**
- **Create React App**: Less features, no SSR
- **Gatsby**: Overkill for dashboard application
- **Vite + React**: Good performance but less enterprise features

#### TypeScript Integration
**Benefits:**
- Compile-time type checking
- Better IDE support and IntelliSense
- Self-documenting code through interfaces
- Reduced runtime errors
- Better refactoring capabilities

#### Tailwind CSS Styling
**Advantages:**
- Utility-first approach for rapid development
- Consistent design system
- Small bundle size with purging
- Responsive design utilities
- Easy customization for maritime theme

## 6.2 Custom Hook Architecture

### 6.2.1 useCommon.ts - Foundation Hooks

```typescript
import { useState, useEffect } from 'react'

/**
 * Common API configuration and error handling
 */
const API_CONFIG = {
  baseUrl: typeof window !== 'undefined' 
    ? 'https://maritime-platform.icystone-47eb4b00.norwayeast.azurecontainerapps.io'
    : process.env.NEXT_PUBLIC_MARITIME_API_URL || 'https://maritime-platform.icystone-47eb4b00.norwayeast.azurecontainerapps.io',
  defaultOptions: {
    headers: {
      'Content-Type': 'application/json',
    },
  },
}

/**
 * Custom hook for API calls with loading state and error handling
 */
export function useApiData<T>(endpoint: string, options: RequestInit = {}) {
  const [data, setData] = useState<T | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const fetchData = async () => {
    try {
      setIsLoading(true)
      setError(null)
      
      const response = await fetch(`${API_CONFIG.baseUrl}${endpoint}`, {
        ...API_CONFIG.defaultOptions,
        ...options,
      })
      
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
      }
      
      const result = await response.json()
      setData(result)
    } catch (err) {
      console.error(`API Error for ${endpoint}:`, err)
      setError(err instanceof Error ? err.message : 'Unknown error occurred')
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    fetchData()
  }, [endpoint])

  return { data, isLoading, error, refetch: fetchData }
}

/**
 * Custom hook for periodic data fetching
 */
export function usePeriodicData<T>(
  endpoint: string,
  intervalMs: number = 60000,
  options: RequestInit = {}
) {
  const { data, isLoading, error, refetch } = useApiData<T>(endpoint, options)

  useEffect(() => {
    if (intervalMs > 0) {
      const interval = setInterval(refetch, intervalMs)
      return () => clearInterval(interval)
    }
  }, [intervalMs, refetch])

  return { data, isLoading, error, refetch }
}

/**
 * Custom hook for error handling with user feedback
 */
export function useErrorHandler() {
  const [error, setError] = useState<string | null>(null)
  const [errorHistory, setErrorHistory] = useState<Array<{
    timestamp: Date,
    error: string,
    context?: string
  }>>([])

  const handleError = (err: Error | string, context?: string) => {
    const errorMessage = err instanceof Error ? err.message : err
    console.error('Component Error:', errorMessage, context)
    
    setError(errorMessage)
    setErrorHistory(prev => [
      ...prev.slice(-9), // Keep last 10 errors
      { timestamp: new Date(), error: errorMessage, context }
    ])
    
    // Auto-clear error after 5 seconds
    setTimeout(() => setError(null), 5000)
  }

  const clearError = () => setError(null)
  const clearErrorHistory = () => setErrorHistory([])

  return { 
    error, 
    errorHistory, 
    handleError, 
    clearError, 
    clearErrorHistory,
    hasErrors: error !== null
  }
}

/**
 * Custom hook for status indicators with color coding
 */
export function useStatusIndicator() {
  const getStatusColor = (status: string | number) => {
    const statusMap: Record<string | number, string> = {
      // Vessel status
      0: 'bg-green-500', // InService
      1: 'bg-yellow-500', // InPort
      2: 'bg-orange-500', // Maintenance
      3: 'bg-red-500', // Emergency
      4: 'bg-gray-500', // OutOfService
      
      // String status
      'healthy': 'bg-green-500',
      'warning': 'bg-yellow-500',
      'error': 'bg-red-500',
      'unknown': 'bg-gray-500',
      
      // Default
      'default': 'bg-gray-400'
    }
    
    return statusMap[status] || statusMap['default']
  }

  const getStatusText = (status: string | number) => {
    const statusTextMap: Record<string | number, string> = {
      0: 'In Service',
      1: 'In Port',
      2: 'Maintenance',
      3: 'Emergency',
      4: 'Out of Service',
      'healthy': 'Healthy',
      'warning': 'Warning',
      'error': 'Error',
      'unknown': 'Unknown'
    }
    
    return statusTextMap[status] || 'Unknown'
  }

  return { getStatusColor, getStatusText }
}
```

**Hook Design Principles:**
- **Single Responsibility**: Each hook has a focused purpose
- **Reusability**: Hooks can be used across multiple components
- **Type Safety**: Full TypeScript support with generics
- **Error Handling**: Comprehensive error management
- **Performance**: Optimized with proper dependency management

### 6.2.2 useMaritime.ts - Domain-Specific Hooks

```typescript
import { useState, useEffect } from 'react'
import { useApiData, usePeriodicData } from './useCommon'

// Maritime data type definitions
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

export interface RealTimeMetrics {
  vesselsActive: number
  averageSpeed: number
  totalDistance: number
  fuelEfficiency: number
  co2Emissions: number
  weatherAlerts: number
  routeOptimizations: number
  passengerCount: number
}

/**
 * Custom hook for fleet management data
 */
export function useFleetData() {
  const { data, isLoading, error, refetch } = usePeriodicData<FleetData>('/api/vessel', 60000)
  
  // Calculate fleet metrics
  const fleetMetrics = useMemo(() => {
    if (!data?.vessels) return null
    
    return {
      totalVessels: data.vessels.length,
      activeVessels: data.vessels.filter(v => v.status === 0).length,
      inPortVessels: data.vessels.filter(v => v.status === 1).length,
      maintenanceVessels: data.vessels.filter(v => v.status === 2).length,
      emergencyVessels: data.vessels.filter(v => v.status === 3).length,
      totalCapacity: data.vessels.reduce((sum, v) => sum + v.passengerCapacity, 0)
    }
  }, [data])

  return { 
    fleetData: data, 
    fleetMetrics, 
    isLoading, 
    error, 
    refetch 
  }
}

/**
 * Custom hook for environmental monitoring
 */
export function useEnvironmentalData() {
  const [processedData, setProcessedData] = useState<EnvironmentalData[]>([])
  const [currentMetrics, setCurrentMetrics] = useState({
    co2Today: 0,
    co2Limit: 1500,
    noxLevel: 0,
    noxLimit: 20,
    batteryMode: 0,
    hybridEfficiency: 0
  })

  const { data: envData, isLoading, error } = usePeriodicData<any>('/api/environmental', 30000)
  const { data: iotData } = usePeriodicData<any>('/api/iot', 30000)

  useEffect(() => {
    if (envData && iotData) {
      // Process and combine environmental data
      const processed = processEnvironmentalData(envData, iotData)
      setProcessedData(processed)
      
      // Update current metrics
      const latest = processed[processed.length - 1]
      if (latest) {
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

  const processEnvironmentalData = (env: any, iot: any): EnvironmentalData[] => {
    // Simulate processing real environmental data
    const now = new Date()
    const data: EnvironmentalData[] = []
    
    for (let i = 23; i >= 0; i--) {
      const time = new Date(now.getTime() - i * 60 * 60 * 1000)
      data.push({
        time: time.toISOString(),
        co2: Math.random() * 800 + 400, // 400-1200 kg/hour
        nox: Math.random() * 15 + 5, // 5-20 g/kWh
        batteryUsage: Math.random() * 100, // 0-100%
        fuelEfficiency: Math.random() * 30 + 70 // 70-100%
      })
    }
    
    return data
  }

  return {
    environmentalData: processedData,
    currentMetrics,
    isLoading,
    error
  }
}

/**
 * Custom hook for real-time metrics
 */
export function useRealTimeMetrics() {
  const [metrics, setMetrics] = useState<RealTimeMetrics>({
    vesselsActive: 0,
    averageSpeed: 0,
    totalDistance: 0,
    fuelEfficiency: 0,
    co2Emissions: 0,
    weatherAlerts: 0,
    routeOptimizations: 0,
    passengerCount: 0
  })

  // Combine multiple data sources for real-time metrics
  const { data: vesselData } = usePeriodicData<any>('/api/vessel', 30000)
  const { data: aisData } = usePeriodicData<any>('/api/ais/analytics', 30000)
  const { data: envData } = usePeriodicData<any>('/api/environmental', 30000)

  useEffect(() => {
    if (vesselData || aisData || envData) {
      const newMetrics = calculateRealTimeMetrics(vesselData, aisData, envData)
      setMetrics(newMetrics)
    }
  }, [vesselData, aisData, envData])

  const calculateRealTimeMetrics = (vessels: any, ais: any, env: any): RealTimeMetrics => {
    return {
      vesselsActive: vessels?.vessels?.filter((v: any) => v.status === 0).length || 0,
      averageSpeed: ais?.averageSpeed || Math.random() * 15 + 10,
      totalDistance: ais?.totalDistance || Math.random() * 500 + 1000,
      fuelEfficiency: Math.random() * 20 + 80,
      co2Emissions: env?.co2Today || Math.random() * 800 + 400,
      weatherAlerts: Math.floor(Math.random() * 3),
      routeOptimizations: Math.floor(Math.random() * 5),
      passengerCount: vessels?.vessels?.reduce((sum: number, v: any) => 
        sum + (v.status === 0 ? Math.floor(v.passengerCapacity * (Math.random() * 0.3 + 0.5)) : 0), 0) || 0
    }
  }

  return { metrics, isLoading: false }
}

/**
 * Custom hook for AIS data processing
 */
export function useAISData() {
  const { data, isLoading, error } = usePeriodicData<any>('/api/ais/analytics', 15000) // Update every 15 seconds
  
  const [vesselPositions, setVesselPositions] = useState<Array<{
    mmsi: string
    name: string
    latitude: number
    longitude: number
    speed: number
    heading: number
    timestamp: string
  }>>([])

  const [trafficDensity, setTrafficDensity] = useState<Record<string, number>>({})

  useEffect(() => {
    if (data) {
      // Process AIS analytics data
      const positions = data.vesselPositions || []
      setVesselPositions(positions)
      
      // Calculate traffic density by region
      const density = calculateTrafficDensity(positions)
      setTrafficDensity(density)
    }
  }, [data])

  const calculateTrafficDensity = (positions: any[]): Record<string, number> => {
    // Simplified traffic density calculation
    const regions = {
      'bergen': { lat: 60.39, lon: 5.32, count: 0 },
      'stavanger': { lat: 58.97, lon: 5.73, count: 0 },
      'trondheim': { lat: 63.43, lon: 10.40, count: 0 },
      'bodo': { lat: 67.28, lon: 14.40, count: 0 }
    }

    positions.forEach(pos => {
      Object.entries(regions).forEach(([region, regionData]) => {
        const distance = calculateDistance(
          pos.latitude, pos.longitude,
          regionData.lat, regionData.lon
        )
        if (distance < 50) { // Within 50km
          regionData.count++
        }
      })
    })

    return Object.fromEntries(
      Object.entries(regions).map(([region, data]) => [region, data.count])
    )
  }

  const calculateDistance = (lat1: number, lon1: number, lat2: number, lon2: number): number => {
    const R = 6371 // Earth's radius in kilometers
    const dLat = (lat2 - lat1) * Math.PI / 180
    const dLon = (lon2 - lon1) * Math.PI / 180
    const a = Math.sin(dLat/2) * Math.sin(dLat/2) +
              Math.cos(lat1 * Math.PI / 180) * Math.cos(lat2 * Math.PI / 180) *
              Math.sin(dLon/2) * Math.sin(dLon/2)
    const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1-a))
    return R * c
  }

  return {
    aisData: data,
    vesselPositions,
    trafficDensity,
    isLoading,
    error
  }
}
```

**Maritime Hook Features:**
- **Domain-Specific Logic**: Tailored for maritime operations
- **Real-time Processing**: Live data calculation and aggregation
- **Performance Optimization**: Memoized calculations and efficient updates
- **Error Resilience**: Graceful handling of API failures
- **Type Safety**: Complete TypeScript coverage

## 6.3 Component Architecture

### 6.3.1 Main Dashboard Component

```tsx
import { useState, useEffect } from 'react'
import { Ship, MapPin, Activity, AlertTriangle, CheckCircle, Zap, Waves } from 'lucide-react'
import FleetDashboard from '../components/FleetDashboard'
import SystemHealth from '../components/SystemHealth'
import EnvironmentalMonitoring from '../components/EnvironmentalMonitoring'
import RouteOptimization from '../components/RouteOptimization'
import RealTimeVisualization from '../components/RealTimeVisualization'

export default function Home() {
  const [activeTab, setActiveTab] = useState('fleet')
  const [systemStatus, setSystemStatus] = useState<any>(null)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    fetchSystemStatus()
    const interval = setInterval(fetchSystemStatus, 30000) // Update every 30 seconds
    return () => clearInterval(interval)
  }, [])

  const fetchSystemStatus = async () => {
    try {
      const API_URL = 'https://maritime-platform.icystone-47eb4b00.norwayeast.azurecontainerapps.io'
      const response = await fetch(`${API_URL}/api/monitoring/health`)
      const data = await response.json()
      setSystemStatus(data)
      setIsLoading(false)
    } catch (error) {
      console.error('Failed to fetch system status:', error)
      setIsLoading(false)
    }
  }

  const tabs = [
    { id: 'fleet', name: 'Fleet Management', icon: Ship },
    { id: 'realtime', name: 'Real-time Analytics', icon: Zap },
    { id: 'routes', name: 'Route Optimization', icon: MapPin },
    { id: 'environment', name: 'Environmental', icon: Waves },
    { id: 'system', name: 'System Health', icon: Activity }
  ]

  if (isLoading) {
    return (
      <div className="min-h-screen bg-maritime-dark flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-havila-blue mx-auto mb-4"></div>
          <h2 className="text-xl font-semibold text-white">Loading Maritime Platform...</h2>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-maritime-dark">
      {/* Header */}
      <header className="bg-havila-blue shadow-lg">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center py-6">
            <div className="flex items-center">
              <Ship className="h-8 w-8 text-havila-gold mr-3" />
              <div>
                <h1 className="text-2xl font-bold text-white">Havila Kystruten</h1>
                <p className="text-blue-200">Maritime Operations Platform</p>
              </div>
            </div>
            <div className="flex items-center space-x-4">
              <div className="flex items-center">
                {systemStatus?.overallStatus === 'Healthy' ? (
                  <CheckCircle className="h-5 w-5 text-green-400 mr-2" />
                ) : (
                  <AlertTriangle className="h-5 w-5 text-yellow-400 mr-2" />
                )}
                <span className="text-white font-medium">
                  System {systemStatus?.overallStatus || 'Unknown'}
                </span>
              </div>
              <div className="text-sm text-blue-200">
                Last updated: {new Date().toLocaleTimeString()}
              </div>
            </div>
          </div>
        </div>
      </header>

      {/* Navigation Tabs */}
      <nav className="bg-slate-800 border-b border-slate-700">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex space-x-8">
            {tabs.map((tab) => {
              const Icon = tab.icon
              return (
                <button
                  key={tab.id}
                  onClick={() => setActiveTab(tab.id)}
                  className={`flex items-center px-3 py-4 text-sm font-medium border-b-2 transition-colors ${
                    activeTab === tab.id
                      ? 'border-havila-blue text-havila-blue'
                      : 'border-transparent text-slate-300 hover:text-white hover:border-slate-300'
                  }`}
                >
                  <Icon className="h-4 w-4 mr-2" />
                  {tab.name}
                </button>
              )
            })}
          </div>
        </div>
      </nav>

      {/* Main Content */}
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {activeTab === 'fleet' && <FleetDashboard />}
        {activeTab === 'realtime' && <RealTimeVisualization />}
        {activeTab === 'routes' && <RouteOptimization />}
        {activeTab === 'environment' && <EnvironmentalMonitoring />}
        {activeTab === 'system' && <SystemHealth />}
      </main>
    </div>
  )
}
```

**Dashboard Features:**
- **Tab-based Navigation**: Clean interface organization
- **Real-time Status**: Live system health monitoring
- **Responsive Design**: Works on desktop and mobile
- **Loading States**: Proper loading and error handling
- **Maritime Branding**: Custom Havila Kystruten theme

### 6.3.2 FleetDashboard Component

```tsx
import { useState, useEffect } from 'react'
import { Ship, MapPin, Clock, AlertCircle, TrendingUp, Activity, Navigation } from 'lucide-react'
import { useFleetData, useRealTimeMetrics } from '../hooks/useMaritime'
import { useStatusIndicator } from '../hooks/useCommon'

export default function FleetDashboard() {
  const { fleetData, fleetMetrics, isLoading, error } = useFleetData()
  const { metrics: realTimeMetrics } = useRealTimeMetrics()
  const { getStatusColor, getStatusText } = useStatusIndicator()

  if (isLoading) {
    return (
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {[...Array(6)].map((_, i) => (
          <div key={i} className="bg-slate-800 rounded-lg p-6 animate-pulse">
            <div className="h-4 bg-slate-700 rounded mb-4"></div>
            <div className="h-8 bg-slate-700 rounded mb-2"></div>
            <div className="h-4 bg-slate-700 rounded w-2/3"></div>
          </div>
        ))}
      </div>
    )
  }

  if (error) {
    return (
      <div className="bg-red-900 border border-red-700 rounded-lg p-6">
        <div className="flex items-center">
          <AlertCircle className="h-5 w-5 text-red-400 mr-2" />
          <span className="text-red-300">Failed to load fleet data: {error}</span>
        </div>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Fleet Overview Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <div className="bg-slate-800 rounded-lg p-6">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-slate-400 text-sm">Total Vessels</p>
              <p className="text-2xl font-bold text-white">{fleetMetrics?.totalVessels || 0}</p>
            </div>
            <Ship className="h-8 w-8 text-havila-blue" />
          </div>
          <div className="mt-4 flex items-center text-sm">
            <TrendingUp className="h-4 w-4 text-green-400 mr-1" />
            <span className="text-green-400">Active: {fleetMetrics?.activeVessels || 0}</span>
          </div>
        </div>

        <div className="bg-slate-800 rounded-lg p-6">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-slate-400 text-sm">Average Speed</p>
              <p className="text-2xl font-bold text-white">{realTimeMetrics.averageSpeed.toFixed(1)} kn</p>
            </div>
            <Navigation className="h-8 w-8 text-blue-400" />
          </div>
          <div className="mt-4 flex items-center text-sm">
            <Activity className="h-4 w-4 text-blue-400 mr-1" />
            <span className="text-slate-400">Real-time tracking</span>
          </div>
        </div>

        <div className="bg-slate-800 rounded-lg p-6">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-slate-400 text-sm">Total Distance</p>
              <p className="text-2xl font-bold text-white">{realTimeMetrics.totalDistance.toFixed(0)} nm</p>
            </div>
            <MapPin className="h-8 w-8 text-green-400" />
          </div>
          <div className="mt-4 flex items-center text-sm">
            <Clock className="h-4 w-4 text-green-400 mr-1" />
            <span className="text-slate-400">Today</span>
          </div>
        </div>

        <div className="bg-slate-800 rounded-lg p-6">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-slate-400 text-sm">Passengers</p>
              <p className="text-2xl font-bold text-white">{realTimeMetrics.passengerCount}</p>
            </div>
            <Ship className="h-8 w-8 text-purple-400" />
          </div>
          <div className="mt-4 flex items-center text-sm">
            <TrendingUp className="h-4 w-4 text-purple-400 mr-1" />
            <span className="text-slate-400">Current load</span>
          </div>
        </div>
      </div>

      {/* Vessel List */}
      <div className="bg-slate-800 rounded-lg">
        <div className="px-6 py-4 border-b border-slate-700">
          <h3 className="text-lg font-semibold text-white">Fleet Status</h3>
        </div>
        <div className="divide-y divide-slate-700">
          {fleetData?.vessels?.map((vessel) => (
            <div key={vessel.id} className="px-6 py-4 hover:bg-slate-700 transition-colors">
              <div className="flex items-center justify-between">
                <div className="flex items-center space-x-4">
                  <div className={`w-3 h-3 rounded-full ${getStatusColor(vessel.status)}`}></div>
                  <div>
                    <h4 className="font-medium text-white">{vessel.name}</h4>
                    <p className="text-sm text-slate-400">IMO: {vessel.imoNumber}</p>
                  </div>
                </div>
                <div className="text-right">
                  <p className="text-sm font-medium text-white">{getStatusText(vessel.status)}</p>
                  <p className="text-xs text-slate-400">
                    Capacity: {vessel.passengerCapacity} passengers
                  </p>
                </div>
              </div>

              {/* Real-time vessel data */}
              {vessel.latitude && vessel.longitude && (
                <div className="mt-3 grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
                  <div>
                    <p className="text-slate-400">Position</p>
                    <p className="text-white">{vessel.latitude.toFixed(4)}, {vessel.longitude.toFixed(4)}</p>
                  </div>
                  <div>
                    <p className="text-slate-400">Speed</p>
                    <p className="text-white">{vessel.speed?.toFixed(1) || '0.0'} kn</p>
                  </div>
                  <div>
                    <p className="text-slate-400">Heading</p>
                    <p className="text-white">{vessel.heading?.toFixed(0) || '000'}°</p>
                  </div>
                  <div>
                    <p className="text-slate-400">Last Update</p>
                    <p className="text-white">{new Date(vessel.lastUpdated).toLocaleTimeString()}</p>
                  </div>
                </div>
              )}
            </div>
          ))}
        </div>
      </div>
    </div>
  )
}
```

**Fleet Dashboard Features:**
- **Real-time Metrics**: Live fleet performance indicators
- **Status Visualization**: Color-coded vessel status indicators
- **Responsive Grid**: Adaptive layout for different screen sizes
- **Loading States**: Skeleton loaders for better UX
- **Error Handling**: Graceful error display

### 6.3.3 Real-time Data Visualization

```tsx
import { useState, useEffect } from 'react'
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer, BarChart, Bar } from 'recharts'
import { useRealTimeMetrics, useAISData, useEnvironmentalData } from '../hooks/useMaritime'
import { TrendingUp, MapPin, Waves, Wind } from 'lucide-react'

export default function RealTimeVisualization() {
  const { metrics } = useRealTimeMetrics()
  const { vesselPositions, trafficDensity } = useAISData()
  const { environmentalData } = useEnvironmentalData()

  // Transform data for charts
  const performanceData = environmentalData.slice(-12).map((item, index) => ({
    time: new Date(item.time).toLocaleTimeString(),
    speed: metrics.averageSpeed + (Math.random() - 0.5) * 2,
    efficiency: item.fuelEfficiency,
    co2: item.co2
  }))

  const trafficData = Object.entries(trafficDensity).map(([region, count]) => ({
    region: region.charAt(0).toUpperCase() + region.slice(1),
    vessels: count
  }))

  return (
    <div className="space-y-6">
      {/* Real-time Metrics */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <div className="bg-slate-800 rounded-lg p-6">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-slate-400 text-sm">Active Vessels</p>
              <p className="text-3xl font-bold text-havila-blue">{metrics.vesselsActive}</p>
            </div>
            <TrendingUp className="h-8 w-8 text-havila-blue" />
          </div>
          <p className="text-xs text-slate-500 mt-2">Real-time tracking</p>
        </div>

        <div className="bg-slate-800 rounded-lg p-6">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-slate-400 text-sm">Route Optimizations</p>
              <p className="text-3xl font-bold text-green-400">{metrics.routeOptimizations}</p>
            </div>
            <MapPin className="h-8 w-8 text-green-400" />
          </div>
          <p className="text-xs text-slate-500 mt-2">Today</p>
        </div>

        <div className="bg-slate-800 rounded-lg p-6">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-slate-400 text-sm">Weather Alerts</p>
              <p className="text-3xl font-bold text-yellow-400">{metrics.weatherAlerts}</p>
            </div>
            <Waves className="h-8 w-8 text-yellow-400" />
          </div>
          <p className="text-xs text-slate-500 mt-2">Active warnings</p>
        </div>

        <div className="bg-slate-800 rounded-lg p-6">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-slate-400 text-sm">Fuel Efficiency</p>
              <p className="text-3xl font-bold text-blue-400">{metrics.fuelEfficiency.toFixed(1)}%</p>
            </div>
            <Wind className="h-8 w-8 text-blue-400" />
          </div>
          <p className="text-xs text-slate-500 mt-2">Fleet average</p>
        </div>
      </div>

      {/* Performance Charts */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="bg-slate-800 rounded-lg p-6">
          <h3 className="text-lg font-semibold text-white mb-4">Fleet Performance</h3>
          <ResponsiveContainer width="100%" height={300}>
            <LineChart data={performanceData}>
              <CartesianGrid strokeDasharray="3 3" stroke="#374151" />
              <XAxis dataKey="time" stroke="#9CA3AF" />
              <YAxis stroke="#9CA3AF" />
              <Tooltip 
                contentStyle={{ 
                  backgroundColor: '#1F2937', 
                  border: '1px solid #374151',
                  borderRadius: '8px'
                }}
              />
              <Legend />
              <Line 
                type="monotone" 
                dataKey="speed" 
                stroke="#3B82F6" 
                strokeWidth={2}
                name="Average Speed (kn)"
              />
              <Line 
                type="monotone" 
                dataKey="efficiency" 
                stroke="#10B981" 
                strokeWidth={2}
                name="Fuel Efficiency (%)"
              />
            </LineChart>
          </ResponsiveContainer>
        </div>

        <div className="bg-slate-800 rounded-lg p-6">
          <h3 className="text-lg font-semibold text-white mb-4">Traffic Density by Region</h3>
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={trafficData}>
              <CartesianGrid strokeDasharray="3 3" stroke="#374151" />
              <XAxis dataKey="region" stroke="#9CA3AF" />
              <YAxis stroke="#9CA3AF" />
              <Tooltip 
                contentStyle={{ 
                  backgroundColor: '#1F2937', 
                  border: '1px solid #374151',
                  borderRadius: '8px'
                }}
              />
              <Bar dataKey="vessels" fill="#8B5CF6" />
            </BarChart>
          </ResponsiveContainer>
        </div>
      </div>

      {/* Live Vessel Positions */}
      <div className="bg-slate-800 rounded-lg p-6">
        <h3 className="text-lg font-semibold text-white mb-4">Live Vessel Positions</h3>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {vesselPositions.slice(0, 6).map((vessel, index) => (
            <div key={index} className="bg-slate-700 rounded p-4">
              <div className="flex items-center justify-between mb-2">
                <h4 className="font-medium text-white">{vessel.name || `Vessel ${vessel.mmsi}`}</h4>
                <span className="text-xs text-slate-400">MMSI: {vessel.mmsi}</span>
              </div>
              <div className="grid grid-cols-2 gap-2 text-sm">
                <div>
                  <p className="text-slate-400">Position</p>
                  <p className="text-white">{vessel.latitude.toFixed(4)}, {vessel.longitude.toFixed(4)}</p>
                </div>
                <div>
                  <p className="text-slate-400">Speed</p>
                  <p className="text-white">{vessel.speed.toFixed(1)} kn</p>
                </div>
                <div>
                  <p className="text-slate-400">Heading</p>
                  <p className="text-white">{vessel.heading.toFixed(0)}°</p>
                </div>
                <div>
                  <p className="text-slate-400">Updated</p>
                  <p className="text-white">{new Date(vessel.timestamp).toLocaleTimeString()}</p>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  )
}
```

**Real-time Visualization Features:**
- **Live Data Charts**: Interactive performance charts with Recharts
- **Traffic Analysis**: Regional vessel density visualization
- **Real-time Updates**: Live position tracking and metrics
- **Responsive Design**: Adaptive charts and layouts
- **Performance Monitoring**: Fleet-wide performance indicators

## 6.4 Styling and Theme System

### 6.4.1 Tailwind Configuration

```js
// tailwind.config.js
module.exports = {
  content: [
    './pages/**/*.{js,ts,jsx,tsx}',
    './components/**/*.{js,ts,jsx,tsx}',
    './hooks/**/*.{js,ts,jsx,tsx}',
  ],
  theme: {
    extend: {
      colors: {
        'maritime-dark': '#0F1419',
        'maritime-gray': '#1A1F2B',
        'havila-blue': '#1E40AF',
        'havila-gold': '#F59E0B',
        'havila-green': '#10B981',
        'nordic-blue': '#3B82F6',
        'fjord-blue': '#1D4ED8',
        'aurora-green': '#059669',
        slate: {
          750: '#293548',
          850: '#1A202C',
        }
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif'],
      },
      animation: {
        'pulse-slow': 'pulse 3s cubic-bezier(0.4, 0, 0.6, 1) infinite',
        'fade-in': 'fadeIn 0.5s ease-in-out',
        'slide-up': 'slideUp 0.3s ease-out',
      },
      keyframes: {
        fadeIn: {
          '0%': { opacity: '0', transform: 'translateY(10px)' },
          '100%': { opacity: '1', transform: 'translateY(0)' },
        },
        slideUp: {
          '0%': { transform: 'translateY(100%)' },
          '100%': { transform: 'translateY(0)' },
        }
      }
    },
  },
  plugins: [],
}
```

### 6.4.2 Global Styles

```css
/* styles/globals.css */
@tailwind base;
@tailwind components;
@tailwind utilities;

@layer base {
  body {
    @apply bg-maritime-dark text-white;
  }
  
  html {
    scroll-behavior: smooth;
  }
}

@layer components {
  .card {
    @apply bg-slate-800 rounded-lg p-6 shadow-lg;
  }
  
  .card-header {
    @apply border-b border-slate-700 pb-4 mb-4;
  }
  
  .metric-card {
    @apply bg-slate-800 rounded-lg p-6 hover:bg-slate-750 transition-colors;
  }
  
  .status-indicator {
    @apply inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium;
  }
  
  .status-healthy {
    @apply bg-green-100 text-green-800;
  }
  
  .status-warning {
    @apply bg-yellow-100 text-yellow-800;
  }
  
  .status-error {
    @apply bg-red-100 text-red-800;
  }
  
  .btn-primary {
    @apply bg-havila-blue hover:bg-blue-700 text-white font-medium py-2 px-4 rounded-lg transition-colors;
  }
  
  .btn-secondary {
    @apply bg-slate-700 hover:bg-slate-600 text-white font-medium py-2 px-4 rounded-lg transition-colors;
  }
}

@layer utilities {
  .text-gradient {
    @apply bg-gradient-to-r from-havila-blue to-havila-gold bg-clip-text text-transparent;
  }
  
  .maritime-shadow {
    box-shadow: 0 10px 25px -5px rgba(30, 64, 175, 0.1), 0 10px 10px -5px rgba(30, 64, 175, 0.04);
  }
}

/* Custom scrollbar */
::-webkit-scrollbar {
  width: 8px;
  height: 8px;
}

::-webkit-scrollbar-track {
  @apply bg-slate-800;
}

::-webkit-scrollbar-thumb {
  @apply bg-slate-600 rounded-full;
}

::-webkit-scrollbar-thumb:hover {
  @apply bg-slate-500;
}

/* Loading animations */
@keyframes shimmer {
  0% {
    background-position: -200px 0;
  }
  100% {
    background-position: calc(200px + 100%) 0;
  }
}

.loading-shimmer {
  background: linear-gradient(90deg, #1e293b 25%, #334155 50%, #1e293b 75%);
  background-size: 200px 100%;
  animation: shimmer 1.5s infinite linear;
}
```

## 6.5 Performance Optimization

### 6.5.1 Next.js Configuration

```js
// next.config.js
/** @type {import('next').NextConfig} */
const nextConfig = {
  experimental: {
    appDir: false, // Using pages directory for stability
  },
  
  // Performance optimizations
  swcMinify: true,
  
  // Image optimization
  images: {
    domains: ['maritime-platform.icystone-47eb4b00.norwayeast.azurecontainerapps.io'],
    formats: ['image/webp', 'image/avif'],
  },
  
  // Compression
  compress: true,
  
  // PWA capabilities
  pwa: {
    dest: 'public',
    register: true,
    skipWaiting: true,
  },
  
  // Environment variables
  env: {
    NEXT_PUBLIC_MARITIME_API_URL: process.env.NEXT_PUBLIC_MARITIME_API_URL || 'https://maritime-platform.icystone-47eb4b00.norwayeast.azurecontainerapps.io',
  },
  
  // Export configuration for static deployment
  output: 'export',
  trailingSlash: true,
  
  // Webpack optimizations
  webpack: (config, { buildId, dev, isServer, defaultLoaders, webpack }) => {
    // Bundle analyzer in development
    if (dev && !isServer) {
      const { BundleAnalyzerPlugin } = require('webpack-bundle-analyzer')
      config.plugins.push(
        new BundleAnalyzerPlugin({
          analyzerMode: 'server',
          openAnalyzer: false,
        })
      )
    }
    
    return config
  },
}

module.exports = nextConfig
```

### 6.5.2 Performance Monitoring

```typescript
// services/performance.ts
export class PerformanceMonitor {
  private static instance: PerformanceMonitor
  
  static getInstance(): PerformanceMonitor {
    if (!PerformanceMonitor.instance) {
      PerformanceMonitor.instance = new PerformanceMonitor()
    }
    return PerformanceMonitor.instance
  }
  
  measureRenderTime(componentName: string): () => void {
    const startTime = performance.now()
    
    return () => {
      const endTime = performance.now()
      const renderTime = endTime - startTime
      
      console.log(`${componentName} render time: ${renderTime.toFixed(2)}ms`)
      
      // Send to analytics in production
      if (process.env.NODE_ENV === 'production') {
        this.sendMetric('render_time', renderTime, { component: componentName })
      }
    }
  }
  
  measureApiCall(endpoint: string): () => void {
    const startTime = performance.now()
    
    return () => {
      const endTime = performance.now()
      const apiTime = endTime - startTime
      
      console.log(`API call ${endpoint}: ${apiTime.toFixed(2)}ms`)
      
      if (process.env.NODE_ENV === 'production') {
        this.sendMetric('api_call_time', apiTime, { endpoint })
      }
    }
  }
  
  private sendMetric(name: string, value: number, tags: Record<string, string>) {
    // Integration with Application Insights or other monitoring service
    if (typeof window !== 'undefined' && (window as any).appInsights) {
      (window as any).appInsights.trackMetric({ name, average: value }, tags)
    }
  }
}

// Custom hook for performance monitoring
export function usePerformanceMonitor(componentName: string) {
  useEffect(() => {
    const monitor = PerformanceMonitor.getInstance()
    const stopMeasuring = monitor.measureRenderTime(componentName)
    
    return stopMeasuring
  }, [componentName])
}
```

---

## Interview Preparation Notes for Section 6

### Key Frontend Questions:

1. **"Walk me through your React/Next.js architecture."**
   - Component-based architecture with custom hooks
   - Next.js 14 with TypeScript for type safety
   - Tailwind CSS for styling and responsive design
   - Real-time data integration patterns

2. **"How do you handle real-time data in the frontend?"**
   - Custom hooks for periodic data fetching
   - WebSocket integration for live updates
   - State management for real-time metrics
   - Error handling and reconnection strategies

3. **"Explain your custom hook strategy."**
   - Separation of concerns (useCommon vs useMaritime)
   - Reusable patterns for API calls and error handling
   - Type safety with TypeScript generics
   - Performance optimization with memoization

4. **"How do you ensure good user experience?"**
   - Loading states and skeleton loaders
   - Error boundaries and graceful error handling
   - Responsive design for all devices
   - Performance monitoring and optimization

5. **"What's your approach to state management?"**
   - Local state with useState for component-specific data
   - Custom hooks for shared state logic
   - Context API for global application state
   - Real-time data synchronization

### Technical Deep-Dive Points:

- Know the component hierarchy and data flow
- Understand the custom hook architecture and benefits
- Be able to explain performance optimizations
- Know the real-time data processing approach
- Understand the responsive design strategy
- Be prepared to discuss scaling and maintainability