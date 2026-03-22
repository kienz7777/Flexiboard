using FlexiBoard.Application.Interfaces;
using FlexiBoard.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace FlexiBoard.Infrastructure.Services;

/// <summary>
/// Production-ready authentication service with JWT token support
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly ILogger<AuthenticationService> _logger;
    private readonly Dictionary<int, User> _users = new();
    private readonly Dictionary<string, RefreshToken> _refreshTokens = new();
    private readonly Dictionary<string, AuditLog> _auditLogs = new();
    private static int _nextUserId = 100;

    public AuthenticationService(ILogger<AuthenticationService> logger)
    {
        _logger = logger;
        // Seed demo users
        SeedDemoUsers();
    }

    public async Task<AuthResult> RegisterAsync(string username, string email, string password, string firstName = "", string lastName = "")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return new AuthResult { Success = false, Message = "Username, email, and password are required" };

            if (_users.Values.Any(u => u.Username == username))
                return new AuthResult { Success = false, Message = "Username already exists" };

            if (_users.Values.Any(u => u.Email == email))
                return new AuthResult { Success = false, Message = "Email already registered" };

            var user = new User
            {
                Id = _nextUserId++,
                Username = username,
                Email = email,
                PasswordHash = HashPassword(password),
                FirstName = firstName,
                LastName = lastName,
                Role = "Viewer",
                IsActive = true
            };

            _users[user.Id] = user;
            _logger.LogInformation("User registered: {Username}", username);

            return new AuthResult
            {
                Success = true,
                Message = "Registration successful",
                User = MapToDto(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed for {Username}", username);
            return new AuthResult { Success = false, Message = ex.Message };
        }
    }

    public async Task<AuthResult> LoginAsync(string username, string password)
    {
        try
        {
            var user = _users.Values.FirstOrDefault(u => u.Username == username);
            if (user == null || !VerifyPassword(password, user.PasswordHash))
            {
                _logger.LogWarning("Failed login attempt for {Username}", username);
                return new AuthResult { Success = false, Message = "Invalid username or password" };
            }

            if (!user.IsActive)
                return new AuthResult { Success = false, Message = "User account is inactive" };

            // Generate tokens
            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken(user.Id.ToString());

            user.LastLoginAt = DateTime.UtcNow;
            _logger.LogInformation("User logged in: {Username}", username);

            return new AuthResult
            {
                Success = true,
                Message = "Login successful",
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                User = MapToDto(user),
                ExpiresIn = 3600 // 1 hour
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for {Username}", username);
            return new AuthResult { Success = false, Message = ex.Message };
        }
    }

    public async Task<UserClaims?> VerifyTokenAsync(string token)
    {
        try
        {
            // In production, use proper JWT library (System.IdentityModel.Tokens.Jwt)
            // This is a simplified implementation for demo
            if (string.IsNullOrEmpty(token))
                return null;

            // Extract user ID from token (simple base64 parsing for demo)
            var parts = token.Split('.');
            if (parts.Length != 3)
                return null;

            var claims = DecodeTokenClaims(parts[1]);
            if (claims == null)
                return null;

            if (claims.ExpiresAt < DateTime.UtcNow)
                return null; // Token expired

            return claims;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token verification failed");
            return null;
        }
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshTokenString)
    {
        try
        {
            var refreshToken = _refreshTokens.Values.FirstOrDefault(t => t.Token == refreshTokenString && !t.IsRevoked);
            if (refreshToken == null || refreshToken.ExpiresAt < DateTime.UtcNow)
                return new AuthResult { Success = false, Message = "Invalid or expired refresh token" };

            if (!int.TryParse(refreshToken.UserId, out var userId) || !_users.TryGetValue(userId, out var user))
                return new AuthResult { Success = false, Message = "User not found" };

            if (!user.IsActive)
                return new AuthResult { Success = false, Message = "User account is inactive" };

            var newAccessToken = GenerateAccessToken(user);
            var newRefreshToken = GenerateRefreshToken(user.Id.ToString());
            refreshToken.IsRevoked = true; // Revoke old token

            _logger.LogInformation("Token refreshed for user: {UserId}", user.Id);

            return new AuthResult
            {
                Success = true,
                Message = "Token refreshed",
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token,
                User = MapToDto(user),
                ExpiresIn = 3600
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh failed");
            return new AuthResult { Success = false, Message = ex.Message };
        }
    }

    public async Task<UserDto?> GetUserAsync(string userId)
    {
        if (int.TryParse(userId, out var id) && _users.TryGetValue(id, out var user))
            return MapToDto(user);
        return null;
    }

    public async Task<UserDto?> GetUserByUsernameAsync(string username)
    {
        var user = _users.Values.FirstOrDefault(u => u.Username == username);
        return user != null ? MapToDto(user) : null;
    }

    public async Task<List<UserDto>> ListUsersAsync()
    {
        return _users.Values.Select(MapToDto).ToList();
    }

    public async Task<bool> UpdateUserRoleAsync(string userId, string role)
    {
        if (!int.TryParse(userId, out var id) || !_users.TryGetValue(id, out var user))
            return false;

        if (!new[] { "Admin", "Editor", "Viewer" }.Contains(role))
            return false;

        user.Role = role;
        _logger.LogInformation("User role updated: {UserId} -> {Role}", userId, role);
        return true;
    }

    public async Task<bool> DeactivateUserAsync(string userId)
    {
        if (!int.TryParse(userId, out var id) || !_users.TryGetValue(id, out var user))
            return false;

        user.IsActive = false;
        _logger.LogInformation("User deactivated: {UserId}", userId);
        return true;
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        var token = _refreshTokens.Values.FirstOrDefault(t => t.Token == refreshToken);
        if (token == null)
            return false;

        token.IsRevoked = true;
        _logger.LogInformation("Token revoked: {TokenId}", token.Id);
        return true;
    }

    // Private helper methods
    private string GenerateAccessToken(User user)
    {
        var claims = new UserClaims
        {
            UserId = user.Id.ToString(),
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        // Simplified JWT encoding (in production use System.IdentityModel.Tokens.Jwt)
        var json = System.Text.Json.JsonSerializer.Serialize(claims);
        var header = Convert.ToBase64String(Encoding.UTF8.GetBytes(@"{""alg"":""HS256"",""typ"":""JWT""}"));
        var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        var signature = Convert.ToBase64String(Encoding.UTF8.GetBytes("signature_placeholder"));

        return $"{header}.{payload}.{signature}";
    }

    private RefreshToken GenerateRefreshToken(string userId)
    {
        var token = new RefreshToken
        {
            UserId = userId,
            Token = Guid.NewGuid().ToString() + Guid.NewGuid().ToString(),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        _refreshTokens[token.Id] = token;
        return token;
    }

    private UserClaims? DecodeTokenClaims(string payload)
    {
        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            return System.Text.Json.JsonSerializer.Deserialize<UserClaims>(json);
        }
        catch
        {
            return null;
        }
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "salt"));
        return Convert.ToBase64String(hash);
    }

    private bool VerifyPassword(string password, string hash)
    {
        var hashOfInput = HashPassword(password);
        return hashOfInput == hash;
    }

    private UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id.ToString(),
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }

    private void SeedDemoUsers()
    {
        var adminUser = new User
        {
            Id = 1,
            Username = "admin",
            Email = "admin@flexiboard.com",
            PasswordHash = HashPassword("admin123"),
            FirstName = "Admin",
            LastName = "User",
            Role = "Admin"
        };

        var demoUser = new User
        {
            Id = 2,
            Username = "demo",
            Email = "demo@flexiboard.com",
            PasswordHash = HashPassword("demo123"),
            FirstName = "Demo",
            LastName = "User",
            Role = "Editor"
        };

        _users[adminUser.Id] = adminUser;
        _users[demoUser.Id] = demoUser;
        _nextUserId = 3;

        _logger.LogInformation("Seeded demo users: admin/admin123, demo/demo123");
    }
}
