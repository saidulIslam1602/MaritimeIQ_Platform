// Real-time WebSocket service for maritime data streaming
import { useEffect, useRef, useState } from 'react'
import { RealTimeData } from '@/types/analytics'
import { MARITIME_API_URL } from '@/constants/maritime'

export interface WebSocketConfig {
  url: string
  autoReconnect: boolean
  reconnectInterval: number
  maxReconnectAttempts: number
}

export class MaritimeWebSocketService {
  private ws: WebSocket | null = null
  private listeners: Map<string, ((data: RealTimeData) => void)[]> = new Map()
  private config: WebSocketConfig
  private reconnectAttempts = 0
  private reconnectTimer: NodeJS.Timeout | null = null

  constructor(config: WebSocketConfig) {
    this.config = config
  }

  connect(): Promise<void> {
    return new Promise((resolve, reject) => {
      try {
        this.ws = new WebSocket(this.config.url)
        
        this.ws.onopen = () => {
          console.log('Maritime WebSocket connected')
          this.reconnectAttempts = 0
          resolve()
        }

        this.ws.onmessage = (event) => {
          try {
            const data: RealTimeData = JSON.parse(event.data)
            this.notifyListeners(data.type, data)
          } catch (error) {
            console.error('Failed to parse WebSocket message:', error)
          }
        }

        this.ws.onclose = () => {
          console.log('Maritime WebSocket disconnected')
          if (this.config.autoReconnect && this.reconnectAttempts < this.config.maxReconnectAttempts) {
            this.scheduleReconnect()
          }
        }

        this.ws.onerror = (error) => {
          console.error('Maritime WebSocket error:', error)
          reject(error)
        }
      } catch (error) {
        reject(error)
      }
    })
  }

  private scheduleReconnect() {
    this.reconnectTimer = setTimeout(() => {
      this.reconnectAttempts++
      console.log(`Attempting to reconnect (${this.reconnectAttempts}/${this.config.maxReconnectAttempts})`)
      this.connect().catch(console.error)
    }, this.config.reconnectInterval)
  }

  subscribe(dataType: string, callback: (data: RealTimeData) => void) {
    if (!this.listeners.has(dataType)) {
      this.listeners.set(dataType, [])
    }
    this.listeners.get(dataType)!.push(callback)
  }

  unsubscribe(dataType: string, callback: (data: RealTimeData) => void) {
    const callbacks = this.listeners.get(dataType)
    if (callbacks) {
      const index = callbacks.indexOf(callback)
      if (index > -1) {
        callbacks.splice(index, 1)
      }
    }
  }

  private notifyListeners(dataType: string, data: RealTimeData) {
    const callbacks = this.listeners.get(dataType)
    if (callbacks) {
      callbacks.forEach(callback => callback(data))
    }
  }

  disconnect() {
    if (this.reconnectTimer) {
      clearTimeout(this.reconnectTimer)
      this.reconnectTimer = null
    }
    if (this.ws) {
      this.ws.close()
      this.ws = null
    }
    this.listeners.clear()
  }

  send(data: any) {
    if (this.ws && this.ws.readyState === WebSocket.OPEN) {
      this.ws.send(JSON.stringify(data))
    }
  }
}

// React hook for real-time maritime data
export const useRealTimeData = (dataTypes: string[]) => {
  const [data, setData] = useState<Map<string, RealTimeData[]>>(new Map())
  const [isConnected, setIsConnected] = useState(false)
  const wsService = useRef<MaritimeWebSocketService | null>(null)

  useEffect(() => {
    // Initialize WebSocket service
    const config: WebSocketConfig = {
      url: 'wss://maritime-api-container.purplehill-29214279.norwayeast.azurecontainerapps.io/ws',
      autoReconnect: true,
      reconnectInterval: 5000,
      maxReconnectAttempts: 10
    }

    wsService.current = new MaritimeWebSocketService(config)

    // Connect and set up subscriptions
    wsService.current.connect()
      .then(() => {
        setIsConnected(true)
        
        // Subscribe to requested data types
        dataTypes.forEach(dataType => {
          wsService.current!.subscribe(dataType, (newData) => {
            setData(prevData => {
              const updated = new Map(prevData)
              const existing = updated.get(dataType) || []
              const limited = [...existing, newData].slice(-100) // Keep last 100 items
              updated.set(dataType, limited)
              return updated
            })
          })
        })
      })
      .catch(() => setIsConnected(false))

    return () => {
      if (wsService.current) {
        wsService.current.disconnect()
      }
    }
  }, [])

  return { data, isConnected, wsService: wsService.current }
}

// Specific hooks for different data types
export const useRealTimeVesselData = () => {
  return useRealTimeData(['vessel', 'ais'])
}

export const useRealTimeEnvironmentalData = () => {
  return useRealTimeData(['environmental'])
}

export const useRealTimeRouteData = () => {
  return useRealTimeData(['route', 'optimization'])
}

export const useRealTimeAlerts = () => {
  return useRealTimeData(['alert', 'safety'])
}