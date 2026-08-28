# AGENTS.md - Project Guidelines & Standards

## 1. Project Overview
- **Project Name**: ChatWithYourData
- **Backend Stack**: .NET 10 (`net10.0`) / C#
- **API Gateway**: ChilliCream Fusion (Federated GraphQL Gateway)
- **AI Agent & Protocol**: Microsoft Agent Framework (`Microsoft.Agents.AI`) with AG-UI Protocol (SSE-based Agent-User Interaction)
- **Model Context Protocol (MCP)**: ChilliCream MCP Adapter (`HotChocolate.Fusion.Adapters.Mcp`) exposing GraphQL queries/mutations as MCP tools at `/graphql/mcp`
- **Architecture**: Clean Architecture & Microservices
- **Methodology**: Test-Driven Development (TDD)
- **Data Persistence**: Entity Framework Core 10 with SQLite (configurable to In-Memory for testing/local development)

---

## 2. Documentation & Architecture Map

Detailed technical specifications and guidelines are modularized into dedicated reference documents:

| Topic | Reference Document | Description |
| :--- | :--- | :--- |
| **Microservices** | [docs/architecture/microservices.md](file:///c:/Users/vicky/.gemini/antigravity/scratch/ChatWithYourData/docs/architecture/microservices.md) | Responsibilities, DB schemas, and subgraphs for the 4 services |
| **Gateway & MCP** | [docs/architecture/gateway-and-mcp.md](file:///c:/Users/vicky/.gemini/antigravity/scratch/ChatWithYourData/docs/architecture/gateway-and-mcp.md) | ChilliCream Fusion GraphQL gateway & MCP adapter setup (`/graphql/mcp`) |
| **Agent & AG-UI** | [docs/architecture/agent-and-agui.md](file:///c:/Users/vicky/.gemini/antigravity/scratch/ChatWithYourData/docs/architecture/agent-and-agui.md) | Microsoft Agent Framework & AG-UI SSE protocol streaming & state sync |
| **Clean Architecture** | [docs/guidelines/clean-architecture.md](file:///c:/Users/vicky/.gemini/antigravity/scratch/ChatWithYourData/docs/guidelines/clean-architecture.md) | 4-layer separation (Domain, Application, Infrastructure, API) & CQRS |
| **TDD & Testing** | [docs/guidelines/testing-tdd.md](file:///c:/Users/vicky/.gemini/antigravity/scratch/ChatWithYourData/docs/guidelines/testing-tdd.md) | Red-Green-Refactor workflow, test project layout, and test stack |

---

## 3. Solution Directory Structure

```
ChatWithYourData/
├── AGENTS.md
├── run-services.bat
├── stop-services.bat
├── docs/
│   ├── architecture/
│   │   ├── microservices.md
│   │   ├── gateway-and-mcp.md
│   │   └── agent-and-agui.md
│   └── guidelines/
│       ├── clean-architecture.md
│       └── testing-tdd.md
├── src/
│   ├── Gateway/
│   │   └── ChatWithYourData.Gateway/
│   └── Services/
│       ├── ChatWithYourData.InventoryService/
│       ├── ChatWithYourData.SalesService/
│       ├── ChatWithYourData.ProcurementService/
│       └── ChatWithYourData.FinanceService/
└── tests/
    └── Services/
        ├── ChatWithYourData.InventoryService.UnitTests/
        ├── ChatWithYourData.InventoryService.IntegrationTests/
        ├── ChatWithYourData.SalesService.UnitTests/
        ├── ChatWithYourData.SalesService.IntegrationTests/
        ├── ChatWithYourData.ProcurementService.UnitTests/
        ├── ChatWithYourData.ProcurementService.IntegrationTests/
        ├── ChatWithYourData.FinanceService.UnitTests/
        └── ChatWithYourData.FinanceService.IntegrationTests/
```

---

## 4. Coding & Design Conventions

- **Nullable Reference Types**: Enabled across all projects (`<Nullable>enable</Nullable>`).
- **C# Modern Features**: Use primary constructors, pattern matching, records for DTOs/Events where appropriate.
- **Async & Cancellation**: All I/O operations must be `async` and accept `CancellationToken`.
- **Validation**: FluentValidation for request validation via Application pipeline behaviors.
- **Error Handling**: Use Result pattern or centralized exception-handling middleware (ProblemDetails / RFC 7807).
- **Database per Service**: Independent DbContext and SQLite database file per microservice.

---

## 5. Technology & MCP Server Extension Policy

Whenever a new framework, library, external integration, or **MCP (Model Context Protocol) server/tool** is introduced to the project:

1. **Mandatory Documentation**:
   - Update [`AGENTS.md`](file:///c:/Users/vicky/.gemini/antigravity/scratch/ChatWithYourData/AGENTS.md) with the new component name, version, and role.
   - Update or create a corresponding detail document under [`docs/architecture/`](file:///c:/Users/vicky/.gemini/antigravity/scratch/ChatWithYourData/docs/architecture/) or [`docs/guidelines/`](file:///c:/Users/vicky/.gemini/antigravity/scratch/ChatWithYourData/docs/guidelines/).
   - For **MCP Servers/Tools**: Document their URI/endpoint, exposed tool definitions, required parameters, and authorization requirements in [`docs/architecture/gateway-and-mcp.md`](file:///c:/Users/vicky/.gemini/antigravity/scratch/ChatWithYourData/docs/architecture/gateway-and-mcp.md).
2. **New Session Discovery**:
   - At the start of any session or task, AI agents must read [`AGENTS.md`](file:///c:/Users/vicky/.gemini/antigravity/scratch/ChatWithYourData/AGENTS.md) and inspect the documentation map to utilize the latest registered tools, frameworks, and patterns.

---

## 6. Commit Message Rules

- Write commit messages in the imperative mood (`Add`, `Fix`, `Remove`, `Update`).
- Keep the subject line short (50 characters or fewer).
- No prefixes (`feat:`, `fix:`) unless Conventional Commits are explicitly configured.
- Subject line only (no body, bullet points, or explanations).
