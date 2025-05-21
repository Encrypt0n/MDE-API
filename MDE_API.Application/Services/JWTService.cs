// ✅ JWT Generation (Server-side) with RSA Private Key using Microsoft.IdentityModel.Tokens.JsonWebTokens

using MDE_API.Application.Interfaces;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

public class JWTService: IJWTService
{
    private readonly RsaSecurityKey _privateKey;
    private readonly string _issuer;
    private readonly string _audience;

    public JWTService(RsaSecurityKey privateKey, string issuer, string audience)
    {
        
        _privateKey = privateKey;
        _issuer = issuer;
        _audience = audience;
    }

    public string GenerateToken(int userid)
    {
        var handler = new JsonWebTokenHandler();

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Sub, userid.ToString()),
                new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(_privateKey, SecurityAlgorithms.RsaSha256)
        };

        return handler.CreateToken(descriptor);
    }
}
