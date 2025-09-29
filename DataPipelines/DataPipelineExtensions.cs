using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MaritimeIQ.Platform.DataPipelines.ETL;
using MaritimeIQ.Platform.DataPipelines.Streaming;
using MaritimeIQ.Platform.DataPipelines.Quality;

namespace MaritimeIQ.Platform.DataPipelines
{
    /// <summary>
    /// Enterprise Data Pipeline Service Registration
    /// Professional dependency injection for data engineering services
    /// </summary>
    public static class DataPipelineExtensions
    {
        public static IServiceCollection AddMaritimeDataPipelines(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            // Register core data pipeline services as hosted services
            services.AddHostedService<MaritimeDataETLService>();
            services.AddHostedService<MaritimeStreamingProcessor>();
            services.AddHostedService<DataQualityService>();
            
            return services;
        }

        public static IServiceCollection ConfigureDataPipelineOptions(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configure pipeline options here
            return services;
        }
    }
}