// Simple API configuration
const API_BASE_URL = 'https://maritime-platform.icystone-47eb4b00.norwayeast.azurecontainerapps.io';

// Export for use in components
if (typeof window !== 'undefined') {
  (window as any).MARITIME_API_URL = API_BASE_URL;
}