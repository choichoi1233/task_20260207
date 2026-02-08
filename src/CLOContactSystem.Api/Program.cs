using CLOContactSystem.Api.Domain.Interfaces;
using CLOContactSystem.Api.Infrastructure.Parsing;
using CLOContactSystem.Api.Infrastructure.Persistence;
using CLOContactSystem.Api.Application.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// EF Core InMemory Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("CLOContactDb"));

// Repository
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();

// Parsers
builder.Services.AddSingleton<CsvEmployeeParser>();
builder.Services.AddSingleton<JsonEmployeeParser>();

// Validators
builder.Services.AddSingleton<EmployeeValidator>();

// MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Controllers
builder.Services.AddControllers();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CLO Employee Emergency Contact API",
        Version = "v1",
        Description = "CLO Virtual Fashion 직원 긴급 연락망 관리 시스템 API"
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// Global exception handler
app.UseExceptionHandler(appBuilder =>
{
    appBuilder.Run(async context =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        var exceptionFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (exceptionFeature?.Error is not null)
        {
            logger.LogError(exceptionFeature.Error, "Unhandled exception occurred");
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        var errorResponse = new CLOContactSystem.Api.Application.DTOs.ApiResponse<object>
        {
            Success = false,
            Message = "An internal server error occurred.",
            Code = StatusCodes.Status500InternalServerError,
            Data = null
        };
        var errorJson = System.Text.Json.JsonSerializer.Serialize(errorResponse, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });
        await context.Response.WriteAsync(errorJson);
    });
});

// Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CLO Contact API v1");
});

app.UseAuthorization();
app.MapControllers();

app.Run();

// Make Program class accessible for integration testing
public partial class Program { }
