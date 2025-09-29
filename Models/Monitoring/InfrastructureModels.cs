namespace MaritimeIQ.Platform.Models.Monitoring
{
    /// <summary>
    /// Infrastructure status including containers, load balancers, and CDN
    /// </summary>
    public class InfrastructureStatus
    {
        public ContainerEnvironmentStatus ContainerAppsEnvironment { get; set; } = new();
        public LoadBalancerStatus LoadBalancer { get; set; } = new();
        public CDNStatus CDN { get; set; } = new();
    }

    /// <summary>
    /// Container Apps Environment monitoring status
    /// </summary>
    public class ContainerEnvironmentStatus
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int ActiveReplicas { get; set; }
        public int DesiredReplicas { get; set; }
        public double CpuUtilization { get; set; }
        public double MemoryUtilization { get; set; }
        public string NetworkStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Load balancer health and performance status
    /// </summary>
    public class LoadBalancerStatus
    {
        public string Status { get; set; } = string.Empty;
        public int ActiveBackends { get; set; }
        public int TotalBackends { get; set; }
        public int RequestsPerMinute { get; set; }
        public int FailedHealthChecks { get; set; }
    }

    /// <summary>
    /// CDN performance and cache status
    /// </summary>
    public class CDNStatus
    {
        public string Status { get; set; } = string.Empty;
        public double CacheHitRatio { get; set; }
        public string OriginShield { get; set; } = string.Empty;
        public int EdgeLocations { get; set; }
    }
}