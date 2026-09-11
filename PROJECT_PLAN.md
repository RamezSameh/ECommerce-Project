# ECommerce.API — Implementation Plan & TODO

.NET 8 · Clean Architecture · EF Core + SQL Server · JWT + Identity

## Phase 0 — Critical Fixes (DONE ✅)
- [x] Secure `POST /api/products` (Admin only)
- [x] Secure `DELETE /api/products/{id}` (Admin only)
- [ ] Return 404 instead of silent success in Update/Delete
- [ ] Move JWT secret out of appsettings.json → user-secrets / env vars

## Phase 1 — Foundation / Cross-Cutting
- [ ] Global exception middleware + consistent `ApiResponse<T>` envelope
- [ ] FluentValidation for all DTOs
- [ ] Serilog structured logging (console + file)
- [ ] Rate limiting (`Microsoft.AspNetCore.RateLimiting`)
- [ ] CORS policy from configuration
- [ ] Upgrade packages to latest 8.0.x patch releases

## Phase 2 — Auth & Authorization
- [ ] Refresh tokens (rotating, stored hashed, revocable)
- [ ] Email verification tokens (SMTP, configurable provider)
- [ ] Forgot / reset password flow
- [ ] Profile management: get/update profile, change password
- [ ] Add "Vendor" role; role seeding + `[Authorize(Roles=...)]` everywhere

## Phase 3 — Product Domain
- [ ] Entities: Category (nested, ParentId), ProductImage, ProductVariant,
      ProductTag, Review, Wishlist, WishlistItem
- [ ] Category CRUD + tree endpoint
- [ ] Product: full CRUD with category, tags, variants, stock
- [ ] Multiple image upload (local storage provider + IImageStorage abstraction
      for cloud swap); Cloudinary integration
- [ ] Low-stock alert flag/endpoint
- [ ] Reviews & ratings (one review per user per product, average rating)
- [ ] Wishlist add/remove/list

## Phase 4 — Cart & Coupons
- [ ] Entities: Cart, CartItem, Coupon
- [ ] Cart endpoints (guest via cart cookie/header token, merge on login)
- [ ] Coupon validation (percentage/fixed, expiry, usage limits)

## Phase 5 — Orders
- [ ] Entities: Order, OrderItem (+ OrderStatus enum), Transaction
- [ ] Checkout: create order from cart, decrement stock (transactional)
- [ ] Status tracking + history; user cancel rules (Pending only)
- [ ] Admin status transitions
- [ ] PDF invoice (QuestPDF)

## Phase 6 — Payments
- [ ] COD payment method
- [ ] Stripe integration behind `IPaymentGateway` abstraction (configurable)
- [ ] Webhook endpoint with signature verification
- [ ] Transaction history (user + admin)

## Phase 7 — Admin Dashboard API
- [ ] User management: list, edit, ban (lockout), delete
- [ ] Order management, coupon CRUD
- [ ] Sales reports: revenue by day/month, top products, top categories

## Phase 8 — Search, Filtering, Performance
- [ ] Full-text search (name/description, EF `Contains` + optional FTS)
- [ ] Filter: category, price range, brand/tag, min rating; sort: price, date, popularity
- [ ] Pagination everywhere (`PagedResult<T>`)
- [ ] Indexes on FKs, name, price, created date (via migrations)
- [ ] Redis caching (or in-memory fallback) for categories/product lists

## Phase 9 — Testing & DevOps
- [ ] xUnit unit tests (services) + integration tests (WebApplicationFactory, EF InMemory/SQLite) — target >70% on Application/Infrastructure
- [ ] `Dockerfile` (multi-stage) + `docker-compose.yml` (api + sqlserver + redis)
- [ ] Seed data (categories, demo products, users)
- [ ] README: setup, endpoints, env vars; OpenAPI spec export

## Conventions
- Secrets via env vars only; `.env.example` provided
- SOLID, interface-driven services in Infrastructure, contracts in Application
- All list endpoints paginated; all responses via `ApiResponse<T>`
