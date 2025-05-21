
using Microsoft.IdentityModel.Tokens;
using MDE_API.Domain;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MDE_API.Application.Services;
using MDE_API.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using MDE_API.Infrastructure.Repositories;
using Microsoft.AspNetCore.Connections;
using MDE_API.Infrastructure.Factories;
using MDE_API.Application.Interfaces.MDE_API.Services;

var builder = WebApplication.CreateBuilder(args);

// Load the RSA private key from PEM file for signing JWTs
var privateKeyPath = builder.Configuration["Jwt:PrivateKeyPath"];
var rsa = RSA.Create();
rsa.ImportFromPem(File.ReadAllText(privateKeyPath));
var privateKey = new RsaSecurityKey(rsa);


builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<IMachineService, MachineService>();
builder.Services.AddSingleton<IDashboardService, DashboardService>();
builder.Services.AddSingleton<IVPNService, VPNService>(); // If you also create IVPNService
builder.Services.AddSingleton<IActivityService, ActivityService>();

builder.Services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();

builder.Services.AddSingleton<IActivityRepository, ActivityRepository>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IMachineRepository, MachineRepository>();
builder.Services.AddSingleton<IDashboardRepository, DashboardRepository>();
builder.Services.AddSingleton<IVPNRepository, VPNRepository>(); // If you also create IVPNService


// Create and register the JWTService instance
var jwtService = new JWTService(
    privateKey,
    builder.Configuration["Jwt:Issuer"],
    builder.Configuration["Jwt:Audience"]
);

builder.Services.AddSingleton<IJWTService>(jwtService); // <-- this line is the fix


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




var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
