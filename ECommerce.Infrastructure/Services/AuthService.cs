using ECommerce.Application.DTOs;
using ECommerce.Application.Services;
using ECommerce.Core.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                return new AuthResponse
                {
                    IsSuccess = false,
                    Message = "Email already exists"
                };
            }

            var user = new User
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email
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
            var roleResult = await _userManager.AddToRoleAsync(user, "User");

            if (!roleResult.Succeeded)
            {
                return new AuthResponse
                {
                    IsSuccess = false,
                    Message = string.Join(", ", roleResult.Errors.Select(e => e.Description))
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
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                return new AuthResponse
                {
                    IsSuccess = false,
                    Message = "Invalid email or password"
                };
            }

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
