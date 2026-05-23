# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Run the API (from repo root)
dotnet run --project HeathCare.Api

# Build entire solution
dotnet build

# EF migrations (run from repo root; design assembly is HeathCare.Api)
dotnet ef migrations add <Nombre> --project HealthCare.Infrastructure --startup-project HeathCare.Api
dotnet ef database update          --project HealthCare.Infrastructure --startup-project HeathCare.Api

# API docs (dev only)
# http://localhost:<port>/scalar/v1
```

## Architecture

Five projects with strict dependency direction: `Domain` ← `Application` ← `Infrastructure` ← `Api`. `Shared` is referenced by everyone.

| Project | Role |
|---|---|
| `HealthCare.Domain` | Entities, repository interfaces, no framework deps |
| `HealthCare.Application` | CQRS handlers, validators, DTOs, service interfaces |
| `HealthCare.Infrastructure` | EF Core (PostgreSQL/Npgsql), repositories, JWT, SMTP |
| `HealthCare.Shared` | `ApiResponse<T>`, `PagedResult<T>`, `AppExceptions`, `ValidationBehavior` |
| `HeathCare.Api` | Controllers, `ExceptionMiddleware`, DI wiring (`ServiceCollectionExtension`) |

Note: the API project is named `HeathCare.Api` (typo, missing 'l') — match this exactly in commands.

## CQRS convention

Every use case lives in `HealthCare.Application/Modules/<Domain>/`:

```
Commands/<Action>/
  <Action>Command.cs          — IRequest<TResponse>
  <Action>CommandHandler.cs   — IRequestHandler<TCommand, TResponse>
  <Action>CommandValidator.cs — AbstractValidator<TCommand>   (when needed)
Queries/<Action>/
  <Action>Query.cs
  <Action>QueryHandler.cs
DTOs/
  <Entity>Dto.cs
  Requests/<Action>Request.cs
```

Controllers only call `mediator.Send(...)` — no business logic.

## Error handling

`ExceptionMiddleware` is the single place that maps exceptions to HTTP status codes. **Always throw `AppExceptions.*` from handlers, never return error objects or throw `KeyNotFoundException`/`InvalidOperationException`.**

| Exception | HTTP |
|---|---|
| `AppExceptions.NotFoundException` | 404 |
| `AppExceptions.ConflictException` | 409 |
| `AppExceptions.BadRequestException` | 400 |
| `AppExceptions.UnauthorizedException` | 401 |
| `AppExceptions.ForbiddenException` | 403 |
| `FluentValidation.ValidationException` (via `ValidationBehavior`) | 400 |

Controllers must **not** have try/catch blocks — existing ones in `UserController` and `RoleController` are legacy and should be migrated.

## Validation

`ValidationBehavior<,>` is registered in the MediatR pipeline. Adding an `AbstractValidator<TCommand>` to the same folder as the command is sufficient — it is auto-discovered via `AddValidatorsFromAssemblyContaining<LoginRequestValidator>()`.

## DI registration

- Repositories: auto-registered via Scrutor scan of `HealthCare.Infrastructure.Repositories` namespace.
- Application services (suffix `Service`): auto-registered via Scrutor scan of `HealthCare.Application.Modules` namespace.
- MediatR handlers: registered from `HealthCare.Application` assembly.

## Configuration

All secrets use environment variables with `appsettings.json` as fallback. Key constants are in `HealthCare.Shared/Constants/ConfigurationConstants.cs`. The database uses PostgreSQL (Npgsql) despite any mention of SQL Server elsewhere.

## Auth flow

Auth endpoints use `AuthService` directly (not CQRS). JWT is validated by `JwtProvider`; passwords use PBKDF2-SHA512 via `PasswordHasher`. New users receive a temporary password by email and have `MustChangePassword = true` until they use `ChangePassword`.
