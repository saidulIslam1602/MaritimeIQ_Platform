import { useState, useEffect } from 'react'
import { Ship, MapPin, Clock, AlertCircle, TrendingUp, Activity, Navigation } from 'lucide-react'
import { useRealTimeVesselData } from '../services/websocket'
import { analyticsService } from '../services/analytics'

interface Vessel {
  id: number
  name: string
  imoNumber: string
  callSign: string
  passengerCapacity: number
  status: number
  lastUpdated: string
}

interface FleetData {
  totalVessels: number
  fleetOperator: string
  routeNetwork: string
  vessels: Vessel[]
}

export default function FleetDashboard() {
  const [fleetData, setFleetData] = useState<FleetData | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [realTimeMetrics, setRealTimeMetrics] = useState({
    totalDistance: 0,
    averageSpeed: 0,
    fuelEfficiency: 0,
    activeVessels: 0
  })
  
  // Use real-time WebSocket data
  const { data: realTimeData, isConnected } = useRealTimeVesselData()

  useEffect(() => {
    fetchFleetData()
    const interval = setInterval(fetchFleetData, 60000) // Update every minute
    return () => clearInterval(interval)
  }, [])

  // Process real-time data updates
  useEffect(() => {
    if (realTimeData.size > 0) {
      const vesselData = realTimeData.get('vessel') || []
      const aisData = realTimeData.get('ais') || []
      
      // Update real-time metrics
      vesselData.forEach(data => analyticsService.processRealTimeData(data))
      aisData.forEach(data => analyticsService.processRealTimeData(data))
      
      // Calculate live metrics
      updateRealTimeMetrics(vesselData, aisData)
    }
  }, [realTimeData])

  const updateRealTimeMetrics = (vesselData: any[], aisData: any[]) => {
    const activeVessels = new Set(vesselData.map(d => d.vesselId)).size
    const totalDistance = vesselData.reduce((sum, d) => sum + (d.data.distanceTraveled || 0), 0)
    const averageSpeed = vesselData.length > 0 
      ? vesselData.reduce((sum, d) => sum + (d.data.speed || 0), 0) / vesselData.length
      : 0
    const fuelEfficiency = vesselData.length > 0
      ? vesselData.reduce((sum, d) => sum + (d.data.fuelEfficiency || 85), 0) / vesselData.length
      : 85

    setRealTimeMetrics({
      totalDistance: Math.round(totalDistance * 100) / 100,
      averageSpeed: Math.round(averageSpeed * 100) / 100,
      fuelEfficiency: Math.round(fuelEfficiency * 100) / 100,
      activeVessels
    })
  }

  const fetchFleetData = async () => {
    try {
      const API_URL = typeof window !== 'undefined' 
        ? 'https://maritime-platform.icystone-47eb4b00.norwayeast.azurecontainerapps.io'
        : process.env.NEXT_PUBLIC_MARITIME_API_URL || 'https://maritime-platform.icystone-47eb4b00.norwayeast.azurecontainerapps.io'
      const response = await fetch(`${API_URL}/api/vessel`)
      const data = await response.json()
      setFleetData(data)
      setIsLoading(false)
    } catch (error) {
      console.error('Failed to fetch fleet data:', error)
      setIsLoading(false)
    }
  }

  const getStatusColor = (status: number) => {
    switch (status) {
      case 0: return 'bg-green-500'
      case 1: return 'bg-yellow-500'
      case 2: return 'bg-red-500'
      default: return 'bg-gray-500'
    }
  }

  const getStatusText = (status: number) => {
    switch (status) {
      case 0: return 'Operational'
      case 1: return 'In Port'
      case 2: return 'Maintenance'
      default: return 'Unknown'
    }
  }

  if (isLoading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-havila-blue"></div>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Fleet Overview */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="metric-card">
          <div className="flex items-center">
            <Ship className="h-8 w-8 text-havila-blue mr-3" />
            <div>
              <p className="text-2xl font-bold text-white">{fleetData?.totalVessels || 0}</p>
              <p className="text-slate-400">Total Vessels</p>
            </div>
          </div>
        </div>
        
        <div className="metric-card">
          <div className="flex items-center">
            <MapPin className="h-8 w-8 text-green-500 mr-3" />
            <div>
              <p className="text-2xl font-bold text-white">4</p>
              <p className="text-slate-400">Active Routes</p>
            </div>
          </div>
        </div>
        
        <div className="metric-card">
          <div className="flex items-center">
            <TrendingUp className="h-8 w-8 text-blue-500 mr-3" />
            <div>
              <p className="text-2xl font-bold text-white">2,560</p>
              <p className="text-slate-400">Passengers Today</p>
            </div>
          </div>
        </div>
        
        <div className="metric-card">
          <div className="flex items-center">
            <Clock className="h-8 w-8 text-yellow-500 mr-3" />
            <div>
              <p className="text-2xl font-bold text-white">98.5%</p>
              <p className="text-slate-400">On-Time Performance</p>
            </div>
          </div>
        </div>
      </div>

      {/* Fleet Status */}
      <div className="dashboard-card">
        <h2 className="text-xl font-semibold text-white mb-6">Fleet Status</h2>
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {fleetData?.vessels.map((vessel) => (
            <div key={vessel.id} className="bg-slate-700 rounded-lg p-4 border border-slate-600">
              <div className="flex justify-between items-start mb-4">
                <div>
                  <h3 className="text-lg font-semibold text-white">{vessel.name}</h3>
                  <p className="text-slate-400">IMO: {vessel.imoNumber} | Call Sign: {vessel.callSign}</p>
                </div>
                <div className="flex items-center">
                  <div className={`w-3 h-3 rounded-full ${getStatusColor(vessel.status)} mr-2`}></div>
                  <span className="text-sm text-slate-300">{getStatusText(vessel.status)}</span>
                </div>
              </div>
              
              <div className="grid grid-cols-2 gap-4 text-sm">
                <div>
                  <p className="text-slate-400">Capacity</p>
                  <p className="text-white font-medium">{vessel.passengerCapacity} passengers</p>
                </div>
                <div>
                  <p className="text-slate-400">Last Updated</p>
                  <p className="text-white font-medium">
                    {new Date(vessel.lastUpdated).toLocaleTimeString()}
                  </p>
                </div>
              </div>
              
              {/* Mock live data */}
              <div className="mt-4 pt-4 border-t border-slate-600">
                <div className="grid grid-cols-3 gap-2 text-xs">
                  <div className="text-center">
                    <p className="text-slate-400">Speed</p>
                    <p className="text-white font-bold">{Math.floor(Math.random() * 15) + 8} kn</p>
                  </div>
                  <div className="text-center">
                    <p className="text-slate-400">Position</p>
                    <p className="text-white font-bold">
                      {(60 + Math.random() * 10).toFixed(2)}°N
                    </p>
                  </div>
                  <div className="text-center">
                    <p className="text-slate-400">ETA</p>
                    <p className="text-white font-bold">
                      {Math.floor(Math.random() * 8) + 1}h
                    </p>
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Route Information */}
      <div className="dashboard-card">
        <h2 className="text-xl font-semibold text-white mb-6">Current Routes</h2>
        <div className="text-center text-slate-400 py-8">
          <MapPin className="h-12 w-12 mx-auto mb-4 opacity-50" />
          <p>Interactive route map will be displayed here</p>
          <p className="text-sm mt-2">Bergen ↔ Kirkenes Coastal Route</p>
        </div>
      </div>
    </div>
  )
}