using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Vite.AspNetCore;
using Vite.AspNetCore.Extensions;

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

builder.Services.AddViteServices(new ViteOptions()
{
    Server = new ViteServerOptions()
    {
        Port = 9000
    }
});

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    // Enable the Middleware to use the Vite Development Server.
    app.UseViteDevMiddleware();
}

app.UseSerilogRequestLogging(cf => cf.Logger = logger);

app.UseStaticFiles();
app.MapGet("/api/profile", () => new
{
    Name = "my email",
    Email = "john"
});



app.Run();