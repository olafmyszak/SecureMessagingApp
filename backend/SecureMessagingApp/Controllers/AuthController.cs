using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SecureMessagingApp.Dtos;
using SecureMessagingApp.Models;
using SecureMessagingApp.Services;

namespace SecureMessagingApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(UserManager<User> userManager, ITokenService tokenService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<JwtResponse>> Login(UserLoginDto dto)
    {
        User? user = await userManager.FindByNameAsync(dto.UserName);

        if (user == null)
        {
            return Unauthorized("Incorrect username or password");
        }

        bool passwordValid = await userManager.CheckPasswordAsync(user, dto.Password);

        if (!passwordValid)
        {
            return Unauthorized("Incorrect username or password");
        }

        string token = tokenService.GenerateJwtToken(user);
        var response = new JwtResponse { AccessToken = token };

        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<string>> GetPublicKey(int id)
    {
        User? user = await userManager.FindByIdAsync(id.ToString());

        if (user == null)
        {
            return NotFound($"User id: {id} not found.");
        }

        return Ok(user.PublicKey);
    }
}