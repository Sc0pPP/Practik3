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

app.MapPost("/orders/status", async (StatusRequest r, ClothingContext db) =>
{
    var order = await db.Orders.FindAsync(r.OrderId);
    if (order is null) return Results.NotFound();
    order.Status = r.Status;
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.Run();

record RegisterRequest(string Login, string Password);
record StatusRequest(int OrderId, string Status);
