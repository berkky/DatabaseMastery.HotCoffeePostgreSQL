# HotCoffee Production Operations Checklist

Use this checklist when preparing a production deployment. It documents operational responsibilities without prescribing a specific cloud provider.

## Environment and secrets

- [ ] Set `ASPNETCORE_ENVIRONMENT=Production` (or the host-equivalent production environment name).
- [ ] Provide `ConnectionStrings:DefaultConnection` through environment variables or platform secret storage.
- [ ] Provide `AdminAuth:Username` and `AdminAuth:Password` through environment variables or platform secret storage.
- [ ] Confirm no real secrets are committed to source control or publish output.
- [ ] Rotate admin credentials through your secret-management process when personnel change.

## TLS / HTTPS

- [ ] Terminate TLS at the reverse proxy or host with a valid certificate.
- [ ] Ensure HTTPS redirection is active for production traffic.
- [ ] Confirm HSTS is emitted for production responses (application enables HSTS outside Development).
- [ ] Do **not** enable HSTS preload or `includeSubDomains` without explicit domain ownership decisions.

## Reverse proxy / load balancer

- [ ] Configure only trusted proxy addresses in application configuration:
  - `ReverseProxy:Enabled=true`
  - `ReverseProxy:KnownProxies[]` and/or `ReverseProxy:KnownNetworks[]`
- [ ] Do **not** clear known-proxy restrictions or trust all forwarded headers.
- [ ] Document the actual proxy/load balancer IP ranges for your deployment.
- [ ] Verify client IP partitioning for rate limiting uses the effective `RemoteIpAddress` established by forwarded-header middleware.

## Database

- [ ] Provision PostgreSQL with backups and restore testing.
- [ ] Apply EF Core migrations during a controlled deployment window.
- [ ] Verify foreign-key delete rules remain intentional (`Products -> Categories` RESTRICT, `Reviews -> Products` RESTRICT).
- [ ] Monitor `/health/ready` for database connectivity failures.

## ASP.NET Core Data Protection

- [ ] Provide a persistent Data Protection key ring strategy for multi-instance deployments so admin authentication cookies remain valid across restarts/instances.
- [ ] Store key material in a secure, backed-up location appropriate to your host (filesystem volume, secret store, or platform key service).

## Application hardening verification

- [ ] Branded `/Error/404` and `/Error/500` responses do not expose exception details.
- [ ] Security headers present on public and admin HTML responses:
  - `X-Content-Type-Options: nosniff`
  - `Referrer-Policy: strict-origin-when-cross-origin`
  - `X-Frame-Options: DENY`
  - `Permissions-Policy: geolocation=(), camera=(), microphone=()`
  - Baseline CSP with `frame-ancestors 'none'`
- [ ] Admin authenticated pages return `Cache-Control: no-store`.
- [ ] Login POST rate limit defaults: 10 attempts / 5 minutes / client IP.
- [ ] Reservation POST rate limit defaults: 20 submissions / 10 minutes / client IP.
- [ ] HTTP 429 responses include `Retry-After` when available.

## Health monitoring

- [ ] Monitor `GET /health/live` for process availability (no database dependency).
- [ ] Monitor `GET /health/ready` for database readiness.
- [ ] Alert on sustained `503` from `/health/ready`.

## Logging

- [ ] Use platform logging with retention appropriate to operations/security review.
- [ ] Confirm logs do not capture admin passwords, antiforgery tokens, authentication cookies, full connection strings, reservation phone/email, or full customer review bodies.
- [ ] Do not enable verbose HTTP request-body logging in production.

## Package vulnerability audit

- [ ] Run `./scripts/Test-NuGetVulnerabilities.ps1` before release.
- [ ] Ensure CI vulnerability gate passes for direct and transitive packages.
- [ ] Remediate discovered vulnerabilities before promoting a release candidate.

## Rate-limit defaults (reference)

| Policy | Default |
|--------|---------|
| Admin login POST | 10 requests / 5 minutes / IP |
| Public reservation POST | 20 requests / 10 minutes / IP |

Adjust through `RateLimiting` configuration if operational needs require different thresholds.

## Rollback planning

- [ ] Keep previous application build artifacts available.
- [ ] Document migration rollback constraints (EF migrations may require forward-fix strategies).
- [ ] Validate restore-from-backup procedure for PostgreSQL.
- [ ] Define a communication path for reservation/customer impact during rollback.

## Repository hygiene (release checkpoint)

- [ ] Ensure `.gitignore` excludes `bin/`, `obj/`, `TestResults/`, coverage, and publish artifacts for future commits.
- [ ] Schedule tracked generated-artifact index cleanup (`git rm --cached` for historical `bin/` / `obj/`) as an explicit release staging action when authorized.

## Post-deploy smoke checks (read-only)

- [ ] `/` and `/Menu/Index` return 200.
- [ ] `/Reservation/CreateReservation` and `/Reservation/Success` return 200.
- [ ] `/Account/Login` returns 200.
- [ ] `/health/live` and `/health/ready` return 200 when database is reachable.
- [ ] Anonymous `/Dashboard/Index` returns 302 to login.
- [ ] Authenticated admin list/dashboard routes return 200.

Perform smoke checks without business mutations unless a dedicated staging environment is intended for write testing.
