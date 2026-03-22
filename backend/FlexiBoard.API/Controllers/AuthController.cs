using FlexiBoard.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FlexiBoard.API.Controllers;

/// <summary>
/// Authentication API endpoints
/// Handles user registration, login, and token management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthenticationService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Register new user
    /// POST /api/auth/register
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResult>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { error = "Username and password required" });

            var result = await _authService.RegisterAsync(
                request.Username,
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName
            );

            if (!result.Success)
                return BadRequest(result);

            _logger.LogInformation("New user registered: {Username}", request.Username);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// User login
    /// POST /api/auth/login
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResult>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { error = "Username and password required" });

            var result = await _authService.LoginAsync(request.Username, request.Password);

            if (!result.Success)
                return Unauthorized(result);

            _logger.LogInformation("User logged in: {Username}", request.Username);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Verify JWT token
    /// POST /api/auth/verify
    /// </summary>
    [HttpPost("verify")]
    public async Task<ActionResult<UserClaims>> VerifyToken([FromBody] VerifyTokenRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var claims = await _authService.VerifyTokenAsync(request.Token);
            if (claims == null)
                return Unauthorized(new { error = "Invalid or expired token" });

            return Ok(claims);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token verification failed");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Refresh access token
    /// POST /api/auth/refresh
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResult>> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _authService.RefreshTokenAsync(request.RefreshToken);

            if (!result.Success)
                return Unauthorized(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh failed");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get current user profile
    /// GET /api/auth/profile?token={token}
    /// </summary>
    [HttpGet("profile")]
    public async Task<ActionResult<UserDto>> GetProfile([FromQuery] string token, CancellationToken cancellationToken)
    {
        try
        {
            var claims = await _authService.VerifyTokenAsync(token);
            if (claims == null)
                return Unauthorized(new { error = "Invalid token" });

            var user = await _authService.GetUserAsync(claims.UserId);
            if (user == null)
                return NotFound();

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get profile");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// List all users (admin only)
    /// GET /api/auth/users
    /// </summary>
    [HttpGet("users")]
    public async Task<ActionResult<List<UserDto>>> ListUsers([FromQuery] string token, CancellationToken cancellationToken)
    {
        try
        {
            var claims = await _authService.VerifyTokenAsync(token);
            if (claims == null || claims.Role != "Admin")
                return Unauthorized(new { error = "Admin access required" });

            var users = await _authService.ListUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list users");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update user role (admin only)
    /// PUT /api/auth/users/{userId}/role
    /// </summary>
    [HttpPut("users/{userId}/role")]
    public async Task<ActionResult> UpdateUserRole(
        string userId,
        [FromQuery] string token,
        [FromBody] UpdateRoleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var claims = await _authService.VerifyTokenAsync(token);
            if (claims == null || claims.Role != "Admin")
                return Unauthorized(new { error = "Admin access required" });

            var success = await _authService.UpdateUserRoleAsync(userId, request.Role);
            if (!success)
                return NotFound();

            return Ok(new { status = "Role updated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user role");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

// Request/Response models
public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class VerifyTokenRequest
{
    public string Token { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class UpdateRoleRequest
{
    public string Role { get; set; } = string.Empty;
}
