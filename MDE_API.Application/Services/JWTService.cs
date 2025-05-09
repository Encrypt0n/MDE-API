// ✅ JWT Generation (Server-side) with RSA Private Key using Microsoft.IdentityModel.Tokens.JsonWebTokens

using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

public class JwtService
{
    private readonly RsaSecurityKey _privateKey;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtService(RsaSecurityKey privateKey, string issuer, string audience)
    {
        
        _privateKey = privateKey;
        _issuer = issuer;
        _audience = audience;
    }

    public string GenerateToken(string username)
    {
        var handler = new JsonWebTokenHandler();

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Sub, username),
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
