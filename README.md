# SaaS Starter Kit

A production-ready, open-source SaaS boilerplate built with **.NET 10** and **React**. Designed to give developers a solid foundation with multi-tenancy, authentication, role-based access control, audit logging, and caching — all out of the box.

---

## Tech Stack

### Backend
- .NET 10 + ASP.NET Core
- Clean Architecture + CQRS (MediatR)
- Entity Framework Core + PostgreSQL
- Redis (caching + token blacklist)
- Hangfire (background jobs)
- Docker + Docker Compose

### Frontend
- React 18 + TypeScript + Vite
- TailwindCSS + shadcn/ui
- Axios (with JWT interceptor + refresh token rotation)
- React Query (server state)
- Zustand (client state)

---

## Features

- ✅ Multi-tenant architecture (row-level isolation)
- ✅ JWT authentication + refresh token rotation
- ✅ Token blacklisting on password change
- ✅ Role-based access control (Admin / User)
- ✅ Audit logging per tenant
- ✅ Redis caching with tenant-scoped keys
- ✅ Background jobs (welcome email on registration)
- ✅ Rate limiting on auth + general endpoints
- ✅ Global exception handler (RFC 7807 ProblemDetails)
- ✅ API versioning
- ✅ Scalar API documentation
- ✅ Docker Compose for local development
- ✅ React frontend with protected routes
- ✅ User management (invite, roles, activate/deactivate, reset password)
- ✅ Profile management (update name, change password)
- ✅ Paginated audit log viewer

---

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

### 1. Clone the repo

```bash
git clone https://github.com/YOUR_USERNAME/saas-starter-kit.git
cd saas-starter-kit
```

### 2. Configure the backend

Copy and update the connection strings and JWT secret:

```bash
cd backend/SaasStarterKit
cp appsettings.json appsettings.Development.json
```

Update `appsettings.Development.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "your-strong-secret-key-minimum-32-characters"
  },
  "ConnectionStrings": {
    "NpgSqlConnection": "Host=localhost;Port=5432;Database=saas_starter;Username=postgres;Password=your-password",
    "Redis": "localhost:6379"
  }
}
```

### 3. Start infrastructure (PostgreSQL + Redis)

```bash
docker-compose up db redis
```

### 4. Run the backend

```bash
cd backend/SaasStarterKit
dotnet run
```

API will be available at `http://localhost:5000`
Scalar docs at `http://localhost:5000/scalar/v1`

### 5. Configure the frontend

```bash
cd frontend/saas-frontend
cp .env.example .env.local
```

Update `.env.local`:

VITE_API_URL=http://localhost:5000/api/v1

### 6. Run the frontend

```bash
npm install
npm run dev
```

Frontend will be available at `http://localhost:5173`

---

## Project Structure

saas-starter-kit/

backend/

SaasStarterKit.API/          # Controllers, Middleware, Program.cs

SaasStarterKit.Application/  # CQRS Commands/Queries, Interfaces, Services

SaasStarterKit.Domain/       # Entities

SaasStarterKit.Infrastructure/ # DbContext, Repositories

SaasStarterKit.Tests/        # Unit tests

frontend/

saas-frontend/               # React + Vite frontend

## Default Roles

| Role | Permissions |
|---|---|
| Admin | Full access — manage users, view audit logs |
| User | Limited access — view own profile, change password, change full name |

---

## Creating Your First Workspace

1. Go to `http://localhost:5173/signup`
2. Fill in your company name, workspace slug, and admin credentials
3. You're in — register users from the Users page

---

## Building on Top of This

This starter kit is designed to be extended. To add a new feature:

1. Add your entity to `SaasStarterKit.Domain/Entities/` — implement `ITenantEntity` for automatic tenant isolation
2. Add a migration: `dotnet ef migrations add YourMigration`
3. Create your CQRS command/query in `SaasStarterKit.Application/`
4. Add your controller in `SaasStarterKit.API/Controllers/`
5. Add your page in `frontend/saas-frontend/src/features/`

Tenant isolation, audit logging, and caching are already wired — they work automatically for any new entity.

---

## License

MIT — free to use, modify, and distribute.