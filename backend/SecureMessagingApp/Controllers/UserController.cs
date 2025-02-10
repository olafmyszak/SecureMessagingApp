using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureMessagingApp.Dtos;
using SecureMessagingApp.Models;

namespace SecureMessagingApp.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class UserController(UserManager<User> userManager) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Register(UserRegistrationDto dto)
    {
        if (await userManager.FindByNameAsync(dto.UserName) != null)
        {
            return Conflict("UserName already exists.");
        }

        var user = new User
        {
            UserName = dto.UserName,
            PublicKey = dto.PublicKey
        };

        IdentityResult result = await userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Created();
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserIdWithUsernameDto>>> GetAllUsers()
    {
        List<UserIdWithUsernameDto> users = await userManager.Users
            .Select(u => new UserIdWithUsernameDto
            {
                Id = u.Id,
                UserName = u.UserName!
            })
            .ToListAsync();

        return Ok(users);
    }
}