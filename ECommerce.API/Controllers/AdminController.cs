using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.Admin;
using ECommerce.Application.Services;
using ECommerce.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly UserManager<User> _userManager;

    public AdminController(IAdminService adminService, UserManager<User> userManager)
    {
        _adminService = adminService;
        _userManager = userManager;
    }

    [HttpPost("make-admin")]
    public async Task<IActionResult> MakeAdmin(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return NotFound(ApiResponse<object>.Fail("User not found"));
        await _userManager.AddToRoleAsync(user, "Admin");
        return Ok(ApiResponse<object>.Success(null!, $"User is now Admin"));
    }

    [HttpGet("users")]
    public async Task<IActionResult> Users([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
        => Ok(ApiResponse<object>.Success(await _adminService.GetUsersAsync(page, pageSize, search)));

    [HttpPost("users/{id}/ban")]
    public async Task<IActionResult> Ban(string id)
    {
        await _adminService.BanUserAsync(id, true);
        return Ok(ApiResponse<object>.Success(null!, "User banned"));
    }

    [HttpPost("users/{id}/unban")]
    public async Task<IActionResult> Unban(string id)
    {
        await _adminService.BanUserAsync(id, false);
        return Ok(ApiResponse<object>.Success(null!, "User unbanned"));
    }

    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        await _adminService.DeleteUserAsync(id);
        return Ok(ApiResponse<object>.Success(null!, "User deleted"));
    }

    [HttpPost("users/{id}/promote-vendor")]
    public async Task<IActionResult> PromoteVendor(string id)
    {
        await _adminService.PromoteToVendor(id);
        return Ok(ApiResponse<object>.Success(null!, "User promoted to Vendor"));
    }

    [HttpGet("reports/sales")]
    public async Task<IActionResult> SalesReport()
        => Ok(ApiResponse<SalesReportDto>.Success(await _adminService.GetSalesReportAsync()));

    [HttpGet("test")]
    public IActionResult Test() => Ok(ApiResponse<object>.Success(null!, "Admin Works"));
}