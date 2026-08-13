---
description: 
alwaysApply: true
---

## Quick Overview

Internal Installment Sales Management REST API built with ASP.NET Core (.NET 9), backed by a single PostgreSQL
database. Monolithic — Clean Architecture with CQRS (MediatR), Result Pattern, and lightweight DDD domain aggregates; no
microservices. Branch-based multi-tenancy with dynamic role-permission access control. Backend for an internal web
admin panel and mobile app — no external customer-facing users; no external business service integrations (optional Seq
logging only).

Purpose: Digitize traditional offline installment sales operations in Iraq — manage inventory, sales, installment
collections, accounting, employees, and related internal workflows within one organization across multiple branches.

## Your role

- You are fluent in C# and asp.net core framework
- You write business logic code in `Application/` layer
- You write api endpoints of application layer use cases in `Api/` layer

## Tech Stack

- Asp.Net Core
- C#
- .Net 9

## Important Packages

- Entity Framework Core
- Npgsql
- MediatR
- FluentValidation
- Serilog

## Clean Architecture

- Domain layer: Entities of my system
- Application layer: Use cases and business logics implementations, interfaces, common utilities, extensions, error
  classes
- Infrastructure layer: Persistence logic, database context, migrations, third-party integrations
- Api layer: Web API controllers, Authorization policies

## CQRS Pattern

- Commands: Write operations (Create, Update, Delete) → `IRequest<Result>`
- Queries: Read operations (GetById, GetPaginated) → `IRequest<Result<TData>>`
- MediatR handles dispatching
- Each operation has: Command/Query, Handler, Validator (for commands), Response (for queries)

## Result Pattern

Use `Result` and `Result<TData>` instead of exceptions for business logic:

- `Result.Success()` / `Result.Failed(Error)`
- Implicit operators allow returning `Error` or `TData` directly
- `result.IsSucceeded` / `result.IsFailed` for checking status

---

## Naming Conventions

| Component   | Pattern                            | Example                                            |
|-------------|------------------------------------|----------------------------------------------------|
| Commands    | `{Action}{Entity}Command`          | `CreateUserCommand`, `UpdateProductCommand`        |
| Handlers    | `{Action}{Entity}Handler`          | `CreateUserHandler`, `DeleteOrderHandler`          |
| Validators  | `{Action}{Entity}CommandValidator` | `CreateUserCommandValidator`                       |
| Queries     | `Get{Filter}{Entity}Query`         | `GetUserByIdQuery`, `GetPaginatedUsersQuery`       |
| Responses   | `{QueryName}Response`              | `GetUserByIdResponse`, `GetPaginatedUsersResponse` |
| DTOs        | `{Action}{Entity}Dto`              | `UpdateUserDto`, `CreateOrderDto`                  |
| Errors      | `{Entity}Errors` (static class)    | `UserErrors`, `OrderErrors`, `ProductErrors`       |
| Controllers | `{Entity}Controller` (plural)      | `UsersController`, `OrdersController`              |

---

## Application layer folder structure

```
Application/
└── {Entity}/
    ├── Commands/
    │   ├── {Area}/              # e.g., Admin, User, Common
    │   │   └── {Action}/
    │   │       ├── {Action}{Entity}Command.cs
    │   │       ├── {Action}{Entity}CommandValidator.cs
    │   │       └── {Action}{Entity}Handler.cs
    │   └── {ChildEntity}/       # Nested resources (Add, Update, Remove)
    ├── Queries/
    │   └── {Area}/
    │       └── {Action}/
    │           ├── {Action}{Entity}Query.cs
    │           ├── {Action}{Entity}Handler.cs
    │           └── {Action}{Entity}Response.cs
    └── Common/
        └── {Entity}Errors.cs
```

Real world example:

```
Application/
└── AudioDevices/
    ├── Commands/
    │   ├── Admin/              # e.g., Admin, User, Common
    │   │   └── Create/
    │   │       ├── CreateAudioDeviceCommand.cs
    │   │       ├── CreateAudioDeviceCommandValidator.cs
    │   │       └── CreateAudioDeviceHandler.cs
    ├── Queries/
    │   │── Admin/
    │   │    └── GetById/
    │   │        ├── GetAudioDeviceByIdQuery.cs
    │   │        ├── GetAudioDeviceByIdHandler.cs
    │   │        └── GetAudioDeviceByIdResponse.cs
    │   └── App/
    │       └── GetAll/
    │           ├── GetAllAudioDevicesQuery.cs
    │           ├── GetAllAudioDevicesHandler.cs
    │           └── GetAllAudioDevicesResponse.cs        
    └── Common/
        └── AudioDeviceErrors.cs
```

## Implementation Checklist

## Commands (Write Operations)

- Create folder: `Application/{Entity}/Commands/{Area}/{Action}/`
- Command file: `{Action}{Entity}Command.cs`
    - Use `record` implementing `IRequest<Result>` or `IRequest<Result<TData>>`
    - Properties with `{ get; set; }` and `required` for mandatory fields
    - Use primary constructor syntax for records with only one property
    - Use standard record syntax for records with multiple properties
    - Update use cases should have separate dto for updating properties. inside the command file (not separate file)
- Validator file: `{Action}{Entity}CommandValidator.cs`
    - Inherit `AbstractValidator<TCommand>`
    - Localized error messages (based on project culture, Most common: English, Persian, Arabic)
    - File validation for IFormFile properties
- Handler file: `{Action}{Entity}Handler.cs`
    - Defensive checks (existence, uniqueness, dependencies)
    - Return business errors (not exceptions)
    - Use `cancellationToken` everywhere
    - Try to make all database transactions happen atomically as much as possible (single SaveChangesAsync() call)

## Queries (Read Operations)

- Create folder: `Application/{Entity}/Queries/{Area}/{Action}/`
- Query file: `{Action}{Entity}Query.cs`
    - Use `record` implementing `IRequest<Result<TResponse>>`
    - Use `Pagination` type for pagination parameters
    - Use `PaginatedList<T>` type for response
    - Create a dedicated filter dto for query parameters
    - Use primary constructor syntax for records with only one property
    - Use standard record syntax for records with multiple properties
- Response file: `{Action}{Entity}Response.cs`
    - Use `record` for DTOs
- Handler file: `{Action}{Entity}Handler.cs`
    - Use `.AsNoTracking()` for queries
    - Project to DTOs with `.Select()`
    - Return error if not found
    - Use `.ToPaginatedListAsync()` for paginated results
    - Use `.When()` for conditional filters

---

### File upload rules

- Include file in Create (POST) endpoint of entity
- Do not include file in Update (PUT) endpoint of entity
- Write separate endpoint to upload file in replace or add/remove scenario based on the required or optional property
- Use AttachmentUtility class in Application/Common/Utilities for file-related validation

File Validation example:

```
RuleFor(x => x.Image)
  .NotEmpty()
  .WithMessage("Image file is required")
  .Must(x => AttachmentUtility.ImageContentTypes.Contains(x.ContentType))
  .WithMessage("Image file content type is invalid")
  .Must(x => ByteSize.FromBytes(x.Length) <= ByteSize.FromMegaBytes(2))
  .WithMessage("Image can not be larger than 2MB");
```

---

### Business Errors

- Error class location: `{Entity}/Common/{Entity}Errors.cs`
- Error record signature: `public readonly record struct Error(string Message, string Code);`
- Business error example: `public static Error UserNotFound = new("user has not found", "User_Not_Found");`

---

## Defensive programming conventions

1. Entity Existence
    - Use `AnyAsync()` when only checking existence
    - Use `FirstOrDefaultAsync()` when entity is needed
    - Always check `if (entity is null) return Error;`
2. Collection Validation
    - When accepting ID lists: Verify all IDs found (`count != request.count`)
3. Dependency Checks
    - Check counts before deletion: `if (entity.Dependencies.Any()) return Error;`
4. Uniqueness (based on names)
    - Check with `AnyAsync()` before create: `if (exists) return AlreadyExist;`

---

## API Layer

### Controller Setup

Base Controller:

- Inherit from `ApiController` (abstract base class)
- Route: `[Route("api/v{version:apiVersion}/{area}/[Controller]")]`
- CancellationToken: exists in ApiController parent class, and it is not required to be passed in action methods

Areas:

- Admin area route prefix: `/admin`
- User area route prefix: does not have
- Custom area route prefix: `/custom` => example `/app` for app area

### HTTP Verbs & Routes

| Operation     | Verb     | Route                       | Example                             |
|---------------|----------|-----------------------------|-------------------------------------|
| Create        | POST     | `/{resource}`               | `POST /api/v1/admin/users`          |
| Read (single) | GET      | `/{resource}/{id}`          | `GET /api/v1/admin/users/5`         |
| Read (list)   | GET      | `/{resource}`               | `GET /api/v1/admin/users`           |
| Update        | PUT      | `/{resource}/{id}`          | `PUT /api/v1/admin/users/5`         |
| Delete        | DELETE   | `/{resource}/{id}`          | `DELETE /api/v1/admin/users/5`      |
| Custom        | POST/PUT | `/{resource}/{id}/{action}` | `POST /api/v1/admin/users/5/avatar` |

### Nested Resources

```
/api/v1/admin/courses/{courseId}/chapters
/api/v1/admin/courses/{courseId}/chapters/{chapterId}/episodes
```

## Custom Rules

- string properties validation in application layer should follow max length constraint in entity configuration file of
  the entity located in /Infrastructure/Persistence/EntityTypeConfigurations/{Entity}
- write negative conditions (if and else if statements) with explicit 'false' value instead of ! operator
- if there is any "Order" or "Index" or "DisplayPriority" property in an entity then:
  you should implement a use case for changing this order
  you should not get "Order" or "Index" or "DisplayPriority" as input in the creation use case, instead you should set that
  property automatically
- all entities should have their own update method

### Never do

- Modify code not related to the task at hand
- Edit configuration files (appsettings files, ci/cd files, .gitignore)
- Hard code passwords, api keys, base urls, etc.
- Write comment in code
- Write a new documentation file for the task at hand
- Generate migration file
