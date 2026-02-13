namespace WekezaEnterpriseBrain.Core.Models;

/// <summary>
/// Decision request for real-time decisioning
/// </summary>
public class DecisionRequest
{
    public Guid GlobalCustomerId { get; set; }
    public string EventType { get; set; } = string.Empty; // PAYMENT, LOAN_REQUEST, ACCOUNT_OPENING, LOGIN
    public decimal? Amount { get; set; }
    public string? Channel { get; set; }
    public Dictionary<string, object> Context { get; set; } = new();
}

/// <summary>
/// Decision response from the Enterprise Brain
/// </summary>
public class DecisionResponse
{
    public string Decision { get; set; } = string.Empty; // APPROVE, DECLINE, REVIEW, ESCALATE
    public decimal RiskScore { get; set; }
    public string Reason { get; set; } = string.Empty;
    public List<string> Flags { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime Timestamp { get; set; }
}
