using WekezaEnterpriseBrain.Core.Interfaces;
using WekezaEnterpriseBrain.Core.Services;
using WekezaEnterpriseBrain.Core.Models;
using WekezaEnterpriseBrain.Core.DataSources;
using WekezaEnterpriseBrain.Core.Events;
using WekezaEnterpriseBrain.Core.Features;
using WekezaEnterpriseBrain.Infrastructure;
using WekezaEnterpriseBrain.Infrastructure.EventBus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Register core services as singletons (in-memory for POC)
builder.Services.AddSingleton<IIdentityResolutionService, InMemoryIdentityResolutionService>();
builder.Services.AddSingleton<ICustomer360Service, InMemoryCustomer360Service>();
builder.Services.AddSingleton<IDecisionEngineService, DecisionEngineService>();

// Register data integration services
builder.Services.AddSingleton<IDataSourceRegistry, InMemoryDataSourceRegistry>();
builder.Services.AddSingleton<InMemoryEventBus>();
builder.Services.AddSingleton<IEventBus>(sp => sp.GetRequiredService<InMemoryEventBus>());
builder.Services.AddSingleton<IEventPublisher>(sp => sp.GetRequiredService<InMemoryEventBus>());
builder.Services.AddSingleton<IDataAggregationService, DataAggregationService>();
builder.Services.AddSingleton<IFeatureStore, InMemoryFeatureStore>();

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

// Initialize sample data sources
await InitializeDataSourcesAsync(app.Services);

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new HealthCheckResponse
{ 
    Status = "healthy",
    Service = "Wekeza Enterprise Brain",
    Timestamp = DateTime.UtcNow
}))
.WithName("HealthCheck");

app.Run();

// Initialize sample data sources for demo
static async Task InitializeDataSourcesAsync(IServiceProvider services)
{
    var registry = services.GetRequiredService<IDataSourceRegistry>();
    
    // Register Core Banking system
    await registry.RegisterDataSourceAsync(new DataSourceConfiguration
    {
        Name = "Core Banking System",
        Type = DataSourceType.CoreBanking,
        ConnectionString = "Host=localhost;Database=CoreBanking;",
        IsEnabled = true,
        CreatedAt = DateTime.UtcNow,
        Metadata = new Dictionary<string, string>
        {
            ["Description"] = "Main core banking system (ComprehensiveWekezaApi)",
            ["Environment"] = "Production"
        }
    });
    
    // Register Mobile Banking
    await registry.RegisterDataSourceAsync(new DataSourceConfiguration
    {
        Name = "Mobile Banking",
        Type = DataSourceType.MobileBanking,
        ConnectionString = "Host=localhost;Database=MobileBanking;",
        IsEnabled = true,
        CreatedAt = DateTime.UtcNow,
        Metadata = new Dictionary<string, string>
        {
            ["Description"] = "Mobile banking channel",
            ["Platform"] = "iOS/Android"
        }
    });
    
    // Register Fraud Detection System
    await registry.RegisterDataSourceAsync(new DataSourceConfiguration
    {
        Name = "Fraud Detection System",
        Type = DataSourceType.FraudSystem,
        ConnectionString = "Host=localhost;Database=FraudDetection;",
        IsEnabled = true,
        CreatedAt = DateTime.UtcNow,
        Metadata = new Dictionary<string, string>
        {
            ["Description"] = "Real-time fraud detection and prevention",
            ["Version"] = "3.0"
        }
    });
}
