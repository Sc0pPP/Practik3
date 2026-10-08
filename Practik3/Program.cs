using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Practik3.Context;
using Practik3.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDbContext<MyDbContext>();

var key = new SymmetricSecurityKey(
    RandomNumberGenerator.GetBytes(32));

var validation = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidIssuer = "JwtDemo",

    ValidateAudience = true,
    ValidAudience = "JwtDemoApi",

    ValidateIssuerSigningKey = true,
    IssuerSigningKey = key,

    ValidateLifetime = true,
    ClockSkew = TimeSpan.Zero,

    NameClaimType = "name",
    RoleClaimType = "role"
};

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = validation;
    });

builder.Services.AddAuthorization();

var app = builder.Build();


app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();


string CreateToken(User user)
{
    var claims = new[]
    {
        new Claim("sub",  user.Id.ToString()),
        new Claim("name", user.Login),
        new Claim("role", user.UserType.Name)
    };

    var token = new JwtSecurityToken(
        issuer: "JwtDemo",
        audience: "JwtDemoApi",
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(15),
        signingCredentials: new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256));

    return new JwtSecurityTokenHandler().WriteToken(token);
}

app.MapPost("/api/auth/login", async (LoginRequest request, MyDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Login)
        || string.IsNullOrWhiteSpace(request.Password))
        return Results.Challenge();

    var user = await db.Users
        .Include(u => u.UserType)
        .FirstOrDefaultAsync(u => u.Login == request.Login);

    if (user == null || user.Password != request.Password)
        return Results.Challenge();

    return Results.Ok(new
    {
        access_token = CreateToken(user),
        token_type = "Bearer",
    });

}).AllowAnonymous();

app.MapGet("/api/me", (ClaimsPrincipal user) => Results.Ok(new
{
    login = user.Identity?.Name,
    role = user.FindFirst("role")?.Value
})).RequireAuthorization();

app.MapGet("/api/admin", () => Results.Ok("Раздел администратора"))
    .RequireAuthorization(p => p.RequireAuthenticatedUser().RequireRole("Администратор"));




app.Run();
public record LoginRequest(string Login, string Password);