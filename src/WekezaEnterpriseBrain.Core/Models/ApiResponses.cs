namespace WekezaEnterpriseBrain.Core.Models;

/// <summary>
/// Health check response model
/// </summary>
public class HealthCheckResponse
{
    public string Status { get; set; } = string.Empty;
    public string Service { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Risk score response model
/// </summary>
public class RiskScoreResponse
{
    public Guid GlobalCustomerId { get; set; }
    public decimal RiskScore { get; set; }
    public string EventType { get; set; } = string.Empty;
}
