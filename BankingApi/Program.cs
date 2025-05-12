using BankingApi.Data;
using Microsoft.EntityFrameworkCore;
using BankingApi.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Register API Key authentication
builder.Services
    .AddAuthentication("ApiKey")
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
        "ApiKey", options => { });

builder.Services.AddAuthorization();

// Add controllers, Swagger, etc.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "BankingApi", Version = "v1" });
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "X-API-Key: {your_api_key}",
        In = ParameterLocation.Header,
        Name = "X-API-Key",
        Type = SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            new string[] {}
        }
    });
});

// Only register SQL Server when NOT in Test environment
if (!builder.Environment.IsEnvironment("Test"))
{
    builder.Services.AddDbContext<BankingContext>(opt =>
        opt.UseSqlServer(builder.Configuration.GetConnectionString("BankingDB")));
}

var app = builder.Build();

// Auto-create DB (no-op for InMemory, works for SQL)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BankingContext>();
    db.Database.EnsureCreated();
}

// Enable Swagger and middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();


public partial class Program { }
