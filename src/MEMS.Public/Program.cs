using MEMS.Public;
using MEMS.Public.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// The Api address comes from wwwroot/appsettings.json (a browser app cannot use the server-side Aspire service discovery).
var apiBaseUrl = (builder.Configuration["Api:BaseUrl"] ?? builder.HostEnvironment.BaseAddress).TrimEnd('/') + "/";
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });
builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped<TenantService>();
builder.Services.AddScoped<GuideService>();

await builder.Build().RunAsync();
