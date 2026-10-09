using Microsoft.EntityFrameworkCore;
using Practik3.Context;
using Practik3.Entities;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
var app = builder.Build();

app.MapPost("/api/auth/register", async (RegisterRequest r, ClothingContext db) =>
{
    var typeId = await db.UserTypes.Where(t => t.Name == "Клиент").Select(t => t.Id).FirstAsync();
    db.Users.Add(new User { Login = r.Login, Password = r.Password, UserTypeId = typeId });
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.MapPost("/api/orders/status", async (StatusRequest r, ClothingContext db) =>
{
    var order = await db.Orders.FindAsync(r.OrderId);
    if (order is null) return Results.NotFound();
    order.Status = r.Status;
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.MapPost("/api/cloth/stock", async (StockRequest r, ClothingContext db) =>
{
    var cs = await db.ClothSizes.FirstOrDefaultAsync(x => x.ClothId == r.ClothId && x.SizeId == r.SizeId);
    if (cs is null) return Results.NotFound();
    cs.CountInStock = r.Count;
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.MapPost("/api/cloth/{id}/deactivate", async (int id, ClothingContext db) =>
{
    var cloth = await db.Cloths.FindAsync(id);
    if (cloth is null) return Results.NotFound();
    cloth.IsActive = false;
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.MapPost("/cloth/{id}/activate", async (int id, ClothingContext db) =>
{
    var cloth = await db.Cloths.FindAsync(id);
    if (cloth is null) return Results.NotFound();
    cloth.IsActive = true;
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.Run();

record RegisterRequest(string Login, string Password);
record StatusRequest(int OrderId, string Status);
record StockRequest(int ClothId, int SizeId, int Count);