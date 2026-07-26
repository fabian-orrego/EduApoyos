using EduApoyos.Api.Configuration;
using EduApoyos.Api.Configuration.ExceptionHandling;
using EduApoyos.Application;
using EduApoyos.Infrastructure;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerConfiguration();
builder.Services.AddClientCors(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);

// The exception-handling middleware is the single source of truth for validation errors,
// so the built-in [ApiController] auto-400 must be turned off (RN: ProblemDetails everywhere).
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

var app = builder.Build();

app.UseExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerConfiguration();
    await app.ApplyPendingMigrationsAsync();
}

app.UseHttpsRedirection();
app.UseCors(ClientCorsExtensions.ClientPolicyName);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

/// <summary>
/// Exposed so the integration test project can boot the API through <c>WebApplicationFactory</c>.
/// </summary>
public partial class Program;
