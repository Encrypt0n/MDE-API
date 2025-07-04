
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
using MDE_API.Infrastructure;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography.X509Certificates;


var builder = WebApplication.CreateBuilder(args);

// Load the RSA private key from PEM file for signing JWTs
var privateKeyPath = builder.Configuration["Jwt:PrivateKeyPath"];
var rsa = RSA.Create();
rsa.ImportFromPem(File.ReadAllText(privateKeyPath));
var privateKey = new RsaSecurityKey(rsa);
var publicKeyPath = builder.Configuration["Jwt:PublicKeyPath"];
var rsa2 = RSA.Create();
rsa2.ImportFromPem(File.ReadAllText(publicKeyPath));
var publicKey = new RsaSecurityKey(rsa2);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = publicKey
        };
    });



builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Role1Only", policy =>
        policy.RequireClaim("userRole", "1")); // or ClaimTypes.Role, depending on your JWT
    options.AddPolicy("Role1and2Only", policy =>
        policy.RequireClaim("userRole", "1", "2")); // or ClaimTypes.Role, depending on your JWT
    options.AddPolicy("Role1and2and4Only", policy =>
        policy.RequireClaim("userRole", "1", "2", "4")); // or ClaimTypes.Role, depending on your JWT
});


builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<IMachineService, MachineService>();
builder.Services.AddSingleton<IDashboardService, DashboardService>();
builder.Services.AddSingleton<IVPNService, VPNService>(); // If you also create IVPNService
builder.Services.AddSingleton<IActivityService, ActivityService>();
builder.Services.AddSingleton<ICompanyService, CompanyService>();

builder.Services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();

builder.Services.AddSingleton<IActivityRepository, ActivityRepository>();
builder.Services.AddSingleton<ICompanyRepository, CompanyRepository>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IMachineRepository, MachineRepository>();
builder.Services.AddSingleton<IDashboardRepository, DashboardRepository>();
builder.Services.AddSingleton<IVPNRepository, VPNRepository>(); // If you also create IVPNService

builder.Services.AddSingleton<IOpenSslHelper,OpenSslHelper>();
builder.Services.AddSingleton<IFileSystem, FileSystem>();
builder.Services.AddSingleton<IVPNClientNotifier, VPNClientNotifier>();

builder.Services.AddHttpClient(); // for CloudflareDnsUpdater
builder.Services.AddSingleton<IVPNClientConnectedObserver, CloudflareDnsUpdater>();
builder.Services.AddSingleton<IVPNClientConnectedObserver, NginxConfigWriter>();
builder.Services.AddSingleton<VPNClientNotifier>();



// Create and register the JWTService instance
var jwtService = new JWTService(
    privateKey,
    publicKey,
    builder.Configuration["Jwt:Issuer"],
    builder.Configuration["Jwt:Audience"]
);

builder.Services.AddSingleton<IJWTService>(jwtService); // <-- this line is the fix


builder.Services.AddControllers();

/*builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
    });*/




var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }

