# HotCoffee PostgreSQL

HotCoffee is an ASP.NET Core MVC restaurant management application backed by PostgreSQL. It provides a public customer experience (menu, reservation requests, published reviews) and an authenticated admin workspace for categories, products, reservations, reviews, dashboard analytics, and statistics.

## Features

- **Public menu** — database-driven categories and products
- **Reservation requests** — customer-safe public flow with server-owned status
- **Published reviews** — moderated public visibility
- **Admin dashboard** — DB-derived operational snapshot
- **Admin CRUD** — categories, products, reservations, reviews
- **Statistics** — truthful analytics and charts from database data

## Technology

- .NET 10 / ASP.NET Core MVC
- Entity Framework Core 10
- PostgreSQL (Npgsql EF provider)
- xUnit integration and service tests

## Prerequisites

- [.NET SDK 10.0.302](global.json) (or compatible 10.0.x SDK)
- PostgreSQL for local development database access
- `dotnet tool restore` for local tooling defined in `.config/dotnet-tools.json`

## Local setup

1. Clone the repository and restore packages:

   ```bash
   dotnet tool restore
   dotnet restore DatabaseMastery.HotCoffeePostgreSQL.sln
   ```

2. Configure **User Secrets** for the web project (keys only — never commit real values):

   ```bash
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=RestaurantMenuDb;Username=postgres;Password=<YOUR_POSTGRES_PASSWORD>" --project DatabaseMastery.HotCoffeePostgreSQL.csproj
   dotnet user-secrets set "AdminAuth:Username" "<ADMIN_USERNAME>" --project DatabaseMastery.HotCoffeePostgreSQL.csproj
   dotnet user-secrets set "AdminAuth:Password" "<ADMIN_PASSWORD>" --project DatabaseMastery.HotCoffeePostgreSQL.csproj
   ```

   Required configuration keys:

   - `ConnectionStrings:DefaultConnection`
   - `AdminAuth:Username`
   - `AdminAuth:Password`

3. Apply migrations to your local PostgreSQL database (when authorized for your environment):

   ```bash
   dotnet ef database update --project DatabaseMastery.HotCoffeePostgreSQL.csproj
   ```

## How to run

```bash
dotnet run --project DatabaseMastery.HotCoffeePostgreSQL.csproj
```

Development uses HTTPS redirection. Admin login is at `/Account/Login`.

## How to test

```bash
dotnet test DatabaseMastery.HotCoffeePostgreSQL.sln
```

The automated test suite uses an isolated EF Core InMemory host and does not require PostgreSQL, User Secrets, or business writes to a shared development database.

Optional local vulnerability gate:

```powershell
./scripts/Test-NuGetVulnerabilities.ps1
```

## Route overview

| Area | Route examples |
|------|----------------|
| Public landing / menu | `/`, `/Menu/Index` |
| Public reservation | `/Reservation/CreateReservation`, `/Reservation/Success` |
| Admin login | `/Account/Login` |
| Admin dashboard | `/Dashboard/Index` |
| Admin statistics | `/Statistics/Index` |
| Admin CRUD | `/Category/*`, `/Product/*`, `/Reservation/ReservationList`, `/Review/ReviewList` |
| Health | `/health/live`, `/health/ready` |
| Branded errors | `/Error/404`, `/Error/500` |

## Security architecture summary

- Cookie authentication for admin (`AdminOnly` authorization policy)
- Secure HTTP-only admin cookie (`SecurePolicy=Always` in non-test environments)
- Global antiforgery validation on state-changing POST requests
- POST-only mutation routes with route/body ID integrity checks
- Server-owned reservation status on public create
- Production exception handling with branded 404/500 pages (no stack traces)
- Security response headers (nosniff, frame denial, referrer policy, permissions policy, baseline CSP)
- Admin login POST rate limiting (default: 10 attempts / 5 minutes / client IP)
- Public reservation POST rate limiting (default: 20 submissions / 10 minutes / client IP)
- Authenticated admin HTML responses marked `Cache-Control: no-store`

## Health endpoints

- `GET /health/live` — process liveness only (no database query)
- `GET /health/ready` — includes PostgreSQL connectivity via `CanConnectAsync`

Responses are minimal JSON (`Healthy` / `Unhealthy`) and do not expose connection details.

## Known production deployment requirements

See [`docs/production-checklist.md`](docs/production-checklist.md) for deployment responsibilities, including:

- TLS/HTTPS termination and HSTS at the edge or host
- Trusted reverse-proxy forwarded-header configuration (`ReverseProxy:Enabled` with explicit known proxies/networks only)
- Secret-managed database connection string and `AdminAuth` credentials via environment/platform secrets
- Persistent ASP.NET Core Data Protection key ring for multi-instance admin cookie compatibility
- Database backup and migration application procedure
- Health monitoring on `/health/live` and `/health/ready`
- Periodic NuGet vulnerability audits (also enforced in CI)

Do not enable broad trust-all reverse-proxy header forwarding. Configure only the actual deployment proxy or load balancer addresses.

## CI

GitHub Actions workflow: `.github/workflows/ci.yml`

- Restore tools and packages
- Release build
- Release tests
- NuGet vulnerability gate (direct + transitive packages)

## License

Application license decision deferred to repository owner.
