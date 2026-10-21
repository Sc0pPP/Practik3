# Practik3 — API магазина одежды

Минимальный Web API на ASP.NET Core для магазина одежды (практическая работа №3).

## Модель данных

`Cloth` (товар), `ClothType` (тип), `Size` (размер), `ClothSize` (остаток по размерам), `Order` (заказ), `Clothorder` (позиции заказа), `User` и `UserType` (пользователи и роли).

## Эндпоинты

| Метод | Путь | Описание |
|-------|------|----------|
| POST | `/api/auth/register` | регистрация клиента `{ login, password }` |
| POST | `/api/orders/status` | смена статуса заказа `{ orderId, status }` |
| POST | `/api/cloth/stock` | остаток товара по размеру `{ clothId, sizeId, count }` |
| POST | `/api/cloth/{id}/deactivate` | снять товар с продажи |
| POST | `/cloth/{id}/activate` | вернуть товар в продажу |

## Технологии

- C#, .NET 10, ASP.NET Core Minimal API
- Entity Framework Core + PostgreSQL (Npgsql)

## Запуск

1. Поднимите PostgreSQL с базой по схеме из `Entities/`.
2. Зарегистрируйте `ClothingContext` в `Program.cs` со своей строкой подключения:
   ```csharp
   builder.Services.AddDbContext<ClothingContext>(o =>
       o.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
   ```
3. Запустите:

```bash
dotnet run --project Practik3
```
