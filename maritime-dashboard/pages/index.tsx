import { useState, useEffect } from 'react'
import { Ship, MapPin, Activity, AlertTriangle, CheckCircle, TrendingUp, Waves, Wind, Thermometer, Zap } from 'lucide-react'
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
      const API_URL = typeof window !== 'undefined' 
        ? 'https://maritime-platform.icystone-47eb4b00.norwayeast.azurecontainerapps.io'
        : process.env.NEXT_PUBLIC_MARITIME_API_URL || 'https://maritime-platform.icystone-47eb4b00.norwayeast.azurecontainerapps.io'
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

      {/* Navigation */}
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
        {activeTab === 'realtime' && (
          <div className="space-y-6">
            <RealTimeVisualization 
              dataTypes={['vessel', 'ais']} 
              title="Fleet Performance Analytics"
              refreshInterval={5000}
            />
            <RealTimeVisualization 
              dataTypes={['environmental']} 
              title="Environmental Compliance Monitoring"
              refreshInterval={10000}
            />
            <RealTimeVisualization 
              dataTypes={['route', 'optimization']} 
              title="Route Optimization Insights"
              refreshInterval={15000}
            />
          </div>
        )}
        {activeTab === 'routes' && <RouteOptimization />}
        {activeTab === 'environment' && <EnvironmentalMonitoring />}
        {activeTab === 'system' && <SystemHealth systemStatus={systemStatus} />}
      </main>

      {/* Footer */}
      <footer className="bg-slate-900 border-t border-slate-700 mt-12">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
          <div className="flex justify-between items-center">
            <div className="text-slate-400">
              © 2025 Havila Kystruten. Maritime Platform v2.0.0
            </div>
            <div className="flex items-center text-slate-400">
              <Activity className="h-4 w-4 mr-2" />
              Bergen-Kirkenes Coastal Route
            </div>
          </div>
        </div>
      </footer>
    </div>
  )
}