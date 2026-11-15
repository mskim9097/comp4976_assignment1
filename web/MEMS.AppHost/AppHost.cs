// MEMS - Municipal Elections Management System (Assignment 2)
//
//   SQL Server (container) --> MEMS.Api (ASP.NET WebAPI, tenant aware) <-- MEMS.Public (Blazor WASM, one site per tenant URL)
//                         \--> MEMS.Admin (Blazor Server, shared by all tenants, /admin)
//
// Run this project (F5). The Aspire dashboard shows every resource, its logs, traces and health checks.
var builder = DistributedApplication.CreateBuilder(args);

// Connection strings are owned by Aspire: nothing in the applications hard-codes the SQL Server address or password.
var sql = builder.AddSqlServer("sql");
var db = sql.AddDatabase("memsdb");

// Signing key for the API's JWT tokens. Set it with:  dotnet user-secrets set Parameters:jwt-key "<32+ random chars>"
// (appsettings.json in this project carries a development-only default so the solution runs out of the box.)
var jwtKey = builder.AddParameter("jwt-key", secret: true);

// The API owns migrations and seed data, so everything else waits for it to be healthy.
var api = builder.AddProject<Projects.MEMS_Api>("api")
    .WithReference(db)
    .WaitFor(db)
    .WithEnvironment("Jwt__Key", jwtKey)
    .WithHttpHealthCheck("/health");

// Shared administration site (https://localhost:7103/admin). Talks to SQL Server directly through EF Core and to the
// API through Aspire service discovery (https+http://api).
builder.AddProject<Projects.MEMS_Admin>("admin")
    .WithReference(db)
    .WithReference(api)
    .WaitFor(api)
    .WithHttpHealthCheck("/health");

// Public Blazor WebAssembly site (https://localhost:7102/burnaby, /surrey, /richmond ...). It runs in the browser, so it
// can only reach the API at its published address (wwwroot/appsettings.json -> ApiBaseUrl). The reference below makes the
// dependency visible in the dashboard and starts the site after the API.
builder.AddProject<Projects.MEMS_Public>("public")
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
