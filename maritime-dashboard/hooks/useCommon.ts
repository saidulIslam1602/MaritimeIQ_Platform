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
 * Custom hook for loading state management
 */
export function useLoadingState(initialState: boolean = true) {
  const [isLoading, setIsLoading] = useState(initialState)

  const startLoading = () => setIsLoading(true)
  const stopLoading = () => setIsLoading(false)

  return { isLoading, startLoading, stopLoading, setIsLoading }
}

/**
 * Custom hook for error handling
 */
export function useErrorHandler() {
  const [error, setError] = useState<string | null>(null)

  const handleError = (err: Error | string) => {
    const errorMessage = err instanceof Error ? err.message : err
    console.error('Component Error:', errorMessage)
    setError(errorMessage)
  }

  const clearError = () => setError(null)

  return { error, handleError, clearError }
}

/**
 * Custom hook for status indicators
 */
export function useStatusIndicator() {
  const getStatusIcon = (status: string) => {
    switch (status.toLowerCase()) {
      case 'healthy':
      case 'available':
      case 'online':
        return 'check-circle'
      case 'warning':
      case 'degraded':
        return 'alert-triangle'
      case 'error':
      case 'unhealthy':
      case 'offline':
        return 'x-circle'
      default:
        return 'activity'
    }
  }

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'healthy':
      case 'available':
      case 'online':
        return 'text-green-400'
      case 'warning':
      case 'degraded':
        return 'text-yellow-400'
      case 'error':
      case 'unhealthy':
      case 'offline':
        return 'text-red-400'
      default:
        return 'text-gray-400'
    }
  }

  const getStatusBadgeColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'healthy':
      case 'available':
      case 'online':
        return 'bg-green-500/10 text-green-400 border-green-500/20'
      case 'warning':
      case 'degraded':
        return 'bg-yellow-500/10 text-yellow-400 border-yellow-500/20'
      case 'error':
      case 'unhealthy':
      case 'offline':
        return 'bg-red-500/10 text-red-400 border-red-500/20'
      default:
        return 'bg-gray-500/10 text-gray-400 border-gray-500/20'
    }
  }

  return { getStatusIcon, getStatusColor, getStatusBadgeColor }
}

/**
 * Custom hook for local storage
 */
export function useLocalStorage<T>(key: string, initialValue: T) {
  const [storedValue, setStoredValue] = useState<T>(() => {
    try {
      if (typeof window === 'undefined') return initialValue
      const item = window.localStorage.getItem(key)
      return item ? JSON.parse(item) : initialValue
    } catch (error) {
      console.error(`Error reading localStorage key "${key}":`, error)
      return initialValue
    }
  })

  const setValue = (value: T | ((val: T) => T)) => {
    try {
      const valueToStore = value instanceof Function ? value(storedValue) : value
      setStoredValue(valueToStore)
      if (typeof window !== 'undefined') {
        window.localStorage.setItem(key, JSON.stringify(valueToStore))
      }
    } catch (error) {
      console.error(`Error setting localStorage key "${key}":`, error)
    }
  }

  return [storedValue, setValue] as const
}

/**
 * Custom hook for metrics calculations
 */
export function useMetricsCalculator() {
  const calculatePercentage = (current: number, total: number): number => {
    if (total === 0) return 0
    return Math.round((current / total) * 100)
  }

  const calculateAverage = (values: number[]): number => {
    if (values.length === 0) return 0
    return values.reduce((sum, value) => sum + value, 0) / values.length
  }

  const calculateTrend = (current: number, previous: number): 'up' | 'down' | 'stable' => {
    const threshold = 0.01 // 1% threshold for considering stable
    const percentChange = Math.abs((current - previous) / previous)
    
    if (percentChange < threshold) return 'stable'
    return current > previous ? 'up' : 'down'
  }

  const formatMetric = (value: number, decimals: number = 2): string => {
    if (value >= 1000000) {
      return `${(value / 1000000).toFixed(decimals)}M`
    }
    if (value >= 1000) {
      return `${(value / 1000).toFixed(decimals)}K`
    }
    return value.toFixed(decimals)
  }

  return { calculatePercentage, calculateAverage, calculateTrend, formatMetric }
}