using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MDE_API.Application.Services;
using System.Security.Claims;

namespace MDE_API.Application.Interfaces
{
    public interface IJWTService
    {
        
        string GenerateToken(int userid, int role, int companyId);
        ClaimsPrincipal? ValidateToken(string token);


    }
}
