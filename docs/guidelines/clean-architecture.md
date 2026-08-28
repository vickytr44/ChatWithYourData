# Clean Architecture Guidelines

## Overview
Every microservice in `ChatWithYourData` follows Clean Architecture (Ports and Adapters / Onion Architecture) to guarantee testability, maintainability, and domain isolation.

---

## 1. Layer Definitions & Responsibilities

```
[ Domain Layer ]  <--- Pure Business Entities & Rules (Zero Dependencies)
      ^
[ Application Layer ] <--- Use Cases, CQRS Handlers, DTOs, Interfaces
      ^
[ Infrastructure Layer ] & [ API Layer ] <--- EF Core, SQLite, GraphQL, Controllers/Endpoints
```

### 1. Domain (`<ServiceName>.Domain`)
- **Dependencies**: None. Pure .NET standard/C#.
- **Contents**:
  - `Entities/`: Domain entities inheriting from `BaseEntity<TId>` or `IAggregateRoot`.
  - `ValueObjects/`: Immutable value objects with equality semantics.
  - `Exceptions/`: Domain-specific exceptions (e.g. `DocumentNotFoundException`, `InvalidChunkSizeException`).
  - `Events/`: Domain events for state changes.

### 2. Application (`<ServiceName>.Application`)
- **Dependencies**: `<ServiceName>.Domain` only.
- **Contents**:
  - `Interfaces/`: Repository interfaces (`IDocumentRepository`), service contracts (`IPdfParserService`), Unit of Work.
  - `Features/`: CQRS Commands, Queries, and Handlers grouped by feature (e.g. `Features/Documents/UploadDocument/`).
  - `DTOs/`: Response and input models.
  - `Validators/`: FluentValidation validators for commands/queries.
  - `Behaviors/`: Pipeline behaviors (ValidationBehavior, LoggingBehavior).

### 3. Infrastructure (`<ServiceName>.Infrastructure`)
- **Dependencies**: `<ServiceName>.Application`, `<ServiceName>.Domain`.
- **Contents**:
  - `Persistence/`: `DbContext`, EF Core Entity Type Configurations (`IEntityTypeConfiguration<T>`), Migrations.
  - `Repositories/`: Concrete implementations of Application repository interfaces.
  - `ExternalServices/`: Third-party clients, file storage adapters, parser implementations.

### 4. API (`<ServiceName>.API`)
- **Dependencies**: `<ServiceName>.Application`, `<ServiceName>.Infrastructure`.
- **Contents**:
  - `Endpoints/` / `GraphQL/`: Minimal API routes or Hot Chocolate GraphQL Query/Mutation/Type definitions.
  - `Middleware/`: Global exception handling (ProblemDetails RFC 7807), logging/correlation IDs.
  - `Program.cs`: Dependency injection composition root.

---

## 2. CQRS & Handler Pattern
- Commands and Queries should be isolated as discrete records:
  ```csharp
  public record UploadDocumentCommand(string Title, Stream Content, string ContentType) : IRequest<Result<DocumentDto>>;
  ```
- Use the **Result Pattern** (`Result<T>`) for operational outcomes to avoid throwing exceptions for regular control flows.

---

## 3. Project Configuration (.NET 10)

Every `.csproj` file in the solution targets `.NET 10` and enables nullable reference types:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>
</Project>
```
