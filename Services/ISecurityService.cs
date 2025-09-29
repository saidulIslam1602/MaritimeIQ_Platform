namespace MaritimeIQ.Platform.Services
{
    /// <summary>
    /// Security service interface for authentication, authorization, and security monitoring
    /// </summary>
    public interface ISecurityService
    {
        // Authentication
        Task<AuthenticationResult> AuthenticateAsync(AuthenticationRequest request);
        Task<bool> ValidateTokenAsync(string token);
        Task<TokenValidationResult> ValidateJwtTokenAsync(string jwtToken);
        Task<RefreshTokenResult> RefreshTokenAsync(string refreshToken);
        Task<bool> RevokeTokenAsync(string token);

        // Authorization
        Task<bool> AuthorizeAsync(string userId, string resource, string action);
        Task<IEnumerable<Permission>> GetUserPermissionsAsync(string userId);
        Task<bool> HasPermissionAsync(string userId, string permission);
        Task<IEnumerable<Role>> GetUserRolesAsync(string userId);
        Task<bool> AssignRoleToUserAsync(string userId, string roleId);
        Task<bool> RemoveRoleFromUserAsync(string userId, string roleId);

        // Role Management
        Task<IEnumerable<Role>> GetRolesAsync();
        Task<Role> GetRoleByIdAsync(string roleId);
        Task<Role> CreateRoleAsync(Role role);
        Task<Role> UpdateRoleAsync(string roleId, Role role);
        Task<bool> DeleteRoleAsync(string roleId);

        // Permission Management
        Task<IEnumerable<Permission>> GetPermissionsAsync();
        Task<Permission> GetPermissionByIdAsync(string permissionId);
        Task<Permission> CreatePermissionAsync(Permission permission);
        Task<bool> AssignPermissionToRoleAsync(string roleId, string permissionId);
        Task<bool> RemovePermissionFromRoleAsync(string roleId, string permissionId);

        // User Security Management
        Task<UserSecurityProfile> GetUserSecurityProfileAsync(string userId);
        Task<bool> UpdateUserSecurityProfileAsync(string userId, UserSecurityProfile profile);
        Task<bool> EnableTwoFactorAuthenticationAsync(string userId);
        Task<bool> DisableTwoFactorAuthenticationAsync(string userId);
        Task<bool> VerifyTwoFactorCodeAsync(string userId, string code);

        // API Key Management
        Task<IEnumerable<ApiKey>> GetApiKeysAsync(string userId);
        Task<ApiKey> CreateApiKeyAsync(string userId, ApiKeyRequest request);
        Task<bool> RevokeApiKeyAsync(string apiKeyId);
        Task<bool> ValidateApiKeyAsync(string apiKey);
        Task<ApiKeyValidationResult> GetApiKeyDetailsAsync(string apiKey);

        // Security Monitoring
        Task<SecurityDashboard> GetSecurityDashboardAsync();
        Task<IEnumerable<SecurityEvent>> GetSecurityEventsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<LoginAttempt>> GetLoginAttemptsAsync(string userId, DateTime? startDate = null);
        Task<IEnumerable<SuspiciousActivity>> GetSuspiciousActivitiesAsync();
        Task<SecurityMetrics> GetSecurityMetricsAsync(string timeframe);

        // Audit Logging
        Task LogSecurityEventAsync(SecurityEvent securityEvent);
        Task LogLoginAttemptAsync(LoginAttempt loginAttempt);
        Task LogPermissionChangeAsync(string userId, string action, string details);
        Task<IEnumerable<AuditLog>> GetAuditLogsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);

        // Threat Detection
        Task<ThreatAnalysis> AnalyzeThreatAsync(string ipAddress, string userAgent);
        Task<bool> IsIpAddressBlockedAsync(string ipAddress);
        Task<bool> BlockIpAddressAsync(string ipAddress, string reason);
        Task<bool> UnblockIpAddressAsync(string ipAddress);
        Task<IEnumerable<BlockedIp>> GetBlockedIpAddressesAsync();

        // Rate Limiting Security
        Task<RateLimitStatus> CheckRateLimitAsync(string identifier, string endpoint);
        Task<bool> IncrementRateLimitAsync(string identifier, string endpoint);
        Task<bool> ResetRateLimitAsync(string identifier, string endpoint);

        // Password Security
        Task<PasswordValidationResult> ValidatePasswordStrengthAsync(string password);
        Task<bool> CheckPasswordBreachAsync(string password);
        Task<PasswordPolicy> GetPasswordPolicyAsync();
        Task<bool> UpdatePasswordPolicyAsync(PasswordPolicy policy);

        // Session Management
        Task<UserSession> CreateSessionAsync(string userId, string ipAddress, string userAgent);
        Task<bool> ValidateSessionAsync(string sessionId);
        Task<bool> InvalidateSessionAsync(string sessionId);
        Task<IEnumerable<UserSession>> GetActiveSessionsAsync(string userId);
        Task<bool> InvalidateAllUserSessionsAsync(string userId);

        // Compliance and Reporting
        Task<ComplianceReport> GenerateComplianceReportAsync(DateTime startDate, DateTime endDate);
        Task<SecurityReport> GenerateSecurityReportAsync(string reportType, DateTime startDate, DateTime endDate);
        Task<bool> ExportSecurityDataAsync(string format, DateTime startDate, DateTime endDate);
    }

    // Supporting classes for Security Service
    public class AuthenticationRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? TwoFactorCode { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
    }

    public class AuthenticationResult
    {
        public bool IsSuccess { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string? UserId { get; set; }
        public string? ErrorMessage { get; set; }
        public bool RequiresTwoFactor { get; set; }
    }

    public class TokenValidationResult
    {
        public bool IsValid { get; set; }
        public string? UserId { get; set; }
        public IEnumerable<string> Roles { get; set; } = new List<string>();
        public IEnumerable<string> Permissions { get; set; } = new List<string>();
        public DateTime ExpiresAt { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class RefreshTokenResult
    {
        public bool IsSuccess { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class Role
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public IEnumerable<Permission> Permissions { get; set; } = new List<Permission>();
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class Permission
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Resource { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class UserSecurityProfile
    {
        public string UserId { get; set; } = string.Empty;
        public bool IsTwoFactorEnabled { get; set; }
        public DateTime? LastPasswordChange { get; set; }
        public DateTime? LastLogin { get; set; }
        public int FailedLoginAttempts { get; set; }
        public bool IsAccountLocked { get; set; }
        public DateTime? AccountLockedUntil { get; set; }
        public IEnumerable<string> TrustedDevices { get; set; } = new List<string>();
        public SecuritySettings Settings { get; set; } = new SecuritySettings();
    }

    public class SecuritySettings
    {
        public bool EmailNotifications { get; set; }
        public bool SmsNotifications { get; set; }
        public bool LoginAlerts { get; set; }
        public int SessionTimeoutMinutes { get; set; }
    }

    public class ApiKey
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string KeyValue { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public IEnumerable<string> Scopes { get; set; } = new List<string>();
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? LastUsed { get; set; }
        public long UsageCount { get; set; }
    }

    public class ApiKeyRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public IEnumerable<string> Scopes { get; set; } = new List<string>();
        public DateTime? ExpiresAt { get; set; }
    }

    public class ApiKeyValidationResult
    {
        public bool IsValid { get; set; }
        public string? UserId { get; set; }
        public IEnumerable<string> Scopes { get; set; } = new List<string>();
        public string? ErrorMessage { get; set; }
        public RateLimitInfo RateLimit { get; set; } = new RateLimitInfo();
    }

    public class RateLimitInfo
    {
        public int RequestsRemaining { get; set; }
        public int RequestsPerHour { get; set; }
        public DateTime ResetTime { get; set; }
    }

    public class SecurityDashboard
    {
        public SecurityMetrics Overview { get; set; } = new SecurityMetrics();
        public IEnumerable<SecurityEvent> RecentEvents { get; set; } = new List<SecurityEvent>();
        public IEnumerable<SuspiciousActivity> SuspiciousActivities { get; set; } = new List<SuspiciousActivity>();
        public ThreatSummary ThreatSummary { get; set; } = new ThreatSummary();
        public DateTime LastUpdated { get; set; }
    }

    public class SecurityMetrics
    {
        public string Timeframe { get; set; } = string.Empty;
        public int TotalLogins { get; set; }
        public int SuccessfulLogins { get; set; }
        public int FailedLogins { get; set; }
        public int BlockedAttempts { get; set; }
        public int ActiveSessions { get; set; }
        public int SuspiciousActivities { get; set; }
        public int ThreatEvents { get; set; }
        public double SuccessRate { get; set; }
    }

    public class SecurityEvent
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object> Details { get; set; } = new Dictionary<string, object>();
    }

    public class LoginAttempt
    {
        public string Id { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool IsSuccessful { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? FailureReason { get; set; }
        public string Location { get; set; } = string.Empty;
    }

    public class SuspiciousActivity
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string RiskLevel { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public DateTime DetectedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public Dictionary<string, object> Indicators { get; set; } = new Dictionary<string, object>();
    }

    public class ThreatAnalysis
    {
        public string IpAddress { get; set; } = string.Empty;
        public string RiskScore { get; set; } = string.Empty;
        public IEnumerable<string> ThreatCategories { get; set; } = new List<string>();
        public bool IsBlocked { get; set; }
        public string RecommendedAction { get; set; } = string.Empty;
        public Dictionary<string, object> Details { get; set; } = new Dictionary<string, object>();
    }

    public class ThreatSummary
    {
        public int HighRiskEvents { get; set; }
        public int MediumRiskEvents { get; set; }
        public int LowRiskEvents { get; set; }
        public int BlockedIpAddresses { get; set; }
        public IEnumerable<string> TopThreatTypes { get; set; } = new List<string>();
    }

    public class BlockedIp
    {
        public string IpAddress { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime BlockedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string BlockedBy { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class RateLimitStatus
    {
        public string Identifier { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public int RequestsRemaining { get; set; }
        public int RequestLimit { get; set; }
        public DateTime ResetTime { get; set; }
        public bool IsLimited { get; set; }
    }

    public class PasswordValidationResult
    {
        public bool IsValid { get; set; }
        public int Score { get; set; }
        public IEnumerable<string> Issues { get; set; } = new List<string>();
        public IEnumerable<string> Suggestions { get; set; } = new List<string>();
    }

    public class PasswordPolicy
    {
        public int MinimumLength { get; set; }
        public bool RequireUppercase { get; set; }
        public bool RequireLowercase { get; set; }
        public bool RequireNumbers { get; set; }
        public bool RequireSpecialCharacters { get; set; }
        public int MaxPasswordAge { get; set; }
        public int PasswordHistoryCount { get; set; }
        public bool CheckForBreaches { get; set; }
    }

    public class UserSession
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime LastActivity { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsActive { get; set; }
        public string Location { get; set; } = string.Empty;
    }

    public class AuditLog
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Resource { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    public class ComplianceReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ComplianceMetrics Metrics { get; set; } = new ComplianceMetrics();
        public IEnumerable<ComplianceViolation> Violations { get; set; } = new List<ComplianceViolation>();
        public Dictionary<string, object> Standards { get; set; } = new Dictionary<string, object>();
    }

    public class ComplianceMetrics
    {
        public double ComplianceScore { get; set; }
        public int TotalAudits { get; set; }
        public int PassedAudits { get; set; }
        public int FailedAudits { get; set; }
        public int PolicyViolations { get; set; }
    }

    public class ComplianceViolation
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public DateTime DetectedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class SecurityReport
    {
        public string ReportType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public SecurityMetrics Summary { get; set; } = new SecurityMetrics();
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
    }
}