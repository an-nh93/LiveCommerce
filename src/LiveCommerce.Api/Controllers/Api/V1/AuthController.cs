using LiveCommerce.Application.Common;
using LiveCommerce.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveCommerce.Api.Controllers.Api.V1;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResult>>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ShopCode) || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(ApiResponse<LoginResult>.Fail("ShopCode, Username and Password are required."));

        var result = await _authService.LoginAsync(request.ShopCode, request.Username, request.Password, ct);
        if (result == null)
            return Unauthorized(ApiResponse<LoginResult>.Fail("Invalid shop code, username or password."));

        return Ok(ApiResponse<LoginResult>.Ok(result));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResult>>> Refresh([FromBody] RefreshRequest request, CancellationToken ct)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken ?? "", ct);
        if (result == null)
            return Unauthorized(ApiResponse<LoginResult>.Fail("Invalid or expired refresh token."));
        return Ok(ApiResponse<LoginResult>.Ok(result));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<CurrentUserDto>>> Me(CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var shopIdClaim = User.FindFirst("shop_id")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(shopIdClaim) ||
            !long.TryParse(userIdClaim, out var userId) || !long.TryParse(shopIdClaim, out var shopId))
            return Unauthorized();

        var user = await _authService.GetCurrentUserAsync(userId, shopId, ct);
        if (user == null)
            return Unauthorized(ApiResponse<CurrentUserDto>.Fail("User not found or inactive."));
        return Ok(ApiResponse<CurrentUserDto>.Ok(user));
    }
}

public class LoginRequest
{
    public string ShopCode { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class RefreshRequest
{
    public string? RefreshToken { get; set; }
}
