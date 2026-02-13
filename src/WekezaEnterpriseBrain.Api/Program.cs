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
    
    // CORE BANKING SYSTEMS (5)
    
    // 1. Core Banking System (ComprehensiveWekezaApi)
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
    
    // 2. DatabaseWekezaApi
    await registry.RegisterDataSourceAsync(new DataSourceConfiguration
    {
        Name = "DatabaseWekezaApi",
        Type = DataSourceType.CoreBanking,
        ConnectionString = "Host=localhost;Database=DatabaseWekezaApi_DB;",
        IsEnabled = true,
        CreatedAt = DateTime.UtcNow,
        Metadata = new Dictionary<string, string>
        {
            ["Description"] = "Database-centric banking API",
            ["Environment"] = "Production"
        }
    });
    
    // 3. EnhancedWekezaApi
    await registry.RegisterDataSourceAsync(new DataSourceConfiguration
    {
        Name = "EnhancedWekezaApi",
        Type = DataSourceType.CoreBanking,
        ConnectionString = "Host=localhost;Database=EnhancedBanking;",
        IsEnabled = true,
        CreatedAt = DateTime.UtcNow,
        Metadata = new Dictionary<string, string>
        {
            ["Description"] = "Enhanced banking features",
            ["Environment"] = "Production"
        }
    });
    
    // 4. MinimalWekezaApi
    await registry.RegisterDataSourceAsync(new DataSourceConfiguration
    {
        Name = "MinimalWekezaApi",
        Type = DataSourceType.CoreBanking,
        ConnectionString = "Host=localhost;Database=MinimalBanking;",
        IsEnabled = true,
        CreatedAt = DateTime.UtcNow,
        Metadata = new Dictionary<string, string>
        {
            ["Description"] = "Lightweight core banking",
            ["Environment"] = "Production"
        }
    });
    
    // 5. Wekeza.Core.Api
    await registry.RegisterDataSourceAsync(new DataSourceConfiguration
    {
        Name = "Wekeza.Core.Api",
        Type = DataSourceType.CoreBanking,
        ConnectionString = "Host=localhost;Database=WekeazCore;",
        IsEnabled = true,
        CreatedAt = DateTime.UtcNow,
        Metadata = new Dictionary<string, string>
        {
            ["Description"] = "Central core banking API",
            ["Environment"] = "Production"
        }
    });
    
    // CHANNEL SYSTEMS (3)
    
    // 6. Mobile Banking
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
    
    // 7. Web Banking
    await registry.RegisterDataSourceAsync(new DataSourceConfiguration
    {
        Name = "Web Banking",
        Type = DataSourceType.WebBanking,
        ConnectionString = "Host=localhost;Database=WebBanking;",
        IsEnabled = true,
        CreatedAt = DateTime.UtcNow,
        Metadata = new Dictionary<string, string>
        {
            ["Description"] = "Web banking portal",
            ["Platform"] = "Web"
        }
    });
    
    // 8. USSD Banking
    await registry.RegisterDataSourceAsync(new DataSourceConfiguration
    {
        Name = "USSD Banking",
        Type = DataSourceType.USSD,
        ConnectionString = "Host=localhost;Database=USSD_Banking;",
        IsEnabled = true,
        CreatedAt = DateTime.UtcNow,
        Metadata = new Dictionary<string, string>
        {
            ["Description"] = "USSD banking channel",
            ["Platform"] = "USSD Gateway"
        }
    });
    
    // SECURITY & RISK SYSTEMS (2)
    
    // 9. Fraud Detection System
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
    
    // 10. ERMS (Enterprise Risk Management System)
    await registry.RegisterDataSourceAsync(new DataSourceConfiguration
    {
        Name = "ERMS",
        Type = DataSourceType.ERMS,
        ConnectionString = "Host=localhost;Database=RiskManagement;",
        IsEnabled = true,
        CreatedAt = DateTime.UtcNow,
        Metadata = new Dictionary<string, string>
        {
            ["Description"] = "Enterprise risk management",
            ["Version"] = "2.0"
        }
    });
    
    // INTEGRATION SYSTEMS (2)
    
    // 11. Open Banking (Nexus)
    await registry.RegisterDataSourceAsync(new DataSourceConfiguration
    {
        Name = "Nexus (Open Banking)",
        Type = DataSourceType.OpenBanking,
        ConnectionString = "Host=localhost;Database=OpenBanking;",
        IsEnabled = true,
        CreatedAt = DateTime.UtcNow,
        Metadata = new Dictionary<string, string>
        {
            ["Description"] = "Open banking platform",
            ["APIVersion"] = "v2"
        }
    });
    
    // 12. AI Copilot
    await registry.RegisterDataSourceAsync(new DataSourceConfiguration
    {
        Name = "AI Copilot",
        Type = DataSourceType.AICopilot,
        ConnectionString = "Host=localhost;Database=AICopilot;",
        IsEnabled = true,
        CreatedAt = DateTime.UtcNow,
        Metadata = new Dictionary<string, string>
        {
            ["Description"] = "AI financial copilot",
            ["Model"] = "GPT-4"
        }
    });
    
    // ANALYTICS & SUPPORT SYSTEMS (3)
    
    // 13. Analytics/BI
    await registry.RegisterDataSourceAsync(new DataSourceConfiguration
    {
        Name = "Analytics/BI",
        Type = DataSourceType.Analytics,
        ConnectionString = "Host=localhost;Database=BI_DataWarehouse;",
        IsEnabled = true,
        CreatedAt = DateTime.UtcNow,
        Metadata = new Dictionary<string, string>
        {
            ["Description"] = "Business intelligence and analytics",
            ["DataWarehouse"] = "PostgreSQL"
        }
    });
    
    // 14. Audit Logs
    await registry.RegisterDataSourceAsync(new DataSourceConfiguration
    {
        Name = "Audit Logs",
        Type = DataSourceType.External,
        ConnectionString = "Host=localhost;Database=AuditLogs;",
        IsEnabled = true,
        CreatedAt = DateTime.UtcNow,
        Metadata = new Dictionary<string, string>
        {
            ["Description"] = "Audit and compliance logs",
            ["Retention"] = "7 years"
        }
    });
    
    // 15. Reporting
    await registry.RegisterDataSourceAsync(new DataSourceConfiguration
    {
        Name = "Reporting",
        Type = DataSourceType.External,
        ConnectionString = "Host=localhost;Database=Reporting;",
        IsEnabled = true,
        CreatedAt = DateTime.UtcNow,
        Metadata = new Dictionary<string, string>
        {
            ["Description"] = "Operational reporting",
            ["ReportCount"] = "500"
        }
    });
}
