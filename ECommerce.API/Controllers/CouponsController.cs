using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.Coupon;
using ECommerce.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class CouponsController : ControllerBase
{
    private readonly ICouponService _couponService;

    public CouponsController(ICouponService couponService) => _couponService = couponService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(ApiResponse<object>.Success(await _couponService.GetAllAsync(page, pageSize)));

    [HttpPost]
    public async Task<IActionResult> Create(CreateCouponDto dto)
        => Ok(ApiResponse<CouponDto>.Success(await _couponService.CreateAsync(dto), "Coupon created"));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateCouponDto dto)
        => Ok(ApiResponse<CouponDto>.Success(await _couponService.UpdateAsync(id, dto), "Coupon updated"));

    [HttpPost("{id:int}/toggle")]
    public async Task<IActionResult> Toggle(int id)
    {
        await _couponService.ToggleActiveAsync(id);
        return Ok(ApiResponse<object>.Success(null!, "Coupon toggled"));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _couponService.DeleteAsync(id);
        return Ok(ApiResponse<object>.Success(null!, "Coupon deleted"));
    }
}