using Mcp.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication()
    .AddJwtBearer();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanRefund", policy =>
        policy.RequireAuthenticatedUser().RequireClaim("permission", "orders.refund"));
});

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly()
    .WithAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapMcp("/mcp");

app.Run();
