using System.Diagnostics;
using MaritimeIQ.Platform.Models.Monitoring;

namespace MaritimeIQ.Platform.Services
{
    /// <summary>
    /// Collects real system resource metrics from the current process
    /// </summary>
    public class SystemMetricsCollector
    {
        private readonly ILogger<SystemMetricsCollector> _logger;
        private readonly Process _currentProcess;
        private DateTime _lastCpuCheck = DateTime.MinValue;
        private TimeSpan _lastCpuTime = TimeSpan.Zero;

        public SystemMetricsCollector(ILogger<SystemMetricsCollector> logger)
        {
            _logger = logger;
            _currentProcess = Process.GetCurrentProcess();
        }

        /// <summary>
        /// Get current system resource usage metrics
        /// </summary>
        public SystemResourceMetrics GetCurrentMetrics()
        {
            try
            {
                _currentProcess.Refresh();

                return new SystemResourceMetrics
                {
                    CpuUsagePercent = GetCpuUsage(),
                    MemoryUsageBytes = _currentProcess.WorkingSet64,
                    MemoryUsagePercent = GetMemoryUsagePercent(),
                    ThreadCount = _currentProcess.Threads.Count,
                    WorkingSetBytes = _currentProcess.WorkingSet64,
                    Collected = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error collecting system metrics");
                return new SystemResourceMetrics
                {
                    CpuUsagePercent = 0,
                    MemoryUsageBytes = 0,
                    MemoryUsagePercent = 0,
                    ThreadCount = 0,
                    WorkingSetBytes = 0,
                    Collected = DateTime.UtcNow
                };
            }
        }

        /// <summary>
        /// Calculate CPU usage percentage
        /// </summary>
        private double GetCpuUsage()
        {
            try
            {
                var currentTime = DateTime.UtcNow;
                var currentCpuTime = _currentProcess.TotalProcessorTime;

                if (_lastCpuCheck == DateTime.MinValue)
                {
                    _lastCpuCheck = currentTime;
                    _lastCpuTime = currentCpuTime;
                    return 0;
                }

                var timeDiff = (currentTime - _lastCpuCheck).TotalMilliseconds;
                var cpuDiff = (currentCpuTime - _lastCpuTime).TotalMilliseconds;

                if (timeDiff > 0)
                {
                    var cpuUsage = (cpuDiff / (Environment.ProcessorCount * timeDiff)) * 100;
                    
                    _lastCpuCheck = currentTime;
                    _lastCpuTime = currentCpuTime;

                    return Math.Min(Math.Max(cpuUsage, 0), 100);
                }

                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating CPU usage");
                return 0;
            }
        }

        /// <summary>
        /// Calculate memory usage as percentage of total system memory
        /// </summary>
        private double GetMemoryUsagePercent()
        {
            try
            {
                var memoryInfo = GC.GetGCMemoryInfo();
                var installedMemory = memoryInfo.TotalAvailableMemoryBytes;
                
                if (installedMemory > 0)
                {
                    return (_currentProcess.WorkingSet64 / (double)installedMemory) * 100;
                }

                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating memory usage percentage");
                return 0;
            }
        }
    }
}

