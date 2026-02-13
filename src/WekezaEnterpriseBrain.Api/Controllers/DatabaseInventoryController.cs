using Microsoft.AspNetCore.Mvc;

namespace WekezaEnterpriseBrain.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DatabaseInventoryController : ControllerBase
{
    private readonly ILogger<DatabaseInventoryController> _logger;

    public DatabaseInventoryController(ILogger<DatabaseInventoryController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<DatabaseInventoryResponse> GetInventory()
    {
        var inventory = new DatabaseInventoryResponse
        {
            TotalDatabasesDiscovered = 15,
            ConnectedDatabases = 3,
            ReadyToConnect = 12,
            IntegrationPercentage = 20,
            LastUpdated = DateTime.UtcNow,
            Databases = new List<DatabaseInfo>
            {
                // Core Banking Systems (5)
                new DatabaseInfo
                {
                    Id = 1,
                    SystemName = "ComprehensiveWekezaApi",
                    DatabaseName = "CoreBanking",
                    Category = "Core Banking",
                    Technology = "PostgreSQL",
                    Purpose = "Full-featured core banking operations",
                    IntegrationStatus = "Connected",
                    WriteFrequency = "Real-time",
                    ConnectorImplemented = true
                },
                new DatabaseInfo
                {
                    Id = 2,
                    SystemName = "DatabaseWekezaApi",
                    DatabaseName = "DatabaseWekezaApi_DB",
                    Category = "Core Banking",
                    Technology = "PostgreSQL",
                    Purpose = "Database-centric banking API",
                    IntegrationStatus = "Ready",
                    WriteFrequency = "Real-time",
                    ConnectorImplemented = false
                },
                new DatabaseInfo
                {
                    Id = 3,
                    SystemName = "EnhancedWekezaApi",
                    DatabaseName = "EnhancedBanking",
                    Category = "Core Banking",
                    Technology = "PostgreSQL",
                    Purpose = "Enhanced banking features",
                    IntegrationStatus = "Ready",
                    WriteFrequency = "Real-time",
                    ConnectorImplemented = false
                },
                new DatabaseInfo
                {
                    Id = 4,
                    SystemName = "MinimalWekezaApi",
                    DatabaseName = "MinimalBanking",
                    Category = "Core Banking",
                    Technology = "PostgreSQL",
                    Purpose = "Lightweight core banking",
                    IntegrationStatus = "Ready",
                    WriteFrequency = "Real-time",
                    ConnectorImplemented = false
                },
                new DatabaseInfo
                {
                    Id = 5,
                    SystemName = "Wekeza.Core.Api",
                    DatabaseName = "WekeazCore",
                    Category = "Core Banking",
                    Technology = "PostgreSQL",
                    Purpose = "Central core banking API",
                    IntegrationStatus = "Ready",
                    WriteFrequency = "Real-time",
                    ConnectorImplemented = false
                },
                
                // Channel Systems (3)
                new DatabaseInfo
                {
                    Id = 6,
                    SystemName = "Mobile Banking",
                    DatabaseName = "MobileBanking",
                    Category = "Channels",
                    Technology = "PostgreSQL",
                    Purpose = "Mobile app transactions and sessions",
                    IntegrationStatus = "Connected",
                    WriteFrequency = "Real-time (high volume)",
                    ConnectorImplemented = true
                },
                new DatabaseInfo
                {
                    Id = 7,
                    SystemName = "Web Banking",
                    DatabaseName = "WebBanking",
                    Category = "Channels",
                    Technology = "PostgreSQL",
                    Purpose = "Web portal sessions and transactions",
                    IntegrationStatus = "Ready",
                    WriteFrequency = "Real-time",
                    ConnectorImplemented = false
                },
                new DatabaseInfo
                {
                    Id = 8,
                    SystemName = "USSD Banking",
                    DatabaseName = "USSD_Banking",
                    Category = "Channels",
                    Technology = "PostgreSQL",
                    Purpose = "USSD session management",
                    IntegrationStatus = "Ready",
                    WriteFrequency = "Real-time (high volume)",
                    ConnectorImplemented = false
                },
                
                // Security & Risk (2)
                new DatabaseInfo
                {
                    Id = 9,
                    SystemName = "Fraud Detection",
                    DatabaseName = "FraudDetection",
                    Category = "Security & Risk",
                    Technology = "PostgreSQL",
                    Purpose = "Real-time fraud monitoring and alerts",
                    IntegrationStatus = "Connected",
                    WriteFrequency = "Real-time (continuous)",
                    ConnectorImplemented = true
                },
                new DatabaseInfo
                {
                    Id = 10,
                    SystemName = "ERMS",
                    DatabaseName = "RiskManagement",
                    Category = "Security & Risk",
                    Technology = "PostgreSQL",
                    Purpose = "Enterprise risk management",
                    IntegrationStatus = "Ready",
                    WriteFrequency = "Near real-time",
                    ConnectorImplemented = false
                },
                
                // Integration Systems (2)
                new DatabaseInfo
                {
                    Id = 11,
                    SystemName = "Nexus (Open Banking)",
                    DatabaseName = "OpenBanking",
                    Category = "Integration",
                    Technology = "PostgreSQL",
                    Purpose = "Third-party API access and consent",
                    IntegrationStatus = "Ready",
                    WriteFrequency = "Real-time",
                    ConnectorImplemented = false
                },
                new DatabaseInfo
                {
                    Id = 12,
                    SystemName = "AI Copilot",
                    DatabaseName = "AICopilot",
                    Category = "Integration",
                    Technology = "PostgreSQL",
                    Purpose = "AI model data and interactions",
                    IntegrationStatus = "Ready",
                    WriteFrequency = "Real-time",
                    ConnectorImplemented = false
                },
                
                // Analytics & Support (3)
                new DatabaseInfo
                {
                    Id = 13,
                    SystemName = "Analytics/BI",
                    DatabaseName = "BI_DataWarehouse",
                    Category = "Analytics",
                    Technology = "PostgreSQL",
                    Purpose = "Business intelligence and reporting",
                    IntegrationStatus = "Ready",
                    WriteFrequency = "Batch (hourly/nightly)",
                    ConnectorImplemented = false
                },
                new DatabaseInfo
                {
                    Id = 14,
                    SystemName = "Audit Logs",
                    DatabaseName = "AuditLogs",
                    Category = "Support",
                    Technology = "PostgreSQL",
                    Purpose = "Immutable audit trail",
                    IntegrationStatus = "Ready",
                    WriteFrequency = "Real-time (append-only)",
                    ConnectorImplemented = false
                },
                new DatabaseInfo
                {
                    Id = 15,
                    SystemName = "Reporting",
                    DatabaseName = "Reporting",
                    Category = "Analytics",
                    Technology = "PostgreSQL",
                    Purpose = "Operational reports",
                    IntegrationStatus = "Ready",
                    WriteFrequency = "Batch",
                    ConnectorImplemented = false
                }
            }
        };

        return Ok(inventory);
    }

    [HttpGet("summary")]
    public ActionResult<DatabaseSummary> GetSummary()
    {
        var summary = new DatabaseSummary
        {
            TotalDatabases = 15,
            ConnectedDatabases = 3,
            ReadyToConnect = 12,
            IntegrationPercentage = 20,
            DatabasesByCategory = new Dictionary<string, int>
            {
                ["Core Banking"] = 5,
                ["Channels"] = 3,
                ["Security & Risk"] = 2,
                ["Integration"] = 2,
                ["Analytics & Support"] = 3
            },
            DatabasesByTechnology = new Dictionary<string, int>
            {
                ["PostgreSQL"] = 15,
                ["Redis"] = 1,
                ["Cassandra (Future)"] = 0
            },
            DatabasesByStatus = new Dictionary<string, int>
            {
                ["Connected"] = 3,
                ["Ready"] = 12,
                ["Planned"] = 0
            }
        };

        return Ok(summary);
    }

    [HttpGet("categories")]
    public ActionResult<IEnumerable<string>> GetCategories()
    {
        var categories = new[]
        {
            "Core Banking",
            "Channels",
            "Security & Risk",
            "Integration",
            "Analytics & Support"
        };

        return Ok(categories);
    }
}

public class DatabaseInventoryResponse
{
    public int TotalDatabasesDiscovered { get; set; }
    public int ConnectedDatabases { get; set; }
    public int ReadyToConnect { get; set; }
    public int IntegrationPercentage { get; set; }
    public DateTime LastUpdated { get; set; }
    public List<DatabaseInfo> Databases { get; set; } = new();
}

public class DatabaseInfo
{
    public int Id { get; set; }
    public string SystemName { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Technology { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string IntegrationStatus { get; set; } = string.Empty;
    public string WriteFrequency { get; set; } = string.Empty;
    public bool ConnectorImplemented { get; set; }
}

public class DatabaseSummary
{
    public int TotalDatabases { get; set; }
    public int ConnectedDatabases { get; set; }
    public int ReadyToConnect { get; set; }
    public int IntegrationPercentage { get; set; }
    public Dictionary<string, int> DatabasesByCategory { get; set; } = new();
    public Dictionary<string, int> DatabasesByTechnology { get; set; } = new();
    public Dictionary<string, int> DatabasesByStatus { get; set; } = new();
}
