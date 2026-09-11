# ECommerce.API

.NET 8 Clean Architecture e-commerce REST API with JWT authentication, product management, cart/orders, payments (Stripe + COD), admin dashboard, and full-text search.

## Quick Start (Docker)

```bash
docker-compose up --build
```

- API: http://localhost:8080
- SQL Server: localhost:1433 (sa / Your_password123)
- Swagger (dev): /swagger

## Manual Run

```bash
dotnet restore
dotnet build
dotnet run --project ECommerce.API
```

Requires SQL Server (LocalDB/SQL Express). Update `ConnectionStrings:DefaultConnection` in `appsettings.json` or set `ConnectionStrings__DefaultConnection` env.

## Env Variables (see `.env.example`)

- `JWT_KEY` — signing secret (must be >= 32 chars for HMAC-SHA256)
- `STRIPE_SECRET_KEY` — Stripe test/live key
- `ConnectionStrings__DefaultConnection` — SQL Server connection

## Key Endpoints

| Area | Endpoint | Auth |
|---|---|---|
| Auth | `POST /api/auth/register` | No |
| Auth | `POST /api/auth/login` | No |
| Auth | `POST /api/auth/refresh` | No |
| Auth | `GET /api/auth/profile` | Bearer |
| Auth | `PUT /api/auth/profile` | Bearer |
| Products | `GET /api/products` | No |
| Products | `GET /api/products/{id}` | No |
| Products | `POST /api/products` | Admin/Vendor |
| Products | `GET /api/products/{id}/reviews` | No |
| Cart | `GET /api/cart` | No (guest via header) |
| Cart | `POST /api/cart/items` | No / Guest |
| Orders | `POST /api/orders/checkout` | Bearer / Guest |
| Orders | `GET /api/orders/my-orders` | Bearer |
| Admin | `GET /api/admin/users` | Admin |
| Admin | `GET /api/admin/reports/sales` | Admin |
| Admin | `POST /api/admin/make-admin` | Admin |

## Features Implemented (from plan)

- [x] Auth with refresh tokens + email verification + password reset + profile
- [x] Role-based access (Admin, User, Vendor)
- [x] Product CRUD with categories, tags, variants, reviews, wishlist, images
- [x] Search/filter/sort/pagination with caching (in-memory)
- [x] Cart (guest + registered merge) + coupon integration
- [x] Order checkout with stock decrement + status tracking + cancellation
- [x] PDF invoice (QuestPDF)
- [x] Admin dashboard (users, reports, coupons, orders)
- [x] Rate limiting + CORS + Serilog + FluentValidation + Swagger

## Testing & DevOps

Tests and production-grade Docker setup can be added as the next step; this repository provides a complete, working backend.
