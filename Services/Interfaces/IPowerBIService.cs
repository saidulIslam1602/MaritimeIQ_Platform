using MaritimeIQ.Platform.Models.PowerBI;

namespace MaritimeIQ.Platform.Services.Interfaces
{
    public interface IPowerBIService
    {
        // Dashboard Management
        Task<List<MaritimeDashboard>> GetMaritimeDashboardsAsync();
        Task<MaritimeDashboard?> GetDashboardByIdAsync(string dashboardId);
        Task<PowerBIEmbedConfig> GetEmbedTokenAsync(string dashboardId, string userId = "");
        Task<bool> RefreshDashboardAsync(string dashboardId);

        // KPI and Analytics
        Task<MaritimeKPISummary> GetKPISummaryAsync();
        Task<NorthernLightsForecast> GetNorthernLightsForecastAsync();
        Task<Dictionary<string, object>> GetReportDataAsync(string reportId);

        // Reports Management
        Task<List<MaritimePowerBIReport>> GetReportsAsync();
        Task<MaritimePowerBIReport?> GetReportByIdAsync(string reportId);
        Task<MaritimePowerBIUsageMetrics> GetUsageMetricsAsync(string reportId, DateTime startDate, DateTime endDate);

        // Dataset Management
        Task<List<MaritimePowerBIDataset>> GetDatasetsAsync();
        Task<MaritimePowerBIDataset?> GetDatasetByIdAsync(string datasetId);
        Task<MaritimePowerBIRefreshResult> RefreshDatasetAsync(string datasetId);
        Task<bool> UpdateDatasetAsync(string datasetId, Dictionary<string, object> data);

        // Tile Management
        Task<List<DashboardTile>> GetDashboardTilesAsync(string dashboardId);
        Task<DashboardTile?> GetTileByIdAsync(string tileId);
        Task<bool> UpdateTileAsync(string tileId, DashboardTile tile);

        // Authentication and Configuration
        Task<bool> ValidateConnectionAsync();
        Task<string> GetAccessTokenAsync();
        Task<Dictionary<string, object>> GetWorkspaceInfoAsync();

        // Utilities
        Task<List<string>> GetAvailableDataSourcesAsync();
        Task<bool> TestDataConnectionAsync(string dataSourceId);
        Task<Dictionary<string, object>> GetSystemHealthAsync();
    }
}