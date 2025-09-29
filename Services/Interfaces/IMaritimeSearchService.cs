using MaritimeIQ.Platform.Models;

namespace MaritimeIQ.Platform.Services.Interfaces
{
    /// <summary>
    /// Service interface for maritime search capabilities and intelligence
    /// </summary>
    public interface IMaritimeSearchService
    {
        /// <summary>
        /// Search for vessels by various criteria
        /// </summary>
        Task<VesselSearchResult> SearchVesselsAsync(VesselSearchQuery query);

        /// <summary>
        /// Search for routes and route information
        /// </summary>
        Task<RouteSearchResult> SearchRoutesAsync(RouteSearchQuery query);

        /// <summary>
        /// Search for ports and maritime facilities
        /// </summary>
        Task<PortSearchResult> SearchPortsAsync(PortSearchQuery query);

        /// <summary>
        /// Search maritime regulations and compliance information
        /// </summary>
        Task<RegulationSearchResult> SearchRegulationsAsync(RegulationSearchQuery query);

        /// <summary>
        /// Search historical maritime data and analytics
        /// </summary>
        Task<HistoricalDataSearchResult> SearchHistoricalDataAsync(HistoricalDataQuery query);

        /// <summary>
        /// Search for weather and environmental data
        /// </summary>
        Task<WeatherSearchResult> SearchWeatherDataAsync(WeatherSearchQuery query);

        /// <summary>
        /// Search for safety incidents and reports
        /// </summary>
        Task<SafetyIncidentSearchResult> SearchSafetyIncidentsAsync(SafetyIncidentQuery query);

        /// <summary>
        /// Search for cargo and manifest information
        /// </summary>
        Task<CargoSearchResult> SearchCargoAsync(CargoSearchQuery query);

        /// <summary>
        /// Perform full-text search across all maritime data
        /// </summary>
        Task<FullTextSearchResult> PerformFullTextSearchAsync(string searchTerm, SearchOptions options);

        /// <summary>
        /// Get search suggestions and auto-complete
        /// </summary>
        Task<SearchSuggestionsResult> GetSearchSuggestionsAsync(string partialTerm, string category);

        /// <summary>
        /// Search for maritime events and activities
        /// </summary>
        Task<EventSearchResult> SearchMaritimeEventsAsync(EventSearchQuery query);

        /// <summary>
        /// Advanced search with machine learning capabilities
        /// </summary>
        Task<AISearchResult> PerformAISearchAsync(string naturalLanguageQuery);
    }

    #region Search Query Models

    public class VesselSearchQuery
    {
        public string? VesselName { get; set; }
        public string? IMO { get; set; }
        public string? MMSI { get; set; }
        public string? CallSign { get; set; }
        public string? VesselType { get; set; }
        public string? Flag { get; set; }
        public DateRange? LastSeenRange { get; set; }
        public GeographicBounds? Area { get; set; }
        public int MaxResults { get; set; } = 100;
        public string? SortBy { get; set; }
        public bool IncludeHistoricalData { get; set; } = false;
    }

    public class RouteSearchQuery
    {
        public string? RouteName { get; set; }
        public string? DeparturePort { get; set; }
        public string? ArrivalPort { get; set; }
        public string? RouteType { get; set; }
        public DateRange? DateRange { get; set; }
        public double? MaxDistance { get; set; }
        public TimeSpan? MaxDuration { get; set; }
        public int MaxResults { get; set; } = 50;
        public bool IncludeWeatherData { get; set; } = false;
    }

    public class PortSearchQuery
    {
        public string? PortName { get; set; }
        public string? UNLocode { get; set; }
        public string? Country { get; set; }
        public string? Region { get; set; }
        public List<string>? Facilities { get; set; }
        public GeographicBounds? Area { get; set; }
        public int MaxResults { get; set; } = 100;
        public bool IncludeFacilityDetails { get; set; } = true;
    }

    public class RegulationSearchQuery
    {
        public string? RegulationType { get; set; }
        public string? Jurisdiction { get; set; }
        public string? VesselType { get; set; }
        public List<string>? Keywords { get; set; }
        public DateRange? EffectiveDateRange { get; set; }
        public int MaxResults { get; set; } = 50;
        public bool IncludeHistorical { get; set; } = false;
    }

    public class HistoricalDataQuery
    {
        public string DataType { get; set; } = string.Empty;
        public string? EntityId { get; set; }
        public DateRange DateRange { get; set; } = new();
        public GeographicBounds? Area { get; set; }
        public Dictionary<string, object>? Filters { get; set; }
        public string? AggregationType { get; set; }
        public int MaxResults { get; set; } = 1000;
    }

    public class WeatherSearchQuery
    {
        public GeographicBounds Area { get; set; } = new();
        public DateRange DateRange { get; set; } = new();
        public List<string>? WeatherParameters { get; set; }
        public double? MinSeverity { get; set; }
        public List<string>? WeatherTypes { get; set; }
        public int MaxResults { get; set; } = 200;
    }

    public class SafetyIncidentQuery
    {
        public string? IncidentType { get; set; }
        public string? Severity { get; set; }
        public string? VesselId { get; set; }
        public DateRange? DateRange { get; set; }
        public GeographicBounds? Area { get; set; }
        public string? Status { get; set; }
        public int MaxResults { get; set; } = 100;
        public bool IncludeResolved { get; set; } = true;
    }

    public class CargoSearchQuery
    {
        public string? CargoType { get; set; }
        public string? VesselId { get; set; }
        public string? OriginPort { get; set; }
        public string? DestinationPort { get; set; }
        public DateRange? ShipmentDateRange { get; set; }
        public double? MinWeight { get; set; }
        public double? MaxWeight { get; set; }
        public int MaxResults { get; set; } = 100;
    }

    public class EventSearchQuery
    {
        public string? EventType { get; set; }
        public string? Category { get; set; }
        public DateRange? DateRange { get; set; }
        public GeographicBounds? Area { get; set; }
        public string? EntityId { get; set; }
        public List<string>? Tags { get; set; }
        public int MaxResults { get; set; } = 100;
    }

    public class SearchOptions
    {
        public List<string>? Categories { get; set; }
        public int MaxResults { get; set; } = 100;
        public bool IncludeHighlights { get; set; } = true;
        public bool IncludeFacets { get; set; } = true;
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = true;
    }

    #endregion

    #region Search Result Models

    public class VesselSearchResult : BaseSearchResult
    {
        public List<VesselResult> Vessels { get; set; } = new();
    }

    public class VesselResult
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string IMO { get; set; } = string.Empty;
        public string MMSI { get; set; } = string.Empty;
        public string CallSign { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public Position? LastKnownPosition { get; set; }
        public DateTime? LastSeen { get; set; }
        public double SearchScore { get; set; }
    }

    public class RouteSearchResult : BaseSearchResult
    {
        public List<RouteResult> Routes { get; set; } = new();
    }

    public class RouteResult
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DeparturePort { get; set; } = string.Empty;
        public string ArrivalPort { get; set; } = string.Empty;
        public double Distance { get; set; }
        public TimeSpan EstimatedDuration { get; set; }
        public string RouteType { get; set; } = string.Empty;
        public double SearchScore { get; set; }
    }

    public class PortSearchResult : BaseSearchResult
    {
        public List<PortResult> Ports { get; set; } = new();
    }

    public class PortResult
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string UNLocode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public Position Position { get; set; } = new();
        public List<string> Facilities { get; set; } = new();
        public double SearchScore { get; set; }
    }

    public class RegulationSearchResult : BaseSearchResult
    {
        public List<RegulationResult> Regulations { get; set; } = new();
    }

    public class RegulationResult
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Jurisdiction { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; }
        public string Summary { get; set; } = string.Empty;
        public double SearchScore { get; set; }
    }

    public class HistoricalDataSearchResult : BaseSearchResult
    {
        public List<HistoricalDataPoint> DataPoints { get; set; } = new();
        public Dictionary<string, object>? Aggregations { get; set; }
    }

    public class HistoricalDataPoint
    {
        public string Id { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object> Values { get; set; } = new();
        public string? EntityId { get; set; }
        public Position? Location { get; set; }
    }

    public class WeatherSearchResult : BaseSearchResult
    {
        public List<WeatherDataPoint> WeatherData { get; set; } = new();
    }

    public class WeatherDataPoint
    {
        public string Id { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public Position Location { get; set; } = new();
        public Dictionary<string, double> Parameters { get; set; } = new();
        public string Conditions { get; set; } = string.Empty;
        public double Severity { get; set; }
    }

    public class SafetyIncidentSearchResult : BaseSearchResult
    {
        public List<SafetyIncidentResult> Incidents { get; set; } = new();
    }

    public class SafetyIncidentResult
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime OccurredAt { get; set; }
        public string? VesselId { get; set; }
        public Position? Location { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class CargoSearchResult : BaseSearchResult
    {
        public List<CargoResult> CargoItems { get; set; } = new();
    }

    public class CargoResult
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Weight { get; set; }
        public string VesselId { get; set; } = string.Empty;
        public string OriginPort { get; set; } = string.Empty;
        public string DestinationPort { get; set; } = string.Empty;
        public DateTime ShipmentDate { get; set; }
    }

    public class EventSearchResult : BaseSearchResult
    {
        public List<MaritimeEventResult> Events { get; set; } = new();
    }

    public class MaritimeEventResult
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? EntityId { get; set; }
        public Position? Location { get; set; }
        public List<string> Tags { get; set; } = new();
    }

    public class FullTextSearchResult : BaseSearchResult
    {
        public List<SearchResult> Results { get; set; } = new();
        public Dictionary<string, List<FacetValue>>? Facets { get; set; }
    }

    public class SearchResult
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public double Score { get; set; }
        public List<string>? Highlights { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class SearchSuggestionsResult
    {
        public string Query { get; set; } = string.Empty;
        public List<SearchSuggestion> Suggestions { get; set; } = new();
        public int TotalSuggestions { get; set; }
    }

    public class SearchSuggestion
    {
        public string Text { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public double Score { get; set; }
        public int Frequency { get; set; }
    }

    public class AISearchResult : BaseSearchResult
    {
        public string InterpretedQuery { get; set; } = string.Empty;
        public List<AISearchMatch> Matches { get; set; } = new();
        public string Explanation { get; set; } = string.Empty;
        public double Confidence { get; set; }
    }

    public class AISearchMatch
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public double RelevanceScore { get; set; }
        public string Reasoning { get; set; } = string.Empty;
    }

    #endregion

    #region Supporting Models

    public abstract class BaseSearchResult
    {
        public int TotalResults { get; set; }
        public int ReturnedResults { get; set; }
        public TimeSpan SearchTime { get; set; }
        public DateTime SearchedAt { get; set; } = DateTime.UtcNow;
        public string? NextPageToken { get; set; }
    }

    public class DateRange
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class GeographicBounds
    {
        public double NorthLatitude { get; set; }
        public double SouthLatitude { get; set; }
        public double EastLongitude { get; set; }
        public double WestLongitude { get; set; }
    }

    public class FacetValue
    {
        public string Value { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    #endregion
}