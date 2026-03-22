namespace FlexiBoard.Application.Interfaces;

/// <summary>
/// Authentication and user management service
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Register a new user
    /// </summary>
    Task<AuthResult> RegisterAsync(string username, string email, string password, string firstName = "", string lastName = "");

    /// <summary>
    /// Authenticate user and return JWT token
    /// </summary>
    Task<AuthResult> LoginAsync(string username, string password);

    /// <summary>
    /// Verify JWT token and return claims
    /// </summary>
    Task<UserClaims?> VerifyTokenAsync(string token);

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    Task<AuthResult> RefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Get user by ID
    /// </summary>
    Task<UserDto?> GetUserAsync(string userId);

    /// <summary>
    /// Get user by username
    /// </summary>
    Task<UserDto?> GetUserByUsernameAsync(string username);

    /// <summary>
    /// List all users (admin only)
    /// </summary>
    Task<List<UserDto>> ListUsersAsync();

    /// <summary>
    /// Update user role (admin only)
    /// </summary>
    Task<bool> UpdateUserRoleAsync(string userId, string role);

    /// <summary>
    /// Deactivate user
    /// </summary>
    Task<bool> DeactivateUserAsync(string userId);

    /// <summary>
    /// Revoke refresh token
    /// </summary>
    Task<bool> RevokeTokenAsync(string refreshToken);
}

/// <summary>
/// Result of authentication operation
/// </summary>
public class AuthResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public UserDto? User { get; set; }
    public int ExpiresIn { get; set; } // seconds
}

/// <summary>
/// Claims extracted from JWT token
/// </summary>
public class UserClaims
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// User data transfer object (no sensitive data)
/// </summary>
public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

/// <summary>
/// Audit logging service
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Log user action
    /// </summary>
    Task LogActionAsync(string userId, string action, string entityType, string entityId, string details = "");

    /// <summary>
    /// Get audit log for user (admin only)
    /// </summary>
    Task<List<AuditLogDto>> GetUserAuditLogAsync(string userId, int limit = 100);

    /// <summary>
    /// Get all audit logs (admin only)
    /// </summary>
    Task<List<AuditLogDto>> GetAuditLogAsync(int limit = 1000);
}

public class AuditLogDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
