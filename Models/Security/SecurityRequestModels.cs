namespace MaritimeIQ.Platform.Models.Security
{
    /// <summary>
    /// Request model for blocking IP addresses
    /// </summary>
    public class BlockIpRequest
    {
        public string IpAddress { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
        public string BlockedBy { get; set; } = "System";
    }

    /// <summary>
    /// Request model for unblocking IP addresses
    /// </summary>
    public class UnblockIpRequest
    {
        public string IpAddress { get; set; } = string.Empty;
        public string UnblockedBy { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for threat analysis
    /// </summary>
    public class ThreatAnalysisRequest
    {
        public string IpAddress { get; set; } = string.Empty;
        public string? UserAgent { get; set; }
        public string? UserId { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new();
        public DateTime RequestTime { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Request model for password validation
    /// </summary>
    public class PasswordValidationRequest
    {
        public string Password { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public bool CheckHistory { get; set; } = false;
    }

    /// <summary>
    /// Response model for password validation
    /// </summary>
    public class PasswordValidationResponse
    {
        public bool IsValid { get; set; }
        public int Score { get; set; }
        public List<string> Issues { get; set; } = new();
        public List<string> Suggestions { get; set; } = new();
        public string Strength { get; set; } = string.Empty;
    }

    /// <summary>
    /// Password policy model
    /// </summary>
    public class PasswordPolicy
    {
        public int MinLength { get; set; } = 8;
        public int MaxLength { get; set; } = 128;
        public bool RequireUppercase { get; set; } = true;
        public bool RequireLowercase { get; set; } = true;
        public bool RequireDigits { get; set; } = true;
        public bool RequireSpecialCharacters { get; set; } = true;
        public int MaxHistoryCount { get; set; } = 12;
        public int MaxAge { get; set; } = 90;
        public List<string> ForbiddenPasswords { get; set; } = new();
    }
}