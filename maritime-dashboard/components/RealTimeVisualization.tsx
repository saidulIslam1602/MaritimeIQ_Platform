// Real-time data visualization component
import { useState, useEffect, useRef } from 'react'
import { Activity, TrendingUp, TrendingDown, Zap } from 'lucide-react'
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, AreaChart, Area } from 'recharts'
import { useRealTimeData } from '../services/websocket'
import { analyticsService, AnalyticsMetrics } from '../services/analytics'

interface RealTimeVisualizationProps {
  dataTypes: string[]
  title: string
  refreshInterval?: number
}

export default function RealTimeVisualization({ dataTypes, title, refreshInterval = 5000 }: RealTimeVisualizationProps) {
  const [analytics, setAnalytics] = useState<AnalyticsMetrics | null>(null)
  const [chartData, setChartData] = useState<any[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const { data: realTimeData, isConnected } = useRealTimeData(dataTypes)
  const chartRef = useRef<HTMLDivElement>(null)

  // Update analytics when real-time data changes
  useEffect(() => {
    if (realTimeData.size > 0) {
      realTimeData.forEach((dataPoints, dataType) => {
        dataPoints.forEach(point => analyticsService.processRealTimeData(point))
      })
      
      updateAnalytics()
    }
  }, [realTimeData])

  // Periodic analytics updates
  useEffect(() => {
    const interval = setInterval(updateAnalytics, refreshInterval)
    return () => clearInterval(interval)
  }, [refreshInterval])

  const updateAnalytics = async () => {
    try {
      const newAnalytics = await analyticsService.getAnalytics(dataTypes)
      setAnalytics(newAnalytics)
      
      // Update chart data with time series
      const timeSeriesData = analyticsService.getTimeSeriesData(dataTypes[0], 2) // Last 2 hours
      const formattedData = timeSeriesData.map((point, index) => ({
        time: new Date(point.timestamp).toLocaleTimeString('en-US', { 
          hour: '2-digit', 
          minute: '2-digit' 
        }),
        value: point.value,
        prediction: index >= timeSeriesData.length - 12 ? 
          newAnalytics.predictions.fuelConsumption[index - (timeSeriesData.length - 12)] : 
          undefined
      }))
      
      setChartData(formattedData)
      setIsLoading(false)
    } catch (error) {
      console.error('Failed to update analytics:', error)
      setIsLoading(false)
    }
  }

  const getTrendIcon = (trend: 'up' | 'down' | 'stable') => {
    switch (trend) {
      case 'up': return <TrendingUp className="w-4 h-4 text-green-500" />
      case 'down': return <TrendingDown className="w-4 h-4 text-red-500" />
      default: return <Activity className="w-4 h-4 text-blue-500" />
    }
  }

  const getTrendColor = (trend: 'up' | 'down' | 'stable') => {
    switch (trend) {
      case 'up': return 'text-green-600'
      case 'down': return 'text-red-600'
      default: return 'text-blue-600'
    }
  }

  if (isLoading) {
    return (
      <div className="bg-white rounded-lg shadow-lg p-6">
        <div className="animate-pulse">
          <div className="h-6 bg-gray-200 rounded w-1/3 mb-4"></div>
          <div className="h-64 bg-gray-200 rounded"></div>
        </div>
      </div>
    )
  }

  return (
    <div className="bg-white rounded-lg shadow-lg p-6">
      <div className="flex items-center justify-between mb-6">
        <div className="flex items-center space-x-3">
          <Activity className="w-6 h-6 text-blue-600" />
          <h3 className="text-xl font-bold text-gray-800">{title}</h3>
          <div className={`w-3 h-3 rounded-full ${isConnected ? 'bg-green-500' : 'bg-red-500'}`} />
        </div>
        <div className="text-sm text-gray-500">
          Live • Updated {new Date().toLocaleTimeString()}
        </div>
      </div>

      {/* KPI Cards */}
      {analytics && (
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
          <div className="bg-gradient-to-r from-blue-50 to-blue-100 rounded-lg p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-blue-600 font-medium">Fuel Efficiency</p>
                <p className="text-2xl font-bold text-blue-700">{analytics.kpis.fuelEfficiency.toFixed(1)}%</p>
              </div>
              <Zap className="w-8 h-8 text-blue-500" />
            </div>
          </div>
          
          <div className="bg-gradient-to-r from-green-50 to-green-100 rounded-lg p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-green-600 font-medium">Emission Reduction</p>
                <p className="text-2xl font-bold text-green-700">{analytics.kpis.emissionReduction.toFixed(1)}%</p>
              </div>
              <TrendingDown className="w-8 h-8 text-green-500" />
            </div>
          </div>
          
          <div className="bg-gradient-to-r from-purple-50 to-purple-100 rounded-lg p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-purple-600 font-medium">On-Time Performance</p>
                <p className="text-2xl font-bold text-purple-700">{analytics.kpis.onTimePerformance.toFixed(1)}%</p>
              </div>
              <Activity className="w-8 h-8 text-purple-500" />
            </div>
          </div>
          
          <div className="bg-gradient-to-r from-orange-50 to-orange-100 rounded-lg p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-orange-600 font-medium">Satisfaction</p>
                <p className="text-2xl font-bold text-orange-700">{analytics.kpis.passengerSatisfaction.toFixed(1)}%</p>
              </div>
              <TrendingUp className="w-8 h-8 text-orange-500" />
            </div>
          </div>
        </div>
      )}

      {/* Real-time Chart */}
      <div className="mb-6" ref={chartRef}>
        <h4 className="text-lg font-semibold text-gray-700 mb-3">Real-time Data Stream</h4>
        <div className="h-80">
          <ResponsiveContainer width="100%" height="100%">
            <AreaChart data={chartData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="time" />
              <YAxis />
              <Tooltip />
              <Area
                type="monotone"
                dataKey="value"
                stroke="#3B82F6"
                fill="#3B82F6"
                fillOpacity={0.3}
                strokeWidth={2}
              />
              <Line
                type="monotone"
                dataKey="prediction"
                stroke="#EF4444"
                strokeDasharray="5 5"
                strokeWidth={2}
                dot={false}
              />
            </AreaChart>
          </ResponsiveContainer>
        </div>
      </div>

      {/* Trend Analysis */}
      {analytics?.trends && (
        <div>
          <h4 className="text-lg font-semibold text-gray-700 mb-3">Trend Analysis</h4>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            {analytics.trends.map((trend, index) => (
              <div key={index} className="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
                <div className="flex items-center space-x-3">
                  {getTrendIcon(trend.trend)}
                  <div>
                    <p className="font-medium text-gray-700">{trend.name}</p>
                    <p className="text-2xl font-bold text-gray-800">{trend.value.toFixed(1)}</p>
                  </div>
                </div>
                <div className={`text-right ${getTrendColor(trend.trend)}`}>
                  <p className="text-sm font-medium">
                    {trend.change > 0 ? '+' : ''}{trend.change}%
                  </p>
                  <p className="text-xs capitalize">{trend.trend}</p>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  )
}