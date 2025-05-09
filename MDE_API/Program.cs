
using Microsoft.IdentityModel.Tokens;
using MDE_API.Services;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Load the RSA private key from PEM file for signing JWTs
var privateKeyPath = builder.Configuration["Jwt:PrivateKeyPath"];
var rsa = RSA.Create();
rsa.ImportFromPem(File.ReadAllText(privateKeyPath));
var privateKey = new RsaSecurityKey(rsa);

// Register JwtService with the loaded private key
builder.Services.AddSingleton(new JwtService(privateKey, builder.Configuration["Jwt:Issuer"], builder.Configuration["Jwt:Audience"]));

builder.Services.AddControllers();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            // IMPORTANT: Set only the public key for validation on actual client
            // For the server, this is not required unless you want to validate incoming JWTs from other parties
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSingleton<DatabaseService>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
