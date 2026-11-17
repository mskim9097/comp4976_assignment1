var builder = DistributedApplication.CreateBuilder(args);

// SQL Server runs in a Docker container (same image as the course samples). The data volume keeps the data
// between runs and Aspire passes the connection string to the projects that reference the database.
var sql = builder.AddSqlServer("sql")
    .WithImageTag("2022-latest")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);
var db = sql.AddDatabase("memsdb");

// Development-only signing key for the JWTs. Anywhere else the Api reads the real one from the Jwt__Key setting.
var jwtKey = builder.AddParameter("jwt-key", () => "development-only-signing-key-0123456789-abcdefghij", secret: true);

// Uploaded photos and logos: the Admin app writes them and the Api serves them, so both use this folder.
var uploads = Path.GetFullPath(Path.Combine(builder.AppHostDirectory, "..", "..", "uploads"));

var api = builder.AddProject<Projects.MEMS_Api>("api")
    .WithReference(db)
    .WithEnvironment("Jwt__Key", jwtKey)
    .WithEnvironment("Uploads__Path", uploads)
    .WaitFor(db);

var admin = builder.AddProject<Projects.MEMS_Admin>("admin")
    .WithReference(db)
    .WithReference(api)
    .WithEnvironment("Uploads__Path", uploads)
    // The browser loads photos and logos from the Api, so the Admin app needs its public address.
    .WithEnvironment("Api__BaseUrl", api.GetEndpoint("https"))
    .WaitFor(api);

var publicSite = builder.AddProject<Projects.MEMS_Public>("public")
    .WithReference(api)
    .WaitFor(api);

// The public site runs in the browser, so the Api needs to know its address (CORS).
admin.WithEnvironment("PublicSite__BaseUrl", publicSite.GetEndpoint("https"));

api.WithEnvironment("Cors__AllowedOrigins__0", publicSite.GetEndpoint("https"));

builder.Build().Run();
