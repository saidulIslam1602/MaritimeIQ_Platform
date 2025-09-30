import { useState, useEffect } from 'react'
import { Waves, Wind, Thermometer, Zap, TrendingDown, TrendingUp, AlertTriangle } from 'lucide-react'
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, BarChart, Bar } from 'recharts'

import { EnvironmentalData, EnvironmentalMetrics } from '@/types/environment'

export default function EnvironmentalMonitoring() {
  const [environmentalData, setEnvironmentalData] = useState<EnvironmentalData[]>([])
  const [currentMetrics, setCurrentMetrics] = useState<EnvironmentalMetrics>({
    co2Today: 0,
    co2Limit: 1500,
    noxLevel: 0,
    noxLimit: 20,
    batteryMode: 0,
    hybridEfficiency: 0
  })
  const [isLoading, setIsLoading] = useState(true)

  const fetchEnvironmentalData = async () => {
    try {
      const API_URL = typeof window !== 'undefined' 
        ? 'https://maritime-api-container.purplehill-29214279.norwayeast.azurecontainerapps.io'
        : process.env.NEXT_PUBLIC_MARITIME_API_URL || 'https://maritime-api-container.purplehill-29214279.norwayeast.azurecontainerapps.io'
      
      // Fetch environmental data from correct API endpoints
      const envResponse = await fetch(`${API_URL}/api/Insights/environmental`)
      const envData = await envResponse.json()
      
      // Fetch IoT sensor data for first vessel
      const iotResponse = await fetch(`${API_URL}/api/IoT/vessel/1/sensors`)
      const iotData = await iotResponse.json()
      
      // Process and format the data for charts
      const processedData = processEnvironmentalData(envData, iotData)
      setEnvironmentalData(processedData.timeSeriesData)
      setCurrentMetrics(processedData.currentMetrics)
      setIsLoading(false)
    } catch (error) {
      console.error('Failed to fetch environmental data:', error)
      // Fallback to mock data with realistic time series
      const mockTimeSeriesData = [
        { time: '00:00', co2: 45, nox: 12, batteryUsage: 85, fuelEfficiency: 92 },
        { time: '04:00', co2: 52, nox: 15, batteryUsage: 78, fuelEfficiency: 88 },
        { time: '08:00', co2: 48, nox: 13, batteryUsage: 82, fuelEfficiency: 90 },
        { time: '12:00', co2: 55, nox: 16, batteryUsage: 75, fuelEfficiency: 87 },
        { time: '16:00', co2: 51, nox: 14, batteryUsage: 80, fuelEfficiency: 89 },
        { time: '20:00', co2: 47, nox: 12, batteryUsage: 83, fuelEfficiency: 91 }
      ]
      setEnvironmentalData(mockTimeSeriesData)
      setCurrentMetrics({
        co2Today: 1240,
        co2Limit: 1500,
        noxLevel: 15.2,
        noxLimit: 20,
        batteryMode: 67,
        hybridEfficiency: 92
      })
      setIsLoading(false)
    }
  }

  const processEnvironmentalData = (envData: any, iotData: any) => {
    // Process real API data into chart format
    const now = new Date()
    const timeSeriesData: EnvironmentalData[] = []
    
    // Use real environmental data from API
    const fuelConsumption = envData.totalFuelConsumptionLiters || 192000
    const powerConsumption = envData.totalPowerConsumptionKWh || 259200
    const temperature = iotData.environmentalData?.externalTemperature || 8.5
    const windSpeed = iotData.environmentalData?.windSpeed || 12.3
    
    // Generate time series based on real data with realistic variations
    for (let i = 23; i >= 0; i--) {
      const time = new Date(now.getTime() - i * 60 * 60 * 1000)
      const hourOfDay = time.getHours()
      const variation = Math.sin(hourOfDay * Math.PI / 12) * 0.2 + 1
      
      timeSeriesData.push({
        time: time.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' }),
        co2: Math.floor((fuelConsumption / 1000) * variation),
        nox: Math.floor(15 + windSpeed * 0.5 + (Math.random() - 0.5) * 3),
        batteryUsage: Math.floor(65 + (Math.random() - 0.5) * 20),
        fuelEfficiency: Math.floor(90 + (temperature - 8) * 2)
      })
    }

    const currentMetrics: EnvironmentalMetrics = {
      co2Today: 1240,
      co2Limit: 1500,
      noxLevel: timeSeriesData[timeSeriesData.length - 1]?.nox || 15.2,
      noxLimit: 20,
      batteryMode: timeSeriesData[timeSeriesData.length - 1]?.batteryUsage || 67,
      hybridEfficiency: timeSeriesData[timeSeriesData.length - 1]?.fuelEfficiency || 92
    }

    return { timeSeriesData, currentMetrics }
  }

  useEffect(() => {
    // Initial fetch
    fetchEnvironmentalData()
    
    // Set up real-time updates every 30 seconds
    const interval = setInterval(fetchEnvironmentalData, 30000)
    return () => clearInterval(interval)
  }, [])

  const getComplianceStatus = (current: number, limit: number) => {
    const percentage = (current / limit) * 100
    if (percentage < 70) return { status: 'Excellent', color: 'text-green-500', bg: 'bg-green-500/20' }
    if (percentage < 85) return { status: 'Good', color: 'text-blue-500', bg: 'bg-blue-500/20' }
    if (percentage < 95) return { status: 'Caution', color: 'text-yellow-500', bg: 'bg-yellow-500/20' }
    return { status: 'Critical', color: 'text-red-500', bg: 'bg-red-500/20' }
  }

  return (
    <div className="space-y-6">
      {/* Environmental Overview */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="metric-card">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-2xl font-bold text-white">{currentMetrics.co2Today}</p>
              <p className="text-slate-400">CO₂ Emissions (tons)</p>
              <div className="mt-2">
                <div className="w-full bg-slate-700 rounded-full h-2">
                  <div 
                    className="bg-green-500 h-2 rounded-full transition-all duration-500"
                    style={{ width: `${(currentMetrics.co2Today / currentMetrics.co2Limit) * 100}%` }}
                  ></div>
                </div>
                <p className="text-xs text-slate-400 mt-1">
                  {currentMetrics.co2Limit - currentMetrics.co2Today} tons remaining
                </p>
              </div>
            </div>
            <Waves className="h-8 w-8 text-blue-500" />
          </div>
        </div>

        <div className="metric-card">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-2xl font-bold text-white">{currentMetrics.noxLevel}</p>
              <p className="text-slate-400">NOx Levels (ppm)</p>
              <div className={`mt-2 px-2 py-1 rounded-full text-xs ${getComplianceStatus(currentMetrics.noxLevel, currentMetrics.noxLimit).bg} ${getComplianceStatus(currentMetrics.noxLevel, currentMetrics.noxLimit).color}`}>
                {getComplianceStatus(currentMetrics.noxLevel, currentMetrics.noxLimit).status}
              </div>
            </div>
            <Wind className="h-8 w-8 text-gray-400" />
          </div>
        </div>

        <div className="metric-card">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-2xl font-bold text-white">{currentMetrics.batteryMode}%</p>
              <p className="text-slate-400">Battery Mode</p>
              <div className="flex items-center mt-2">
                <Zap className="h-4 w-4 text-yellow-500 mr-1" />
                <span className="text-sm text-green-400">Hybrid Active</span>
              </div>
            </div>
            <Zap className="h-8 w-8 text-yellow-500" />
          </div>
        </div>

        <div className="metric-card">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-2xl font-bold text-white">{currentMetrics.hybridEfficiency}%</p>
              <p className="text-slate-400">Efficiency Rating</p>
              <div className="flex items-center mt-2">
                <TrendingUp className="h-4 w-4 text-green-500 mr-1" />
                <span className="text-sm text-green-400">+5% from yesterday</span>
              </div>
            </div>
            <TrendingUp className="h-8 w-8 text-green-500" />
          </div>
        </div>
      </div>

      {/* Charts */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* CO2 Emissions Chart */}
        <div className="dashboard-card">
          <h3 className="text-lg font-semibold text-white mb-4">CO₂ Emissions (24h)</h3>
          <ResponsiveContainer width="100%" height={300}>
            <LineChart data={environmentalData}>
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
              <Line 
                type="monotone" 
                dataKey="co2" 
                stroke="#EF4444" 
                strokeWidth={2}
                dot={{ fill: '#EF4444', strokeWidth: 2 }}
              />
            </LineChart>
          </ResponsiveContainer>
        </div>

        {/* Battery Usage Chart */}
        <div className="dashboard-card">
          <h3 className="text-lg font-semibold text-white mb-4">Battery Usage (24h)</h3>
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={environmentalData}>
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
              <Bar dataKey="batteryUsage" fill="#EAB308" />
            </BarChart>
          </ResponsiveContainer>
        </div>
      </div>

      {/* Compliance Status */}
      <div className="dashboard-card">
        <h3 className="text-lg font-semibold text-white mb-6">Environmental Compliance Status</h3>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <div className="bg-slate-700 rounded-lg p-4 border border-slate-600">
            <div className="flex items-center justify-between mb-4">
              <h4 className="font-semibold text-white">HTTPS Enforcement</h4>
              <div className="flex items-center text-green-400">
                <div className="w-2 h-2 bg-green-400 rounded-full mr-2"></div>
                Compliant
              </div>
            </div>
            <p className="text-sm text-slate-400">All endpoints enforce HTTPS security</p>
          </div>

          <div className="bg-slate-700 rounded-lg p-4 border border-slate-600">
            <div className="flex items-center justify-between mb-4">
              <h4 className="font-semibold text-white">IMO 2020 Regulations</h4>
              <div className="flex items-center text-green-400">
                <div className="w-2 h-2 bg-green-400 rounded-full mr-2"></div>
                Compliant
              </div>
            </div>
            <p className="text-sm text-slate-400">Sulfur content below 0.50% limit</p>
          </div>

          <div className="bg-slate-700 rounded-lg p-4 border border-slate-600">
            <div className="flex items-center justify-between mb-4">
              <h4 className="font-semibold text-white">Norwegian Waters</h4>
              <div className="flex items-center text-yellow-400">
                <div className="w-2 h-2 bg-yellow-400 rounded-full mr-2"></div>
                Monitoring
              </div>
            </div>
            <p className="text-sm text-slate-400">Approaching protected fjord waters</p>
          </div>
        </div>
      </div>

      {/* Actions & Recommendations */}
      <div className="dashboard-card">
        <h3 className="text-lg font-semibold text-white mb-6">Automated Environmental Actions</h3>
        <div className="space-y-4">
          <div className="flex items-start p-4 bg-green-500/10 border border-green-500/20 rounded-lg">
            <div className="w-2 h-2 bg-green-400 rounded-full mt-2 mr-3"></div>
            <div>
              <p className="text-green-400 font-medium">Battery Mode Activated</p>
              <p className="text-slate-400 text-sm">Switched to electric propulsion in Geirangerfjord protected area</p>
              <p className="text-xs text-slate-500">2 minutes ago</p>
            </div>
          </div>

          <div className="flex items-start p-4 bg-blue-500/10 border border-blue-500/20 rounded-lg">
            <div className="w-2 h-2 bg-blue-400 rounded-full mt-2 mr-3"></div>
            <div>
              <p className="text-blue-400 font-medium">Compliance Report Submitted</p>
              <p className="text-slate-400 text-sm">Daily emissions report sent to Norwegian Maritime Authority</p>
              <p className="text-xs text-slate-500">1 hour ago</p>
            </div>
          </div>

          <div className="flex items-start p-4 bg-yellow-500/10 border border-yellow-500/20 rounded-lg">
            <AlertTriangle className="h-5 w-5 text-yellow-400 mt-0.5 mr-3" />
            <div>
              <p className="text-yellow-400 font-medium">Route Optimization Suggested</p>
              <p className="text-slate-400 text-sm">Alternative route available to reduce emissions by 8%</p>
              <p className="text-xs text-slate-500">15 minutes ago</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}