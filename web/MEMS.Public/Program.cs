using MEMS.Public;
using MEMS.Public.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// The ONLY backend this site talks to is the ASP.NET WebAPI (never the database).
// Opened over https -> call the API over https; opened over http (no dev certificate) -> call it over http.
var openedOverHttp = builder.HostEnvironment.BaseAddress.StartsWith("http://", StringComparison.OrdinalIgnoreCase);
var apiBase = (openedOverHttp ? builder.Configuration["ApiBaseUrlHttp"] : builder.Configuration["ApiBaseUrl"])
              ?? (openedOverHttp ? "http://localhost:5101/" : "https://localhost:7101/");
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBase) });
builder.Services.AddScoped<ApiClient>();

// Ballot selections live in browser Session Storage only (see BallotStore).
builder.Services.AddScoped<BallotStore>();

await builder.Build().RunAsync();
