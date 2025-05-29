using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Xunit;
using MDE_API.Application.Interfaces; // For IJWTService
using MDE_API.Application.Services; // If JWTService is here

public class JWTServiceTests
{
    private readonly RsaSecurityKey _rsaKey;
    private readonly JWTService _jwtService;
    private readonly string _issuer = "test-issuer";
    private readonly string _audience = "test-audience";

    public JWTServiceTests()
    {
        using var rsa = RSA.Create(2048);
        _rsaKey = new RsaSecurityKey(rsa.ExportParameters(true));
        _jwtService = new JWTService(_rsaKey, _issuer, _audience);
    }

    [Fact]
    public void GenerateToken_ValidClaims_ReturnsValidJwt()
    {
        // Arrange
        int userId = 123;
        int role = 2;
        int companyId = 456;

        // Act
        var token = _jwtService.GenerateToken(userId, role, companyId);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(token));

        var handler = new JwtSecurityTokenHandler();
        var validations = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _issuer,
            ValidAudience = _audience,
            IssuerSigningKey = _rsaKey,
            ClockSkew = TimeSpan.Zero
        };

        handler.ValidateToken(token, validations, out var validatedToken);

        var jwt = (JwtSecurityToken)validatedToken;

        Assert.Equal(userId.ToString(), jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal(role.ToString(), jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Typ).Value);
        Assert.Equal(companyId.ToString(), jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Nbf).Value);
        Assert.NotNull(jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti));
    }
}
