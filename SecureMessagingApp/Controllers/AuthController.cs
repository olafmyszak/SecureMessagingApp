using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SecureMessagingApp.Dtos;
using SecureMessagingApp.Models;

namespace SecureMessagingApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(UserManager<User> userManager, IConfiguration configuration)
    : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<User>> Register(UserRegistrationDto dto)
    {
        if (await userManager.FindByNameAsync(dto.UserName) != null)
        {
            return Conflict("Username already exists.");
        }

        var user = new User { UserName = dto.UserName, PublicKey = $"placeholder_key_{dto.UserName}" };
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

        string token = CreateJwt(user);
        return Ok(token);
    }

    private string CreateJwt(User user)
    {
        string secretKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException();
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.UserName ?? throw new InvalidOperationException())
            ]),
            Expires = DateTime.UtcNow.AddMinutes(configuration.GetValue<int>("Jwt:ExpirationInMinutes")),
            SigningCredentials = credentials,
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"]
        };

        var handler = new JsonWebTokenHandler();

        string token = handler.CreateToken(tokenDescriptor);

        return token;
    }
}