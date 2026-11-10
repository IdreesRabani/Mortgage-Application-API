using FluentValidation;
using Mortage_Application_API.Domain.Affordability;
using Mortage_Application_API.Logging;
using Mortage_Application_API.Middleware;
using Mortage_Application_API.Repositories;
using Mortgage_Application_API.Domain.Requests;
using Mortgage_Application_API.Domain.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton<IAffordabilityCalculator>(
    new AffordabilityCalculator(new AffordabilityOptions()));

builder.Services.AddSingleton<
    IMortgageApplicationRepository,
    InMemoryMortgageApplicationRepository>();

builder.Services.AddSingleton<InMemoryLogStore>();

builder.Services.AddSingleton<
    ILoggerProvider,
    InMemoryLoggerProvider>();

builder.Services.AddScoped<
    IValidator<CreateMortgageApplicationRequest>,
    CreateMortgageApplicationValidator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapGet("/MortgageApplication/logs", (InMemoryLogStore store) =>
    {
        return Results.Ok(store.GetAll());
    });
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseMiddleware<ApiKeyMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }