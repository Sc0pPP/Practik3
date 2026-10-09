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

app.Run();

record RegisterRequest(string Login, string Password);
