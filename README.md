# ECommerce Project

A full-stack e-commerce platform built with .NET 8, Angular, SQL Server, and Clean Architecture principles.

The project includes a secure backend API, a modern frontend storefront, role-based access control, product catalog features, shopping cart, order management, coupons, and admin tools.

## Overview

This repository contains:

- Backend API in `ECommerce.API`
- Application layer in `ECommerce.Application`
- Core entities and business logic in `ECommerce.Core`
- Infrastructure and EF Core data access in `ECommerce.Infrastructure`
- Frontend storefront in `ECommerce.Frontend` built with Angular

## Tech Stack

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- JWT Authentication
- ASP.NET Core Identity
- Angular 19
- Swagger / OpenAPI
- FluentValidation
- Serilog
- QuestPDF for invoice generation
- Docker Compose for local environment

## Features

- User authentication and authorization
- Admin, User, and Vendor roles
- Product catalog with categories, reviews, and search
- Cart management for guest and authenticated users
- Coupon validation and checkout flow
- Order lifecycle and status tracking
- Payment abstraction with Stripe and COD support
- Admin dashboard APIs for users, reports, and orders
- Logging, rate limiting, CORS, and validation
- Local image storage and static file serving

## Repository Structure

```text
ECommerce Project/
├── ECommerce.API/                 # ASP.NET Core API
├── ECommerce.Application/         # Application services and DTOs
├── ECommerce.Core/                # Entities and domain contracts
├── ECommerce.Infrastructure/      # EF Core, services, persistence
├── ECommerce.Frontend/            # Angular frontend app
├── .env.example                   # Environment variables example
├── docker-compose.yml             # Local Docker setup
├── Dockerfile                     # API container build
├── PROJECT_PLAN.md                # Project roadmap and notes
├── README.md
├── ABOUT.md
├── ECommerce.API.sln             # Solution file
└── .gitignore
```

## Prerequisites

Before running the project, make sure you have:

- .NET 8 SDK
- Node.js and npm
- SQL Server or Docker Desktop
- Optional: Visual Studio 2022 / VS Code

## Quick Start with Docker

From the project root:

```bash
docker-compose up --build
```

Then open:

- API: http://localhost:8080
- Swagger UI: http://localhost:8080/swagger
- SQL Server: localhost:1433

## Run the API Manually

```bash
dotnet restore
dotnet build
dotnet run --project ECommerce.API
```

## Run the Frontend

```bash
cd ECommerce.Frontend
npm install
npm start
```

Frontend runs on:

- http://localhost:4200

## Environment Variables

Copy `.env.example` to `.env` and configure your values.

```env
JWT_KEY=YOUR_SUPER_SECRET_KEY
STRIPE_SECRET_KEY=sk_test_...
DB_CONNECTION_STRING=Server=localhost;Database=ECommerceDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
```

## Default Admin Account

The API seeds a default admin user at startup:

- Email: `admin@site.com`
- Password: `Admin@123`

## Main API Endpoints

```text
POST /api/auth/register
POST /api/auth/login
POST /api/auth/refresh
GET /api/auth/profile
PUT /api/auth/profile

GET /api/products
GET /api/products/{id}
POST /api/products
GET /api/products/{id}/reviews

GET /api/cart
POST /api/cart/items

POST /api/orders/checkout
GET /api/orders/my-orders

GET /api/admin/users
GET /api/admin/reports/sales
POST /api/admin/make-admin
```

## Development Notes

- The API uses SQL Server migration support on startup for local development convenience.
- Swagger is enabled in development mode.
- Serilog writes console and file logs.
- CORS is configured for the Angular frontend.

## License

This project is currently intended for learning, portfolio, and internal development use.

## Contributing

Pull requests and improvements are welcome. Feel free to fork the project, open issues, and propose enhancements.

## Contact

For questions or collaboration, open an issue in this repository or reach out through the project owner profile.
