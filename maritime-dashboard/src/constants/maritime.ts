// Maritime platform constants

export const MARITIME_API_URL = process.env.NEXT_PUBLIC_MARITIME_API_URL || 
  'https://maritime-api-container.purplehill-29214279.norwayeast.azurecontainerapps.io'

export const MARITIME_FLEET = {
  NORDIC_EXPLORER: { id: 1, name: 'MS Nordic Explorer', capacity: 640 },
  ARCTIC_SPIRIT: { id: 2, name: 'MS Arctic Spirit', capacity: 640 },
  COASTAL_AURORA: { id: 3, name: 'MS Coastal Aurora', capacity: 640 },
  FJORD_VOYAGER: { id: 4, name: 'MS Fjord Voyager', capacity: 640 }
} as const

export const NORWEGIAN_PORTS = [
  'Bergen', 'Florø', 'Måløy', 'Torvik', 'Ålesund', 'Geiranger', 
  'Stryn', 'Olden', 'Nordfjordeid', 'Måloy', 'Torvik', 'Ålesund'
] as const

export const EMISSION_LIMITS = {
  CO2_DAILY_LIMIT: 500, // tons
  NOX_LIMIT: 2000, // mg/Nm³
  SOX_LIMIT: 0.1, // % m/m
  PARTICULATE_LIMIT: 0.1 // g/kWh
} as const

export const COMPLIANCE_ZONES = {
  NECA: 'Nordic Emission Control Area',
  SECA: 'Sulfur Emission Control Area', 
  ECA: 'Emission Control Area',
  IMO_2020: 'IMO 2020 Global Sulfur Cap'
} as const

export const UPDATE_INTERVALS = {
  REAL_TIME: 1000, // 1 second
  FAST: 5000, // 5 seconds
  NORMAL: 30000, // 30 seconds
  SLOW: 60000 // 1 minute
} as const

export const VESSEL_STATUS = {
  ACTIVE: 'active',
  MAINTENANCE: 'maintenance',
  DOCKED: 'docked',
  EMERGENCY: 'emergency'
} as const

export const SERVICE_STATUS = {
  ONLINE: 'online',
  OFFLINE: 'offline',
  DEGRADED: 'degraded'
} as const