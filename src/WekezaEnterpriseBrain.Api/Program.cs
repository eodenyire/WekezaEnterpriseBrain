using WekezaEnterpriseBrain.Core.Interfaces;
using WekezaEnterpriseBrain.Core.Services;
using WekezaEnterpriseBrain.Core.Models;
using WekezaEnterpriseBrain.Core.DataSources;
using WekezaEnterpriseBrain.Core.Events;
using WekezaEnterpriseBrain.Core.Features;
using WekezaEnterpriseBrain.Infrastructure;
using WekezaEnterpriseBrain.Infrastructure.EventBus;
using WekezaEnterpriseBrain.Infrastructure.DataHub;
using WekezaEnterpriseBrain.Infrastructure.DataHub.Etl;
using Microsoft.EntityFrameworkCore;

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

// ============================================================================
// Wekeza Main Datahub - PostgreSQL Data Warehouse
// ============================================================================
var datahubConnStr = builder.Configuration.GetConnectionString("WekezaDataHub")
    ?? "Host=localhost;Port=5432;Database=wekeza_datahub;Username=wekeza_hub_user;Password=change_in_production";

builder.Services.AddDbContext<WekezaDataHubDbContext>(opts =>
    opts.UseNpgsql(datahubConnStr,
        npgsql => npgsql.MigrationsHistoryTable("__ef_migrations", "warehouse")));

// ETL Services - one per source system
var config = builder.Configuration;
builder.Services.AddScoped<IEtlService>(sp =>
{
    var hub    = sp.GetRequiredService<WekezaDataHubDbContext>();
    var logger = sp.GetRequiredService<ILogger<WekezaBankEtlService>>();
    var connStr = config["SourceSystems:WekezaBank:ConnectionString"]
        ?? "Host=localhost;Port=5432;Database=risk_management;Username=risk_user;Password=change_in_production";
    return new WekezaBankEtlService(hub, logger, connStr);
});

builder.Services.AddScoped<IEtlService>(sp =>
{
    var hub    = sp.GetRequiredService<WekezaDataHubDbContext>();
    var logger = sp.GetRequiredService<ILogger<WekezaOpenBankingEtlService>>();
    var connStr = config["SourceSystems:WekezaOpenBanking:ConnectionString"]
        ?? "Host=localhost;Port=5432;Database=wekeza_banking;Username=wekeza_user;Password=change_in_production";
    return new WekezaOpenBankingEtlService(hub, logger, connStr);
});

builder.Services.AddScoped<IEtlService>(sp =>
{
    var hub    = sp.GetRequiredService<WekezaDataHubDbContext>();
    var logger = sp.GetRequiredService<ILogger<WekezaCrmEtlService>>();
    var connStr = config["SourceSystems:WekezaCRM:ConnectionString"]
        ?? "Server=localhost;Database=WekezaCRM;User Id=sa;Password=change_in_production;TrustServerCertificate=true;";
    return new WekezaCrmEtlService(hub, logger, connStr);
});

builder.Services.AddScoped<IEtlService>(sp =>
{
    var hub    = sp.GetRequiredService<WekezaDataHubDbContext>();
    var logger = sp.GetRequiredService<ILogger<WekezaCoreEtlService>>();
    var connStr = config["SourceSystems:WekezaCore:ConnectionString"]
        ?? "Host=localhost;Port=5432;Database=wekeza_banking_comprehensive;Username=postgres;Password=change_in_production";
    return new WekezaCoreEtlService(hub, logger, connStr);
});

// DataHub Orchestrator
builder.Services.AddScoped<IDataHubOrchestrator, DataHubOrchestrator>();

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
        ConnectionString = "Host=localhost;Database=WekezaCoreDB;",
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
    
    // 11. Open Banking (WekezaOpenBanking)
    await registry.RegisterDataSourceAsync(new DataSourceConfiguration
    {
        Name = "Nexus (Open Banking)",
        Type = DataSourceType.OpenBanking,
        ConnectionString = "Host=localhost;Database=wekeza_banking;Username=wekeza_user;Port=5432;",
        IsEnabled = true,
        CreatedAt = DateTime.UtcNow,
        Metadata = new Dictionary<string, string>
        {
            ["Description"] = "Open banking platform (WekezaOpenBanking) - PostgreSQL wekeza_banking",
            ["APIVersion"] = "v2",
            ["GitHubRepo"] = "https://github.com/eodenyire/WekezaOpenBanking",
            ["KeyEntities"] = "oauth_clients, oauth_tokens, customers, accounts, transactions, payments, webhooks"
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
    
    // NEWLY DISCOVERED SYSTEMS FROM GITHUB SCAN (github.com/eodenyire)
    // These repositories were discovered by scanning https://github.com/eodenyire for repos starting with "Wekeza"
    
    // 16. WekezaCRM - Customer Relationship Management System
    await registry.RegisterDataSourceAsync(new DataSourceConfiguration
    {
        Name = "WekezaCRM",
        Type = DataSourceType.CRM,
        ConnectionString = "Server=(localdb)\\mssqllocaldb;Database=WekezaCRM;Trusted_Connection=True;",
        IsEnabled = true,
        CreatedAt = DateTime.UtcNow,
        Metadata = new Dictionary<string, string>
        {
            ["Description"] = "CRM system with AI/sentiment analysis, WhatsApp and USSD integration",
            ["DatabaseType"] = "SQL Server",
            ["DatabaseName"] = "WekezaCRM",
            ["DbContext"] = "CRMDbContext",
            ["GitHubRepo"] = "https://github.com/eodenyire/WekezaCRM",
            ["KeyEntities"] = "Customer, Account, Transaction, Case, Interaction, Campaign, NextBestAction, SentimentAnalysis"
        }
    });
    
    // 17. WekezaNextGenPersonalBanking - AI-Powered Personal Banking Channel
    await registry.RegisterDataSourceAsync(new DataSourceConfiguration
    {
        Name = "WekezaNextGenPersonalBanking",
        Type = DataSourceType.PersonalBanking,
        ConnectionString = "BaseUrl=http://localhost:5000;",
        IsEnabled = true,
        CreatedAt = DateTime.UtcNow,
        Metadata = new Dictionary<string, string>
        {
            ["Description"] = "AI-powered personal banking API channel aggregating CoreApi, ComprehensiveApi, and Mvp40Api",
            ["Architecture"] = "API Aggregation Layer",
            ["GitHubRepo"] = "https://github.com/eodenyire/WekezaNextGenPersonalBanking",
            ["BackendSystems"] = "ComprehensiveApi (5003), CoreApi (5000), Mvp40Api (5004)",
            ["AIFeatures"] = "Transaction categorization, cash flow prediction, financial health scoring"
        }
    });
    
    // 18. WekezaBank - Risk Management System (KYC/AML)
    await registry.RegisterDataSourceAsync(new DataSourceConfiguration
    {
        Name = "WekezaBank",
        Type = DataSourceType.RiskSystem,
        ConnectionString = "Host=localhost;Database=risk_management;Port=5432;",
        IsEnabled = true,
        CreatedAt = DateTime.UtcNow,
        Metadata = new Dictionary<string, string>
        {
            ["Description"] = "Risk management system with KYC/AML using Ballerine, CISO Assistant and Tazama integrations",
            ["DatabaseType"] = "PostgreSQL",
            ["DatabaseName"] = "risk_management",
            ["GitHubRepo"] = "https://github.com/eodenyire/WekezaBank",
            ["Integrations"] = "Ballerine (KYC/AML), CISO Assistant (Security), Tazama (Transaction Monitoring)"
        }
    });
    
    // 19. WekezaGlobal - Africa's Cross-Border Financial Rail (Placeholder)
    await registry.RegisterDataSourceAsync(new DataSourceConfiguration
    {
        Name = "WekezaGlobal",
        Type = DataSourceType.External,
        ConnectionString = "Host=localhost;Database=WekezaGlobal;",
        IsEnabled = false,
        CreatedAt = DateTime.UtcNow,
        Metadata = new Dictionary<string, string>
        {
            ["Description"] = "Africa's cross-border financial rail - pending implementation",
            ["GitHubRepo"] = "https://github.com/eodenyire/WekezaGlobal",
            ["Status"] = "Repository discovered, implementation pending"
        }
    });
    
    // 20. WekezaDFS - Digital Financial Services (Placeholder)
    await registry.RegisterDataSourceAsync(new DataSourceConfiguration
    {
        Name = "WekezaDFS",
        Type = DataSourceType.External,
        ConnectionString = "Host=localhost;Database=WekezaDFS;",
        IsEnabled = false,
        CreatedAt = DateTime.UtcNow,
        Metadata = new Dictionary<string, string>
        {
            ["Description"] = "Digital financial services - pending implementation",
            ["GitHubRepo"] = "https://github.com/eodenyire/WekezaDFS",
            ["Status"] = "Repository discovered, implementation pending"
        }
    });
    
    // 21. WekezaHela - (Placeholder)
    await registry.RegisterDataSourceAsync(new DataSourceConfiguration
    {
        Name = "WekezaHela",
        Type = DataSourceType.External,
        ConnectionString = "Host=localhost;Database=WekezaHela;",
        IsEnabled = false,
        CreatedAt = DateTime.UtcNow,
        Metadata = new Dictionary<string, string>
        {
            ["Description"] = "WekezaHela system - pending implementation",
            ["GitHubRepo"] = "https://github.com/eodenyire/WekezaHela",
            ["Status"] = "Repository discovered, implementation pending"
        }
    });
    
    // 22. WekezaPublicSectorBanking - Public Sector Banking (Placeholder)
    await registry.RegisterDataSourceAsync(new DataSourceConfiguration
    {
        Name = "WekezaPublicSectorBanking",
        Type = DataSourceType.External,
        ConnectionString = "Host=localhost;Database=WekezaPublicSectorBanking;",
        IsEnabled = false,
        CreatedAt = DateTime.UtcNow,
        Metadata = new Dictionary<string, string>
        {
            ["Description"] = "Public sector banking - pending implementation",
            ["GitHubRepo"] = "https://github.com/eodenyire/WekezaPublicSectorBanking",
            ["Status"] = "Repository discovered, implementation pending"
        }
    });
}
