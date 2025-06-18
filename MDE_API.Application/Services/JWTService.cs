// ✅ JWT Generation (Server-side) with RSA Private Key using Microsoft.IdentityModel.Tokens.JsonWebTokens

using MDE_API.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

public class JWTService: IJWTService
{
    private readonly RsaSecurityKey _privateKey;
    private readonly RsaSecurityKey _publicKey;
    private readonly string _issuer;
    private readonly string _audience;
    //private readonly IConfiguration _config;

    public JWTService(RsaSecurityKey privateKey, RsaSecurityKey publicKey, string issuer, string audience /*IConfiguration config*/)
    {
        _privateKey = privateKey;
        _publicKey = publicKey;
        _issuer = issuer;
        _audience = audience;
        //_config = config;
    }

    public string GenerateToken(int userid, int role, int companyId)
    {
        var handler = new JsonWebTokenHandler();
        

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                
                new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Sub, userid.ToString()),
                new Claim("role", role.ToString()),
                new Claim("companyId", companyId.ToString()),
                new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            }),
            IssuedAt = DateTime.Now,
            Expires = DateTime.Now.AddHours(1),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(_privateKey, SecurityAlgorithms.RsaSha256)
        };

        return handler.CreateToken(descriptor);
    }

    /*public bool IsTokenValid(string token)
    {
        try
        {
            // Debugging if the token is being checked
            //Debug.WriteLine("Starting token validation...");

            var publicKeyPath = "Keys/public.key";

            // Check if the public key file exists and log it
            if (!File.Exists(publicKeyPath))
            {
                //Debug.WriteLine("Public key file not found.");
                throw new FileNotFoundException("Public key not found.");
            }

            // Read the public key content
            var publicKeyPem = File.ReadAllText(publicKeyPath);
            //Debug.WriteLine("Public key loaded from: " + publicKeyPath);

            // Create RSA instance and load the public key
            using var rsa = RSA.Create();
            rsa.ImportFromPem(publicKeyPem);
            //Debug.WriteLine("Public key successfully imported.");

            // Initialize the JWT handler
            var handler = new JsonWebTokenHandler();

            // Log the token being validated (this can be sensitive, be careful in production environments)
            //Debug.WriteLine("Validating token: " + token.Substring(0, 50) + "...");  // Show the first 50 chars for visibility

            // Validate the token with the defined parameters
            var result = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Audience"],
                IssuerSigningKey = new RsaSecurityKey(rsa),
                ValidateLifetime = true
            });

            // Log result of validation
            //Debug.WriteLine("Token validation completed. Valid: " + result.IsValid);

            // Return the validity of the token
            return result.IsValid;
        }
        catch (Exception ex)
        {
            // Log the exception for debugging purposes
           // Debug.WriteLine("Error during token validation: " + ex.Message);
           // Debug.WriteLine("Stack Trace: " + ex.StackTrace);
            return false;
        }
    }*/

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        try
        {
            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _publicKey,
                ValidateLifetime = true,
               // ClockSkew = TimeSpan.FromMinutes(2)
            };

            var principal = handler.ValidateToken(token, parameters, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }
}
