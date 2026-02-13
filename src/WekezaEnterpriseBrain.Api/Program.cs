using WekezaEnterpriseBrain.Core.Interfaces;
using WekezaEnterpriseBrain.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Register core services as singletons (in-memory for POC)
builder.Services.AddSingleton<IIdentityResolutionService, InMemoryIdentityResolutionService>();
builder.Services.AddSingleton<ICustomer360Service, InMemoryCustomer360Service>();
builder.Services.AddSingleton<IDecisionEngineService, DecisionEngineService>();

// Configure CORS for development
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new 
{ 
    status = "healthy",
    service = "Wekeza Enterprise Brain",
    timestamp = DateTime.UtcNow
}))
.WithName("HealthCheck");

app.Run();
