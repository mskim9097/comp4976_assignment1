using MEMS.Data;
using MEMS.Data.Entities;
using MEMS.Data.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Database: the connection string "memsdb" comes from the Aspire AppHost.
builder.Services.AddMemsTenancy();
// Not a pooled context: it takes the scoped TenantContext. Aspire adds health checks, telemetry and retries.
builder.Services.AddDbContext<MemsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("memsdb")));
builder.EnrichSqlServerDbContext<MemsDbContext>();
builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequiredLength = 6;
        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<MemsDbContext>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Create/update the database and load the sample data (every host waits for the Api in the AppHost).
using (var scope = app.Services.CreateScope())
{
    await MemsDbSeeder.InitializeAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapDefaultEndpoints();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
