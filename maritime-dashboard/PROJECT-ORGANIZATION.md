# Maritime Dashboard - Project Organization

## 📁 Directory Structure

The maritime dashboard has been reorganized following Next.js best practices and feature-based organization:

```
maritime-dashboard/
├── src/ # Main source directory
│ ├── components/ # React components by feature
│ │ ├── fleet/ # Fleet management components
│ │ │ └── FleetDashboard.tsx
│ │ ├── environmental/ # Environmental monitoring
│ │ │ └── EnvironmentalMonitoring.tsx
│ │ ├── analytics/ # Analytics and visualization
│ │ │ └── RealTimeVisualization.tsx
│ │ ├── optimization/ # Route optimization
│ │ │ └── RouteOptimization.tsx
│ │ ├── system/ # System health monitoring
│ │ │ └── SystemHealth.tsx
│ │ ├── ui/ # Shared UI components
│ │ └── index.ts # Component exports
│ ├── services/ # External services and APIs
│ │ ├── analytics/ # Analytics processing
│ │ │ └── analytics.ts
│ │ ├── messaging/ # WebSocket and EventHub services
│ │ │ ├── websocket.ts
│ │ │ └── eventhub.ts
│ │ ├── api/ # REST API services
│ │ └── index.ts # Service exports
│ ├── hooks/ # Custom React hooks
│ │ ├── common/ # General-purpose hooks
│ │ │ └── useCommon.ts
│ │ ├── maritime/ # Maritime-specific hooks
│ │ │ └── useMaritime.ts
│ │ └── index.ts # Hook exports
│ ├── types/ # TypeScript type definitions
│ │ ├── maritime.ts # Maritime domain types
│ │ ├── analytics.ts # Analytics and metrics types
│ │ ├── environment.ts # Environmental monitoring types
│ │ ├── env.d.ts # Environment variables
│ │ └── index.ts # Type exports
│ ├── constants/ # Application constants
│ │ └── maritime.ts # Maritime-specific constants
│ ├── utils/ # Utility functions
│ │ ├── formatters.ts # Data formatting utilities
│ │ └── calculations.ts # Maritime calculations
│ └── lib/ # External library configurations
│ └── config.ts # App configuration
├── pages/ # Next.js pages
│ ├── _app.tsx # App wrapper
│ └── index.tsx # Main dashboard page
├── styles/ # Global styles
│ └── globals.css
└── config files... # Various configuration files
```

## Key Improvements

### 1. **Feature-Based Organization**
- Components are grouped by business domain (fleet, environmental, analytics, etc.)
- Related functionality is co-located for better maintainability

### 2. **Centralized Type Definitions**
- All TypeScript types are consolidated in the `src/types/` directory
- No more duplicate type definitions across files
- Domain-specific type files for better organization

### 3. **Enhanced Path Mapping**
- Updated `tsconfig.json` with comprehensive path aliases
- Clean imports using `@/` prefix throughout the codebase
- Easy navigation and refactoring

### 4. **Service Layer Organization**
- Services categorized by functionality (analytics, messaging, api)
- Clear separation of concerns
- Easier testing and mocking

### 5. **Utility Functions**
- Common formatting and calculation utilities
- Reusable maritime-specific functions
- Constants file for configuration values

## 📋 Path Aliases

The following path aliases are configured in `tsconfig.json`:

```json
{
 "@/*": ["./src/*"],
 "@/components/*": ["./src/components/*"],
 "@/services/*": ["./src/services/*"],
 "@/hooks/*": ["./src/hooks/*"],
 "@/types/*": ["./src/types/*"],
 "@/utils/*": ["./src/utils/*"],
 "@/constants/*": ["./src/constants/*"],
 "@/lib/*": ["./src/lib/*"]
}
```

## Usage Examples

### Importing Components
```typescript
// Using component index
import { FleetDashboard, SystemHealth } from '@/components'// Direct import
import FleetDashboard from '@/components/fleet/FleetDashboard'```

### Importing Types
```typescript
// Using type index
import { Vessel, FleetData, AnalyticsMetrics } from '@/types'// Direct import
import { Vessel } from '@/types/maritime'```

### Importing Services
```typescript
// Using service index
import { analyticsService } from '@/services'// Direct import
import { analyticsService } from '@/services/analytics/analytics'```

### Importing Utilities
```typescript
import { formatNumber, formatDateTime } from '@/utils/formatters'import { calculateFuelEfficiency } from '@/utils/calculations'import { MARITIME_FLEET, EMISSION_LIMITS } from '@/constants/maritime'```

## Benefits

1. **Improved Maintainability**: Feature-based organization makes it easier to locate and modify related code
2. **Better Type Safety**: Centralized type definitions prevent inconsistencies and duplication 
3. **Enhanced Developer Experience**: Clear import paths and organized structure improve code readability
4. **Scalability**: Structure supports easy addition of new features and components
5. **Testing**: Organized structure makes unit testing and mocking easier
6. **Code Reusability**: Shared utilities and constants promote DRY principles

## 🔄 Migration Guide

When adding new features:

1. **Components**: Place in appropriate feature directory under `src/components/`
2. **Types**: Add to relevant type file or create new domain-specific type file
3. **Services**: Group by functionality in `src/services/`
4. **Utilities**: Add reusable functions to `src/utils/`
5. **Constants**: Add configuration values to `src/constants/`

## Next Steps

- Consider adding a `src/contexts/` directory for React contexts
- Add `src/providers/` for provider components
- Consider `src/layouts/` for page layouts
- Add comprehensive unit tests following the organized structure