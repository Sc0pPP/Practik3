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

app.MapGet("/api/cloth", async (MyDbContext db) =>
        Results.Ok(await db.Cloths
            .OrderBy(c => c.Id)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Price,
                c.Description,
                c.CountBuys,
                c.IsActive,
                type = c.Type != null ? c.Type.Name : null

            })
            .ToListAsync()))
    .AllowAnonymous();

app.MapGet("/api/cloth/search", async (string name, MyDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(name))
        return Results.BadRequest(new { message = "не указали часть названия йоу" });

    var result = await db.Cloths
        .Where(c => EF.Functions.ILike(c.Name!, $"%{name}%"))
        .OrderBy(c => c.Id)
        .Select(c => new
        {

            c.Id,
            c.Name,
            c.Price,
            c.Description,
            c.CountBuys,
            c.IsActive,
            Type = c.Type != null ? c.Type.Name : null
        }).ToListAsync();
    return Results.Ok(result);
}).AllowAnonymous();


app.MapPost("/api/orders", async (CreateOrderRequest request, ClaimsPrincipal user, MyDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Addres) || request.ClothIds == null || request.ClothIds.Count == 0)
        return Results.BadRequest(new { Message = "укажи адрес или одну вещь " });

    if (!int.TryParse(user.FindFirst("sub")?.Value, out var userId))
        return Results.Unauthorized();

    var existingIds = await db.Cloths
        .Where(c => request.ClothIds.Contains(c.Id))
        .Select(c => c.Id)
        .ToListAsync();

    var missing = request.ClothIds.Except(existingIds).ToList();
    if (missing.Count > 0)
        return Results.BadRequest(new { message = "одежда не нашлась", ids = missing });

    var order = new Order()
    {
        DateTime = DateOnly.FromDateTime(System.DateTime.Now),
        Addres = request.Addres,
        UserId = userId,
        Status = "Собирается",
        Clothorders = request.ClothIds
            .Select(id => new Clothorder { ClothId = id })
            .ToList()
    };
    db.Orders.Add(order);
    await db.SaveChangesAsync();
    return Results.Created($"/api/orders/{order.Id}", new { order.Id, order.Status });
}).RequireAuthorization();

app.MapGet("/api/order/status/{status}", async (MyDbContext db, string status) =>
{
    string[] allowed = { "Собирается", "Едет", "Приехал" };
    if (!allowed.Contains(status))
        return Results.BadRequest(new { message = "допустимые статусы : Собирается, Едет, Приехало" });

    var orders = await db.Orders
        .Where(o => o.Status == status)
        .OrderBy(o => o.Id)
        .Select(o => new
        {
            o.Id,
            Date = o.DateTime,
            o.Addres,
            o.Status,
            o.UserId,
            UserLogin = o.User.Login,
            Items = o.Clothorders.Select(co => new
            {
                co.ClothId,
                co.Cloth!.Name,
                co.Cloth.Price
            }).ToList(),
            Total = o.Clothorders.Sum(co => co.Cloth!.Price)
        })
        .ToListAsync();

    return Results.Ok(orders);

}).RequireAuthorization();

app.MapGet("/api/orders/user/{userId}", async (int userId, MyDbContext db) =>
{
    var orders = await db.Orders
        .Where(o => o.UserId == userId)
        .OrderBy(o => o.Id)
        .Select(o => new
        {
            o.Id,
            Date = o.DateTime,
            o.Addres,
            o.Status,
            o.UserId,
            UserLogin = o.User.Login,
            Items = o.Clothorders.Select(co => new
            {
                co.ClothId,
                co.Cloth!.Name,
                co.Cloth.Price
            }).ToList(),
            Total = o.Clothorders.Sum(co => co.Cloth!.Price)
        })
        .ToListAsync();

    return Results.Ok(orders);
}).RequireAuthorization();

app.Run();

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

public record LoginRequest(string Login, string Password);
public record CreateOrderRequest(string Addres, List<int> ClothIds);