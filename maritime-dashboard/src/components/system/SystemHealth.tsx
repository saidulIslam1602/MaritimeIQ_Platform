import { Activity, Server, Database, Zap, CheckCircle, AlertTriangle, XCircle } from 'lucide-react'

interface SystemHealthProps {
  systemStatus: any
}

export default function SystemHealth({ systemStatus }: SystemHealthProps) {
  if (!systemStatus) {
    return (
      <div className="dashboard-card">
        <p className="text-slate-400">Loading system health data...</p>
      </div>
    )
  }

  const getStatusIcon = (status: string) => {
    switch (status.toLowerCase()) {
      case 'healthy':
      case 'available':
        return <CheckCircle className="h-5 w-5 text-green-500" />
      case 'warning':
        return <AlertTriangle className="h-5 w-5 text-yellow-500" />
      case 'error':
      case 'unhealthy':
        return <XCircle className="h-5 w-5 text-red-500" />
      default:
        return <Activity className="h-5 w-5 text-gray-500" />
    }
  }

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'healthy':
      case 'available':
        return 'text-green-400'
      case 'warning':
        return 'text-yellow-400'
      case 'error':
      case 'unhealthy':
        return 'text-red-400'
      default:
        return 'text-gray-400'
    }
  }

  return (
    <div className="space-y-6">
      {/* Overall System Status */}
      <div className="dashboard-card">
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-xl font-semibold text-white">System Overview</h2>
          <div className="flex items-center">
            {getStatusIcon(systemStatus.overallStatus)}
            <span className={`ml-2 font-medium ${getStatusColor(systemStatus.overallStatus)}`}>
              {systemStatus.overallStatus}
            </span>
          </div>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <div className="metric-card">
            <div className="flex items-center">
              <Activity className="h-8 w-8 text-blue-500 mr-3" />
              <div>
                <p className="text-2xl font-bold text-white">
                  {systemStatus.performance?.cpuUsage?.toFixed(1) || 'N/A'}%
                </p>
                <p className="text-slate-400">CPU Usage</p>
              </div>
            </div>
          </div>

          <div className="metric-card">
            <div className="flex items-center">
              <Server className="h-8 w-8 text-purple-500 mr-3" />
              <div>
                <p className="text-2xl font-bold text-white">
                  {systemStatus.performance?.memoryUsage?.toFixed(1) || 'N/A'}%
                </p>
                <p className="text-slate-400">Memory Usage</p>
              </div>
            </div>
          </div>

          <div className="metric-card">
            <div className="flex items-center">
              <Zap className="h-8 w-8 text-yellow-500 mr-3" />
              <div>
                <p className="text-2xl font-bold text-white">
                  {systemStatus.performance?.requestsPerSecond || 'N/A'}
                </p>
                <p className="text-slate-400">Requests/sec</p>
              </div>
            </div>
          </div>

          <div className="metric-card">
            <div className="flex items-center">
              <Database className="h-8 w-8 text-green-500 mr-3" />
              <div>
                <p className="text-2xl font-bold text-white">
                  {systemStatus.performance?.activeConnections || 'N/A'}
                </p>
                <p className="text-slate-400">Active Connections</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Services Status */}
      <div className="dashboard-card">
        <h3 className="text-lg font-semibold text-white mb-6">Service Health</h3>
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
          {systemStatus.services?.map((service: any, index: number) => (
            <div key={index} className="bg-slate-700 rounded-lg p-4 border border-slate-600">
              <div className="flex items-center justify-between mb-2">
                <h4 className="font-semibold text-white">{service.serviceName}</h4>
                <div className="flex items-center">
                  {getStatusIcon(service.status)}
                  <span className={`ml-2 text-sm ${getStatusColor(service.status)}`}>
                    {service.status}
                  </span>
                </div>
              </div>
              <p className="text-slate-400 text-sm mb-2">{service.details}</p>
              <div className="grid grid-cols-2 gap-4 text-xs">
                <div>
                  <p className="text-slate-500">Response Time</p>
                  <p className="text-white">{service.responseTime || 'N/A'}</p>
                </div>
                <div>
                  <p className="text-slate-500">Version</p>
                  <p className="text-white">{service.version || 'N/A'}</p>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Dependencies Status */}
      <div className="dashboard-card">
        <h3 className="text-lg font-semibold text-white mb-6">External Dependencies</h3>
        <div className="space-y-3">
          {systemStatus.dependencies?.map((dependency: any, index: number) => (
            <div key={index} className="flex items-center justify-between p-3 bg-slate-700 rounded-lg border border-slate-600">
              <div className="flex items-center">
                {getStatusIcon(dependency.status)}
                <div className="ml-3">
                  <p className="text-white font-medium">{dependency.name}</p>
                  <p className="text-slate-400 text-sm">{dependency.endpoint}</p>
                </div>
              </div>
              <div className="text-right">
                <p className={`text-sm font-medium ${getStatusColor(dependency.status)}`}>
                  {dependency.status}
                </p>
                <p className="text-slate-400 text-xs">
                  {dependency.responseTime || 'N/A'}
                </p>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Infrastructure Status */}
      {systemStatus.infrastructure && (
        <div className="dashboard-card">
          <h3 className="text-lg font-semibold text-white mb-6">Infrastructure Status</h3>
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            {/* Container Apps Environment */}
            {systemStatus.infrastructure.containerAppsEnvironment && (
              <div className="bg-slate-700 rounded-lg p-4 border border-slate-600">
                <h4 className="font-semibold text-white mb-3">Container Apps Environment</h4>
                <div className="space-y-2 text-sm">
                  <div className="flex justify-between">
                    <span className="text-slate-400">Name:</span>
                    <span className="text-white">{systemStatus.infrastructure.containerAppsEnvironment.name}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-slate-400">Status:</span>
                    <span className="text-green-400">{systemStatus.infrastructure.containerAppsEnvironment.status}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-slate-400">Active Replicas:</span>
                    <span className="text-white">{systemStatus.infrastructure.containerAppsEnvironment.activeReplicas}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-slate-400">CPU Utilization:</span>
                    <span className="text-white">{systemStatus.infrastructure.containerAppsEnvironment.cpuUtilization}%</span>
                  </div>
                </div>
              </div>
            )}

            {/* Load Balancer */}
            {systemStatus.infrastructure.loadBalancer && (
              <div className="bg-slate-700 rounded-lg p-4 border border-slate-600">
                <h4 className="font-semibold text-white mb-3">Load Balancer</h4>
                <div className="space-y-2 text-sm">
                  <div className="flex justify-between">
                    <span className="text-slate-400">Status:</span>
                    <span className="text-green-400">{systemStatus.infrastructure.loadBalancer.status}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-slate-400">Active Backends:</span>
                    <span className="text-white">{systemStatus.infrastructure.loadBalancer.activeBackends}/{systemStatus.infrastructure.loadBalancer.totalBackends}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-slate-400">Requests/min:</span>
                    <span className="text-white">{systemStatus.infrastructure.loadBalancer.requestsPerMinute?.toLocaleString()}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-slate-400">Failed Health Checks:</span>
                    <span className="text-white">{systemStatus.infrastructure.loadBalancer.failedHealthChecks}</span>
                  </div>
                </div>
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  )
}