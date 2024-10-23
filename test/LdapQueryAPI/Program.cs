using Microsoft.AspNetCore.Authentication.Negotiate;
using System.Net;
using LdapQueryAPI.Services;
using LdapQueryAPI.Services.Ldap;
using LdapQueryAPI.Services.Ldap.Interfaces;
using LdapQueryAPI.Models.ActiveDirectory;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Logger
_ = builder.Host.UseSerilog((context, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration));

builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy.
    options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services
   .AddAuthentication(NegotiateDefaults.AuthenticationScheme)
   .AddNegotiate();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    // X-Forwarded-For, X-Forwarded-Host and X-Forwarded-Proto.
    options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.All;

    foreach (
        var proxy in builder.Configuration
            .GetSection("KnownProxies")
            .AsEnumerable()
            .Where(c => c.Value != null)
    )
    {
        if (proxy.Value != null)
        {
            options.KnownProxies.Add(IPAddress.Parse(proxy.Value)); // read our proxies
        }
    }
});

builder.Configuration.AddJsonFile(
    "custom/appsettings.Custom.json",
    optional: false,
    reloadOnChange: true
);

// Configure Active Directory config.
var activeDirectoryConfig = builder.Configuration.GetSection(nameof(ActiveDirectoryConfig)).Get<ActiveDirectoryConfig>();
builder.Services.AddSingleton(_ => activeDirectoryConfig);

// Configure services
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services
    .AddScoped<IActiveDirectoryService, LdapConnectService>();
builder.Services
    .AddScoped<ICurrentUserService, CurrentUserService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


// Setting default Static Logger.
Log.Logger = app.Services.GetService<Serilog.ILogger>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
