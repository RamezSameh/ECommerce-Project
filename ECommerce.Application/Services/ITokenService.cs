using ECommerce.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Services
{
    public interface ITokenService
    {   
        string GenerateToken(User user, IList<string> roles);
    }
}
