import { MapPin, Navigation, Clock, TrendingUp } from 'lucide-react'

export default function RouteOptimization() {
  const routes = [
    {
      id: 1,
      name: 'Bergen-Kirkenes Northbound',
      status: 'Active',
      efficiency: 92,
      estimatedTime: '6d 15h',
      fuelSaving: '15%',
      ports: ['Bergen', 'Ålesund', 'Geiranger', 'Trondheim', 'Bodø', 'Tromsø', 'Kirkenes']
    },
    {
      id: 2,
      name: 'Kirkenes-Bergen Southbound', 
      status: 'Planned',
      efficiency: 88,
      estimatedTime: '6d 12h',
      fuelSaving: '12%',
      ports: ['Kirkenes', 'Tromsø', 'Bodø', 'Trondheim', 'Geiranger', 'Ålesund', 'Bergen']
    }
  ]

  return (
    <div className="space-y-6">
      {/* Route Overview */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="metric-card">
          <div className="flex items-center">
            <MapPin className="h-8 w-8 text-blue-500 mr-3" />
            <div>
              <p className="text-2xl font-bold text-white">34</p>
              <p className="text-slate-400">Active Ports</p>
            </div>
          </div>
        </div>

        <div className="metric-card">
          <div className="flex items-center">
            <Navigation className="h-8 w-8 text-green-500 mr-3" />
            <div>
              <p className="text-2xl font-bold text-white">2,518</p>
              <p className="text-slate-400">Nautical Miles</p>
            </div>
          </div>
        </div>

        <div className="metric-card">
          <div className="flex items-center">
            <Clock className="h-8 w-8 text-yellow-500 mr-3" />
            <div>
              <p className="text-2xl font-bold text-white">13.5h</p>
              <p className="text-slate-400">Avg Optimization</p>
            </div>
          </div>
        </div>

        <div className="metric-card">
          <div className="flex items-center">
            <TrendingUp className="h-8 w-8 text-purple-500 mr-3" />
            <div>
              <p className="text-2xl font-bold text-white">18%</p>
              <p className="text-slate-400">Fuel Efficiency</p>
            </div>
          </div>
        </div>
      </div>

      {/* Route Details */}
      <div className="dashboard-card">
        <h2 className="text-xl font-semibold text-white mb-6">Route Optimization</h2>
        <div className="space-y-4">
          {routes.map((route) => (
            <div key={route.id} className="bg-slate-700 rounded-lg p-6 border border-slate-600">
              <div className="flex justify-between items-start mb-4">
                <div>
                  <h3 className="text-lg font-semibold text-white">{route.name}</h3>
                  <div className="flex items-center mt-2">
                    <div className={`w-2 h-2 rounded-full mr-2 ${route.status === 'Active' ? 'bg-green-500' : 'bg-yellow-500'}`}></div>
                    <span className="text-sm text-slate-300">{route.status}</span>
                  </div>
                </div>
                <div className="text-right">
                  <p className="text-2xl font-bold text-white">{route.efficiency}%</p>
                  <p className="text-slate-400 text-sm">Efficiency</p>
                </div>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
                <div>
                  <p className="text-slate-400 text-sm">Estimated Time</p>
                  <p className="text-white font-semibold">{route.estimatedTime}</p>
                </div>
                <div>
                  <p className="text-slate-400 text-sm">Fuel Saving</p>
                  <p className="text-green-400 font-semibold">{route.fuelSaving}</p>
                </div>
                <div>
                  <p className="text-slate-400 text-sm">Ports</p>
                  <p className="text-white font-semibold">{route.ports.length} stops</p>
                </div>
              </div>

              <div className="border-t border-slate-600 pt-4">
                <p className="text-slate-400 text-sm mb-2">Route Ports:</p>
                <div className="flex flex-wrap gap-2">
                  {route.ports.map((port, index) => (
                    <span key={index} className="px-3 py-1 bg-slate-600 rounded-full text-xs text-white">
                      {port}
                    </span>
                  ))}
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* AI Recommendations */}
      <div className="dashboard-card">
        <h3 className="text-lg font-semibold text-white mb-6">AI Route Recommendations</h3>
        <div className="space-y-4">
          <div className="flex items-start p-4 bg-blue-500/10 border border-blue-500/20 rounded-lg">
            <div className="w-2 h-2 bg-blue-400 rounded-full mt-2 mr-3"></div>
            <div>
              <p className="text-blue-400 font-medium">Weather Optimization Available</p>
              <p className="text-slate-400 text-sm">Alternative route through Lofoten can save 2.5 hours due to favorable winds</p>
              <p className="text-xs text-slate-500">Recommended for MS Nordic Aurora - departing in 4 hours</p>
            </div>
          </div>

          <div className="flex items-start p-4 bg-green-500/10 border border-green-500/20 rounded-lg">
            <div className="w-2 h-2 bg-green-400 rounded-full mt-2 mr-3"></div>
            <div>
              <p className="text-green-400 font-medium">Fuel Efficiency Improvement</p>
              <p className="text-slate-400 text-sm">Speed reduction by 2 knots in open waters can improve fuel efficiency by 8%</p>
              <p className="text-xs text-slate-500">Applied to all vessels automatically</p>
            </div>
          </div>

          <div className="flex items-start p-4 bg-purple-500/10 border border-purple-500/20 rounded-lg">
            <div className="w-2 h-2 bg-purple-400 rounded-full mt-2 mr-3"></div>
            <div>
              <p className="text-purple-400 font-medium">Northern Lights Route</p>
              <p className="text-slate-400 text-sm">Clear skies predicted for Tromsø-Kirkenes segment - optimal for aurora viewing</p>
              <p className="text-xs text-slate-500">Passenger notification sent to 342 guests</p>
            </div>
          </div>
        </div>
      </div>

      {/* Map Placeholder */}
      <div className="dashboard-card">
        <h3 className="text-lg font-semibold text-white mb-6">Live Route Tracking</h3>
        <div className="bg-slate-800 rounded-lg h-96 flex items-center justify-center border border-slate-600">
          <div className="text-center text-slate-400">
            <MapPin className="h-16 w-16 mx-auto mb-4 opacity-50" />
            <p className="text-lg">Interactive Route Map</p>
            <p className="text-sm mt-2">Real-time vessel positions and route optimization</p>
            <p className="text-xs mt-1">Norwegian Coastal Route: Bergen ↔ Kirkenes</p>
          </div>
        </div>
      </div>

      {/* Performance Metrics */}
      <div className="dashboard-card">
        <h3 className="text-lg font-semibold text-white mb-6">Route Performance Metrics</h3>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          <div className="bg-slate-700 rounded-lg p-4 text-center">
            <p className="text-2xl font-bold text-white">98.2%</p>
            <p className="text-slate-400 text-sm">On-time Arrivals</p>
          </div>
          <div className="bg-slate-700 rounded-lg p-4 text-center">
            <p className="text-2xl font-bold text-white">4.6</p>
            <p className="text-slate-400 text-sm">Avg Delay (min)</p>
          </div>
          <div className="bg-slate-700 rounded-lg p-4 text-center">
            <p className="text-2xl font-bold text-white">15.3%</p>
            <p className="text-slate-400 text-sm">Fuel Savings</p>
          </div>
          <div className="bg-slate-700 rounded-lg p-4 text-center">
            <p className="text-2xl font-bold text-white">4.8/5</p>
            <p className="text-slate-400 text-sm">Passenger Rating</p>
          </div>
        </div>
      </div>
    </div>
  )
}