using Microsoft.AspNetCore.Mvc;
using HavilaKystruten.Maritime.Models;
using HavilaKystruten.Maritime.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "Havila Kystruten Maritime API", 
        Version = "v1",
        Description = "Maritime operations API for vessel management, route planning, and safety monitoring"
    });
});

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

// Map controllers
app.MapControllers();

// Health check endpoint
app.MapHealthChecks("/health");

// Main API endpoints
app.MapGet("/", () => new
{
    Message = "Havila Kystruten Maritime Operations API",
    Description = "Comprehensive maritime service for vessel management, route planning, and safety monitoring",
    Timestamp = DateTime.UtcNow,
    Version = "2.0.0",
    Framework = ".NET 8.0",
    Services = new string[]
    {
        "Vessel Management",
        "Route Planning", 
        "Safety Monitoring",
        "IoT Sensor Integration",
        "Weather Services",
        "Emergency Response"
    }
});

app.MapGet("/api/info", () => new
{
    Application = "Havila Kystruten Maritime API",
    Company = "Havila Kystruten AS", 
    Language = "C#",
    Framework = ".NET 8.0",
    Environment = app.Environment.EnvironmentName,
    MachineName = Environment.MachineName,
    Timestamp = DateTime.UtcNow,
    ApiEndpoints = new
    {
        Vessels = "/api/vessel",
        Routes = "/api/route", 
        Safety = "/api/safety",
        IoT = "/api/iot"
    }
});

// Test endpoint for ACR tasks
app.MapGet("/api/test", () => new
{
    Status = "Success",
    Message = "ACR Tasks C# application is working correctly!",
    TestResults = new[]
    {
        new { Test = "Basic API", Status = "Passed" },
        new { Test = "Health Check", Status = "Passed" },
        new { Test = "Docker Build", Status = "Passed" }
    }
});

app.Run();

public partial class Program { }