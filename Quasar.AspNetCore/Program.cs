using Vite.AspNetCore;
using Vite.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

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

app.UseStaticFiles();
app.MapGet("/qwerty", () => "Hello World!");



app.Run();