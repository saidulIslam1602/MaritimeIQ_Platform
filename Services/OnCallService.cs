using Microsoft.ApplicationInsights;
using MaritimeIQ.Platform.Models.Incident;
using System.Collections.Concurrent;

namespace MaritimeIQ.Platform.Services
{
    /// <summary>
    /// On-call rotation and escalation management service
    /// Implements industry-standard on-call practices
    /// </summary>
    public interface IOnCallService
    {
        Task<OnCallEngineer?> GetCurrentOnCallEngineerAsync(string role = "Primary");
        Task<List<OnCallEngineer>> GetOnCallTeamAsync();
        Task<OnCallSchedule?> GetCurrentScheduleAsync();
        Task<List<OnCallSchedule>> GetScheduleAsync(DateTime from, DateTime to);
        Task<bool> CreateScheduleEntryAsync(OnCallSchedule schedule);
        Task<bool> UpdateScheduleEntryAsync(string scheduleId, OnCallSchedule updatedSchedule);
        Task<OnCallEngineer?> EscalateToNextLevelAsync(string currentEngineerId);
        Task<List<OnCallEngineer>> GetEscalationChainAsync();
        Task<bool> NotifyOnCallEngineerAsync(string engineerId, string message, string priority = "normal");
        Task<bool> RegisterEngineerAsync(OnCallEngineer engineer);
        Task<bool> UpdateEngineerAsync(string engineerId, OnCallEngineer updatedEngineer);
        Task<OnCallEngineer?> GetEngineerAsync(string engineerId);
        Task<List<OnCallEngineer>> GetAllEngineersAsync();
        Task<bool> TestEscalationAsync();
    }

    public class OnCallService : BaseMaritimeService, IOnCallService
    {
        private readonly TelemetryClient _telemetryClient;
        
        // In-memory storage for demo purposes - in production, use a database
        private readonly ConcurrentDictionary<string, OnCallEngineer> _engineers = new();
        private readonly ConcurrentDictionary<string, OnCallSchedule> _schedules = new();

        public override string ServiceName => "On-Call Management Service";

        public OnCallService(
            TelemetryClient telemetryClient,
            IConfiguration configuration,
            ILogger<OnCallService> logger) : base(logger, configuration)
        {
            _telemetryClient = telemetryClient;
            
            // Initialize with sample engineers for demo
            InitializeSampleData();
        }

        /// <summary>
        /// Get current on-call engineer by role
        /// </summary>
        public async Task<OnCallEngineer?> GetCurrentOnCallEngineerAsync(string role = "Primary")
        {
            return await ExecuteOperationAsync(async () =>
            {
                await Task.CompletedTask; // Async for consistency
                
                var now = DateTime.UtcNow;
                var currentSchedule = _schedules.Values
                    .Where(s => s.IsActive && s.Role == role && s.StartTime <= now && s.EndTime >= now)
                    .OrderBy(s => s.StartTime)
                    .FirstOrDefault();

                if (currentSchedule != null && _engineers.TryGetValue(currentSchedule.EngineerId, out var engineer))
                {
                    Logger.LogInformation("Current {Role} on-call engineer: {EngineerName} ({Email})", role, engineer.Name, engineer.Email);
                    return engineer;
                }

                Logger.LogWarning("No {Role} on-call engineer found for current time", role);
                return null;
            });
        }

        /// <summary>
        /// Get current on-call team (all roles)
        /// </summary>
        public async Task<List<OnCallEngineer>> GetOnCallTeamAsync()
        {
            return await ExecuteOperationAsync(async () =>
            {
                var team = new List<OnCallEngineer>();
                
                var primary = await GetCurrentOnCallEngineerAsync("Primary");
                if (primary != null) team.Add(primary);
                
                var secondary = await GetCurrentOnCallEngineerAsync("Secondary");
                if (secondary != null && !team.Any(e => e.Id == secondary.Id)) team.Add(secondary);
                
                var manager = await GetCurrentOnCallEngineerAsync("Manager");
                if (manager != null && !team.Any(e => e.Id == manager.Id)) team.Add(manager);

                return team;
            });
        }

        /// <summary>
        /// Get current schedule entry
        /// </summary>
        public async Task<OnCallSchedule?> GetCurrentScheduleAsync()
        {
            return await ExecuteOperationAsync(async () =>
            {
                await Task.CompletedTask; // Async for consistency
                
                var now = DateTime.UtcNow;
                return _schedules.Values
                    .Where(s => s.IsActive && s.StartTime <= now && s.EndTime >= now)
                    .OrderBy(s => s.StartTime)
                    .FirstOrDefault();
            });
        }

        /// <summary>
        /// Get schedule for date range
        /// </summary>
        public async Task<List<OnCallSchedule>> GetScheduleAsync(DateTime from, DateTime to)
        {
            return await ExecuteOperationAsync(async () =>
            {
                await Task.CompletedTask; // Async for consistency
                
                return _schedules.Values
                    .Where(s => s.IsActive && 
                               ((s.StartTime >= from && s.StartTime <= to) ||
                                (s.EndTime >= from && s.EndTime <= to) ||
                                (s.StartTime <= from && s.EndTime >= to)))
                    .OrderBy(s => s.StartTime)
                    .ThenBy(s => s.Role)
                    .ToList();
            });
        }

        /// <summary>
        /// Create new schedule entry
        /// </summary>
        public async Task<bool> CreateScheduleEntryAsync(OnCallSchedule schedule)
        {
            return await ExecuteOperationAsync(async () =>
            {
                await Task.CompletedTask; // Async for consistency
                
                // Validate engineer exists
                if (!_engineers.ContainsKey(schedule.EngineerId))
                {
                    Logger.LogError("Cannot create schedule entry: Engineer {EngineerId} not found", schedule.EngineerId);
                    return false;
                }

                // Check for overlapping schedules
                var overlapping = _schedules.Values
                    .Where(s => s.IsActive && s.Role == schedule.Role && s.Id != schedule.Id)
                    .Any(s => (schedule.StartTime < s.EndTime && schedule.EndTime > s.StartTime));

                if (overlapping)
                {
                    Logger.LogError("Cannot create schedule entry: Overlapping schedule found for role {Role}", schedule.Role);
                    return false;
                }

                _schedules[schedule.Id] = schedule;
                
                Logger.LogInformation("Schedule entry created: {ScheduleId} - {EngineerName} ({Role}) from {StartTime} to {EndTime}", 
                    schedule.Id, 
                    _engineers[schedule.EngineerId].Name, 
                    schedule.Role, 
                    schedule.StartTime, 
                    schedule.EndTime);

                _telemetryClient.TrackEvent("OnCallScheduleCreated", new Dictionary<string, string>
                {
                    ["ScheduleId"] = schedule.Id,
                    ["EngineerId"] = schedule.EngineerId,
                    ["Role"] = schedule.Role,
                    ["Duration"] = schedule.EndTime.Subtract(schedule.StartTime).ToString()
                });

                return true;
            });
        }

        /// <summary>
        /// Update existing schedule entry
        /// </summary>
        public async Task<bool> UpdateScheduleEntryAsync(string scheduleId, OnCallSchedule updatedSchedule)
        {
            return await ExecuteOperationAsync(async () =>
            {
                await Task.CompletedTask; // Async for consistency
                
                if (!_schedules.ContainsKey(scheduleId))
                {
                    Logger.LogError("Cannot update schedule entry: Schedule {ScheduleId} not found", scheduleId);
                    return false;
                }

                updatedSchedule.Id = scheduleId; // Ensure ID consistency
                _schedules[scheduleId] = updatedSchedule;
                
                Logger.LogInformation("Schedule entry updated: {ScheduleId}", scheduleId);
                return true;
            });
        }

        /// <summary>
        /// Escalate to next level in the escalation chain
        /// </summary>
        public async Task<OnCallEngineer?> EscalateToNextLevelAsync(string currentEngineerId)
        {
            return await ExecuteOperationAsync(async () =>
            {
                var escalationChain = await GetEscalationChainAsync();
                var currentIndex = escalationChain.FindIndex(e => e.Id == currentEngineerId);
                
                if (currentIndex >= 0 && currentIndex < escalationChain.Count - 1)
                {
                    var nextEngineer = escalationChain[currentIndex + 1];
                    
                    Logger.LogInformation("Escalating from {CurrentEngineer} to {NextEngineer}", 
                        escalationChain[currentIndex].Name, nextEngineer.Name);

                    _telemetryClient.TrackEvent("OnCallEscalation", new Dictionary<string, string>
                    {
                        ["FromEngineerId"] = currentEngineerId,
                        ["ToEngineerId"] = nextEngineer.Id,
                        ["EscalationLevel"] = (currentIndex + 2).ToString()
                    });

                    return nextEngineer;
                }

                Logger.LogWarning("Cannot escalate: End of escalation chain reached for engineer {EngineerId}", currentEngineerId);
                return null;
            });
        }

        /// <summary>
        /// Get escalation chain (Primary -> Secondary -> Manager -> VP)
        /// </summary>
        public async Task<List<OnCallEngineer>> GetEscalationChainAsync()
        {
            return await ExecuteOperationAsync(async () =>
            {
                var chain = new List<OnCallEngineer>();
                
                var primary = await GetCurrentOnCallEngineerAsync("Primary");
                if (primary != null) chain.Add(primary);
                
                var secondary = await GetCurrentOnCallEngineerAsync("Secondary");
                if (secondary != null && !chain.Any(e => e.Id == secondary.Id)) chain.Add(secondary);
                
                var manager = await GetCurrentOnCallEngineerAsync("Manager");
                if (manager != null && !chain.Any(e => e.Id == manager.Id)) chain.Add(manager);

                // Add VP Engineering as final escalation (if configured)
                var vpEngineering = _engineers.Values.FirstOrDefault(e => e.Skills.Contains("VP") && e.IsActive);
                if (vpEngineering != null && !chain.Any(e => e.Id == vpEngineering.Id))
                {
                    chain.Add(vpEngineering);
                }

                return chain;
            });
        }

        /// <summary>
        /// Notify on-call engineer (placeholder for real notification system)
        /// </summary>
        public async Task<bool> NotifyOnCallEngineerAsync(string engineerId, string message, string priority = "normal")
        {
            return await ExecuteOperationAsync(async () =>
            {
                if (!_engineers.TryGetValue(engineerId, out var engineer))
                {
                    Logger.LogError("Cannot notify engineer: Engineer {EngineerId} not found", engineerId);
                    return false;
                }

                // In a real implementation, this would integrate with:
                // - PagerDuty for paging
                // - Slack for messaging
                // - SMS/Phone for critical alerts
                // - Email for non-urgent notifications

                Logger.LogInformation("Notifying engineer {EngineerName} ({Email}): {Message} [Priority: {Priority}]", 
                    engineer.Name, engineer.Email, message, priority);

                _telemetryClient.TrackEvent("OnCallNotification", new Dictionary<string, string>
                {
                    ["EngineerId"] = engineerId,
                    ["EngineerName"] = engineer.Name,
                    ["Priority"] = priority,
                    ["MessageLength"] = message.Length.ToString()
                });

                await Task.Delay(100); // Simulate notification delay
                return true;
            });
        }

        /// <summary>
        /// Register new engineer
        /// </summary>
        public async Task<bool> RegisterEngineerAsync(OnCallEngineer engineer)
        {
            return await ExecuteOperationAsync(async () =>
            {
                await Task.CompletedTask; // Async for consistency
                
                _engineers[engineer.Id] = engineer;
                
                Logger.LogInformation("Engineer registered: {EngineerName} ({Email})", engineer.Name, engineer.Email);
                return true;
            });
        }

        /// <summary>
        /// Update existing engineer
        /// </summary>
        public async Task<bool> UpdateEngineerAsync(string engineerId, OnCallEngineer updatedEngineer)
        {
            return await ExecuteOperationAsync(async () =>
            {
                await Task.CompletedTask; // Async for consistency
                
                if (!_engineers.ContainsKey(engineerId))
                {
                    Logger.LogError("Cannot update engineer: Engineer {EngineerId} not found", engineerId);
                    return false;
                }

                updatedEngineer.Id = engineerId; // Ensure ID consistency
                _engineers[engineerId] = updatedEngineer;
                
                Logger.LogInformation("Engineer updated: {EngineerName}", updatedEngineer.Name);
                return true;
            });
        }

        /// <summary>
        /// Get engineer by ID
        /// </summary>
        public async Task<OnCallEngineer?> GetEngineerAsync(string engineerId)
        {
            return await ExecuteOperationAsync(async () =>
            {
                await Task.CompletedTask; // Async for consistency
                return _engineers.TryGetValue(engineerId, out var engineer) ? engineer : null;
            });
        }

        /// <summary>
        /// Get all engineers
        /// </summary>
        public async Task<List<OnCallEngineer>> GetAllEngineersAsync()
        {
            return await ExecuteOperationAsync(async () =>
            {
                await Task.CompletedTask; // Async for consistency
                return _engineers.Values.Where(e => e.IsActive).OrderBy(e => e.Name).ToList();
            });
        }

        /// <summary>
        /// Test escalation chain
        /// </summary>
        public async Task<bool> TestEscalationAsync()
        {
            return await ExecuteOperationAsync(async () =>
            {
                Logger.LogInformation("Testing escalation chain...");
                
                var chain = await GetEscalationChainAsync();
                
                if (!chain.Any())
                {
                    Logger.LogError("Escalation test failed: No engineers in escalation chain");
                    return false;
                }

                foreach (var engineer in chain)
                {
                    var notified = await NotifyOnCallEngineerAsync(engineer.Id, "Escalation chain test - please acknowledge", "test");
                    if (!notified)
                    {
                        Logger.LogError("Escalation test failed: Could not notify {EngineerName}", engineer.Name);
                        return false;
                    }
                    
                    await Task.Delay(500); // Brief delay between notifications
                }

                Logger.LogInformation("Escalation chain test completed successfully. Notified {Count} engineers", chain.Count);
                return true;
            });
        }

        /// <summary>
        /// Initialize sample data for demonstration
        /// </summary>
        private void InitializeSampleData()
        {
            // Sample engineers
            var engineers = new[]
            {
                new OnCallEngineer
                {
                    Id = "eng-001",
                    Name = "Sarah Chen",
                    Email = "sarah.chen@maritimeiq.com",
                    Phone = "+47 123 45 678",
                    PagerDutyUserId = "PUSER001",
                    SlackUserId = "U001SARAH",
                    Skills = new List<string> { "SRE", "Kubernetes", "Azure", "Maritime Systems" },
                    TimeZone = "Europe/Oslo",
                    IsActive = true
                },
                new OnCallEngineer
                {
                    Id = "eng-002",
                    Name = "Erik Nordahl",
                    Email = "erik.nordahl@maritimeiq.com",
                    Phone = "+47 987 65 432",
                    PagerDutyUserId = "PUSER002",
                    SlackUserId = "U002ERIK",
                    Skills = new List<string> { "Backend", "C#", "SQL", "Performance Tuning" },
                    TimeZone = "Europe/Oslo",
                    IsActive = true
                },
                new OnCallEngineer
                {
                    Id = "eng-003",
                    Name = "Maria Santos",
                    Email = "maria.santos@maritimeiq.com",
                    Phone = "+47 555 12 345",
                    PagerDutyUserId = "PUSER003",
                    SlackUserId = "U003MARIA",
                    Skills = new List<string> { "Manager", "Incident Response", "Team Lead" },
                    TimeZone = "Europe/Oslo",
                    IsActive = true
                },
                new OnCallEngineer
                {
                    Id = "eng-004",
                    Name = "Dr. Lars Andersen",
                    Email = "lars.andersen@maritimeiq.com",
                    Phone = "+47 777 88 999",
                    PagerDutyUserId = "PUSER004",
                    SlackUserId = "U004LARS",
                    Skills = new List<string> { "VP", "Architecture", "Strategic Planning" },
                    TimeZone = "Europe/Oslo",
                    IsActive = true
                }
            };

            foreach (var engineer in engineers)
            {
                _engineers[engineer.Id] = engineer;
            }

            // Sample schedule (current week)
            var now = DateTime.UtcNow;
            var weekStart = now.Date.AddDays(-(int)now.DayOfWeek);
            
            var schedules = new[]
            {
                new OnCallSchedule
                {
                    Id = "sched-001",
                    EngineerId = "eng-001",
                    StartTime = weekStart,
                    EndTime = weekStart.AddDays(7),
                    Role = "Primary",
                    IsActive = true,
                    EscalationContacts = new List<string> { "eng-002", "eng-003" }
                },
                new OnCallSchedule
                {
                    Id = "sched-002",
                    EngineerId = "eng-002",
                    StartTime = weekStart,
                    EndTime = weekStart.AddDays(7),
                    Role = "Secondary",
                    IsActive = true,
                    EscalationContacts = new List<string> { "eng-003" }
                },
                new OnCallSchedule
                {
                    Id = "sched-003",
                    EngineerId = "eng-003",
                    StartTime = weekStart,
                    EndTime = weekStart.AddDays(30), // Monthly rotation for manager
                    Role = "Manager",
                    IsActive = true,
                    EscalationContacts = new List<string> { "eng-004" }
                }
            };

            foreach (var schedule in schedules)
            {
                _schedules[schedule.Id] = schedule;
            }

            Logger.LogInformation("Initialized sample on-call data: {EngineerCount} engineers, {ScheduleCount} schedules", 
                engineers.Length, schedules.Length);
        }
    }
}
