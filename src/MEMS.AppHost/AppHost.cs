var builder = DistributedApplication.CreateBuilder(args);

// SQL Server runs in a Docker container (same image as the course samples). The data volume keeps the data
// between runs and Aspire passes the connection string to the projects that reference the database.
var sql = builder.AddSqlServer("sql")
    .WithImageTag("2022-latest")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);
var db = sql.AddDatabase("memsdb");

var api = builder.AddProject<Projects.MEMS_Api>("api")
    .WithReference(db)
    .WaitFor(db);

builder.AddProject<Projects.MEMS_Admin>("admin")
    .WithReference(db)
    .WithReference(api)
    .WaitFor(api);

builder.AddProject<Projects.MEMS_Public>("public")
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
