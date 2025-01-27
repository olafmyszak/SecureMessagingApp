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
    [HttpPost("register")]
    public async Task<ActionResult<User>> Register(UserRegistrationDto dto)
    {
        if (await userManager.FindByNameAsync(dto.UserName) != null)
        {
            return Conflict("Username already exists.");
        }

        var user = new User
        {
            UserName = dto.UserName, PublicKey = $"placeholder_key_{dto.UserName}"
        }; //TODO: Client side public key generation
        IdentityResult result = await userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Created();
    }

    [HttpPost("login")]
    public async Task<ActionResult<string>> Login(UserLoginDto dto)
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
        return Ok(token);
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