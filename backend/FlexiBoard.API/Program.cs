using FlexiBoard.Application.Interfaces;
using FlexiBoard.Application.Services;
using FlexiBoard.Infrastructure.ExternalAPIs;
using FlexiBoard.Infrastructure.DataGenerators;
using FlexiBoard.API.BackgroundServices;
using FlexiBoard.API.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container
builder.Services.AddMemoryCache();
builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Register application services
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IOrderGenerator, OrderGenerator>();

// Register infrastructure services
builder.Services.AddHttpClient<IFakeStoreApiClient, FakeStoreApiClient>()
    .ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(10);
    });

// Register background service
builder.Services.AddHostedService<DashboardRefreshService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Remove HTTPS redirection in development
// app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();

app.MapControllers();
app.MapHub<DashboardHub>("/hub/dashboard");

app.Run();
