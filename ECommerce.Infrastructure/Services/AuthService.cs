using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommerce.Application.DTOs;
using ECommerce.Application.Services;
using ECommerce.Core.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;

        public AuthService(UserManager<User> userManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        public async Task<AuthResponse> Register(RegisterDto registerDto)
        {
            var existingUser = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Email == registerDto.Email);
            if (existingUser != null)
            {
                throw new ArgumentException("User with this email already exists.");
            }

            var user = new User
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                PasswordHash = registerDto.Password
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                return new AuthResponse
                {
                    IsSuccess = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }
            return new AuthResponse
            {
                IsSuccess = true,
                Message = "User registered successfully",
                Email = user.Email,
                Roles = new List<string>()

            };
        }

        public async Task<AuthResponse> Login(LoginDto dto)
        {
            // في الـ Constructor لازم تعمل Inject لـ UserManager<ApplicationUser>
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                throw new Exception("Invalid login");

            var result = await _userManager.CheckPasswordAsync(user, dto.Password);

            if (!result)
                throw new Exception("Invalid login");

            var roles = await _userManager.GetRolesAsync(user);

            var token = _tokenService.GenerateToken(user, roles);

            return new AuthResponse
            {
                IsSuccess = true,
                Message = "Login successful",
                Token = token,
                Email = user.Email,
                Roles = roles.ToList()
            };
        }
    }
}
