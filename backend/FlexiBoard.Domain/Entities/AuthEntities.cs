namespace FlexiBoard.Domain.Entities;

using System.Text.Json.Serialization;

/// <summary>
/// User account for FlexiBoard
/// </summary>
public class User
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    public string PasswordHash { get; set; } = string.Empty;
    
    [JsonPropertyName("firstname")]
    public string FirstName { get; set; } = string.Empty;
    
    [JsonPropertyName("lastname")]
    public string LastName { get; set; } = string.Empty;
    
    public string Role { get; set; } = "Viewer"; // Admin, Editor, Viewer
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}

/// <summary>
/// Represents a refresh token for persistent authentication
/// </summary>
public class RefreshToken
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Audit log entry for tracking user actions
/// </summary>
public class AuditLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // Created, Updated, Deleted, etc.
    public string EntityType { get; set; } = string.Empty; // Dashboard, Widget, DataSource, etc.
    public string EntityId { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
