using ECommerce.Application.DTOs;
using ECommerce.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> Register(RegisterDto registerDto);
        Task<AuthResponse> Login(LoginDto loginDto);
    }
}
