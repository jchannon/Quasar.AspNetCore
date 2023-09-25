using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Http.HttpResults;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Vite.AspNetCore;
using Vite.AspNetCore.Extensions;

var rnd = new Random();

var logger = new LoggerConfiguration()
    .WriteTo.Seq("http://localhost:5341")
    .WriteTo.Console(theme: AnsiConsoleTheme.Code)
    .MinimumLevel.Information()
    .MinimumLevel.Override("System.Net.Http", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Host.UseSerilog(logger);

builder.Services
    .AddAuthorization()
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.ReturnUrlParameter = "redirectUri";
    })
    .AddAuth0WebAppAuthentication(options =>
    {
        options.Domain = builder.Configuration["Auth0:Domain"];
        options.ClientId = builder.Configuration["Auth0:ClientId"];
        options.SkipCookieMiddleware = true;
    });

builder.Services.AddViteServices(new ViteOptions()
{
    Server = new ViteServerOptions()
    {
        Port = 9000
    }
});

var app = builder.Build();

app.UseSerilogRequestLogging(cf => cf.Logger = logger);

if (app.Environment.IsDevelopment())
{
    // Enable the Middleware to use the Vite Development Server.
    app.UseViteDevMiddleware();
}

app.Map("/installer-portal", builder =>
{
	app.UseCookiePolicy(new CookiePolicyOptions()
	{
	    Secure = CookieSecurePolicy.Always,
	    HttpOnly = HttpOnlyPolicy.Always,
	    MinimumSameSitePolicy = SameSiteMode.None
	});

    builder.UseStaticFiles();
    
    if (app.Environment.IsDevelopment())
    {
        // Enable the Middleware to use the Vite Development Server.
        app.UseViteDevMiddleware();
    }

});

app.UseAuthentication();
app.UseAuthorization();

app.UseCookiePolicy(new CookiePolicyOptions()
{
    Secure = CookieSecurePolicy.Always,
    HttpOnly = HttpOnlyPolicy.Always,
    MinimumSameSitePolicy = SameSiteMode.None
});

app.MapGet("/api/login", async (HttpContext context, string redirectUri) =>
{
    var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
        .WithRedirectUri(redirectUri)
        .Build();

    await context.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
});

app.MapGet("/api/logout", async (HttpContext context) =>
{
    var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
        .WithRedirectUri("/")
        .Build();

    await context.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
});

app.MapGet("/api/profile", (HttpContext context) =>
    {
        var number = rnd.NextDouble();
        if (number >= 0.5)
        {
            return Results.Ok(new
            {
                Name = context.User.Identity.Name,
                Email = "john@smith.com"
            });
        }

        return Results.StatusCode(403);
    })
    .RequireAuthorization();


app.Run();