namespace ECommerce.API.Controllers;

using ECommerce.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly UserManager<User> _userManager;

    public AdminController(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("make-admin")]
    public async Task<IActionResult> MakeAdmin(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
            return NotFound("User not found");

        await _userManager.AddToRoleAsync(user, "Admin");

        return Ok("User is now Admin");
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("test")]
    public IActionResult Test()
    {
        return Ok("Admin Works 🔥");
    }
}