# System architecture

```mermaid
flowchart TB
    subgraph Aspire["MEMS.AppHost (.NET Aspire): orchestration, service discovery, configuration, dashboard"]
        direction TB
        SQL[("SQL Server container<br/>database: memsdb")]
        API["MEMS.Api (ASP.NET WebAPI)<br/>/api/{tenant}/...  JWT, tenant filter"]
        ADMIN["MEMS.Admin (Blazor Server)<br/>one shared app: /admin<br/>Identity cookie, tenant-scoped services"]
        subgraph PUB["MEMS.Public (Blazor WebAssembly, one site per tenant URL)"]
            B["/burnaby"]
            S["/surrey"]
            R["/richmond"]
        end
    end
    Browser((Visitor)) --> B & S & R
    B -- "GET /api/burnaby/..." --> API
    S -- "GET /api/surrey/..." --> API
    R -- "GET /api/richmond/..." --> API
    Admin((Administrator)) --> ADMIN
    ADMIN -- "EF Core + service layer<br/>(TenantScope)" --> SQL
    ADMIN -. "service discovery<br/>https+http://api/health" .-> API
    API -- "EF Core + service layer<br/>ForTenant(id)" --> SQL
    B -. "voting choices:<br/>Session Storage only<br/>(never sent)" .- Browser
```

## Tenant isolation, layer by layer

| Layer | Mechanism |
|---|---|
| URL | `/burnaby` -> Blazor `TenantShell` loads only `GET /api/burnaby/municipality`; every child page asks for `/api/burnaby/...` |
| API | `TenantResolutionFilter` resolves `{tenant}` (unknown -> 404) and returns 403 when a signed-in Municipality Admin asks for another tenant |
| Service layer | every call takes the tenant id and a `TenantScope`; writes check `scope.CanManage(tenantId)`; queries use `ForTenant(id)` |
| EF Core / SQL | `Candidate (PositionId, MunicipalityId)` is a composite foreign key to `Position`, so a candidate cannot reference another tenant's position; unique indexes on slug and (tenant, position name) |
| Admin app | the active tenant of a Municipality Admin is fixed to the `mems:municipality_id` claim in the cookie; Super Admin chooses from a menu; `/media` image endpoints re-check the scope |

## Entity relationship diagram

```mermaid
erDiagram
    Municipality ||--o{ Position : has
    Municipality ||--o{ Candidate : has
    Position ||--o{ Candidate : "stands for (PositionId + MunicipalityId)"
    Municipality ||--o{ ApplicationUser : "administered by"
    Municipality {
        int MunicipalityId PK
        string Name UK
        string Slug UK
        string Province
        date ElectionDate
        string Description
        string ThemeColor
        blob LogoData
        string LogoContentType
        int LogoVersion
    }
    Position {
        int PositionId PK
        string Name
        int SeatsAvailable
        int MunicipalityId FK
    }
    Candidate {
        int CandidateId PK
        string Name
        string Profile
        blob PhotoData
        string PhotoContentType
        int PhotoVersion
        string Website
        string Email
        datetime CreatedUtc
        int PositionId FK
        int MunicipalityId FK
    }
    ApplicationUser {
        string Id PK
        string UserName
        string DisplayName
        int MunicipalityId FK "null for Super Admin"
    }
```

Roles: `Super Admin` (no tenant) and `Municipality Admin` (exactly one tenant). Ballot selections have no table: the
database cannot store them.
