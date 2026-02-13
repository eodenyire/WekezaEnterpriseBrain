namespace WekezaEnterpriseBrain.Core.DataSources;

/// <summary>
/// Configuration for external data sources
/// </summary>
public class DataSourceConfiguration
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DataSourceType Type { get; set; }
    public string ConnectionString { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Types of data sources in the Wekeza ecosystem
/// </summary>
public enum DataSourceType
{
    CoreBanking,
    MobileBanking,
    WebBanking,
    USSD,
    FraudSystem,
    RiskSystem,
    OpenBanking,
    ERMS,
    AICopilot,
    Analytics,
    External
}

/// <summary>
/// Result of data source connection test
/// </summary>
public class DataSourceConnectionResult
{
    public bool IsConnected { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime TestedAt { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}
