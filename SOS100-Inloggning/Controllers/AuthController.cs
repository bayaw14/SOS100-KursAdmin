using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SOS100_Inloggning.DTOs;
using SOS100_Inloggning.Services;

namespace SOS100_Inloggning.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly JwtService _jwtService;

    public AuthController(AuthService authService, JwtService jwtService)
    {
        _authService = authService;
        _jwtService = jwtService;
    }

    // ONLY ADMIN CAN CREATE USERS
    [Authorize(Roles = "Admin")]
    [HttpPost("create-user")]
    public IActionResult CreateUser(CreateUserDTO dto)
    {
        var result = _authService.CreateUser(dto);
        return Ok(result);
    }

    // LOGIN
    [HttpPost("login")]
    public IActionResult Login(LoginDTO dto)
    {
        var user = _authService.Login(dto);

        if (user == null)
            return Unauthorized("Invalid login");

        var token = _jwtService.GenerateToken(user);

        return Ok(new
        {
            token,
            role = user.Role,
            name = user.Email
        });
    }
}