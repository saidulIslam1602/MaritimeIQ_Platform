// Utility functions for formatting data

export const formatNumber = (num: number, decimals: number = 2): string => {
  return num.toLocaleString('en-US', {
    minimumFractionDigits: decimals,
    maximumFractionDigits: decimals
  })
}

export const formatPercentage = (value: number, total: number = 100): string => {
  const percentage = (value / total) * 100
  return `${percentage.toFixed(1)}%`
}

export const formatDateTime = (date: string | Date): string => {
  const d = new Date(date)
  return d.toLocaleString('en-US', {
    year: 'numeric',
    month: 'short',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}

export const formatTime = (date: string | Date): string => {
  const d = new Date(date)
  return d.toLocaleTimeString('en-US', {
    hour: '2-digit',
    minute: '2-digit'
  })
}

export const formatDistance = (meters: number): string => {
  if (meters < 1000) {
    return `${meters}m`
  }
  const km = meters / 1000
  return `${km.toFixed(1)}km`
}

export const formatSpeed = (knots: number): string => {
  return `${knots.toFixed(1)} kn`
}

export const formatFuelConsumption = (liters: number): string => {
  return `${formatNumber(liters, 0)} L`
}

export const formatEmission = (value: number, unit: string): string => {
  return `${formatNumber(value)} ${unit}`
}

export const formatUptime = (seconds: number): string => {
  const days = Math.floor(seconds / (24 * 3600))
  const hours = Math.floor((seconds % (24 * 3600)) / 3600)
  const minutes = Math.floor((seconds % 3600) / 60)
  
  if (days > 0) {
    return `${days}d ${hours}h`
  }
  if (hours > 0) {
    return `${hours}h ${minutes}m`
  }
  return `${minutes}m`
}

export const formatCurrency = (amount: number, currency: string = 'NOK'): string => {
  return new Intl.NumberFormat('no-NO', {
    style: 'currency',
    currency: currency
  }).format(amount)
}

export const capitalizeFirst = (str: string): string => {
  return str.charAt(0).toUpperCase() + str.slice(1)
}

export const truncateText = (text: string, maxLength: number): string => {
  if (text.length <= maxLength) return text
  return text.slice(0, maxLength) + '...'
}