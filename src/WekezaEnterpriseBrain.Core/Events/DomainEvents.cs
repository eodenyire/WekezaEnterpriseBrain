namespace WekezaEnterpriseBrain.Core.Events;

/// <summary>
/// Base class for all domain events
/// </summary>
public abstract class DomainEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string EventType { get; set; } = string.Empty;
    public string SourceSystem { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Event raised when a customer is created or updated
/// </summary>
public class CustomerEvent : DomainEvent
{
    public Guid GlobalCustomerId { get; set; }
    public string LocalCustomerId { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // Created, Updated, Deleted
}

/// <summary>
/// Event raised when an account is created or updated
/// </summary>
public class AccountEvent : DomainEvent
{
    public Guid GlobalCustomerId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public decimal CurrentBalance { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // Created, Updated, Closed
}

/// <summary>
/// Event raised when a transaction occurs
/// </summary>
public class TransactionEvent : DomainEvent
{
    public Guid GlobalCustomerId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "KES";
    public string TransactionType { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public decimal BalanceAfter { get; set; }
}

/// <summary>
/// Event raised for login activities
/// </summary>
public class LoginEvent : DomainEvent
{
    public Guid GlobalCustomerId { get; set; }
    public string Channel { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
    public string? Location { get; set; }
}

/// <summary>
/// Event raised for fraud alerts
/// </summary>
public class FraudAlertEvent : DomainEvent
{
    public Guid GlobalCustomerId { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public decimal RiskScore { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? RelatedTransactionId { get; set; }
    public string Severity { get; set; } = "Medium"; // Low, Medium, High, Critical
}

/// <summary>
/// Event raised when risk assessment is performed
/// </summary>
public class RiskAssessmentEvent : DomainEvent
{
    public Guid GlobalCustomerId { get; set; }
    public decimal RiskScore { get; set; }
    public string RiskCategory { get; set; } = string.Empty;
    public string AssessmentType { get; set; } = string.Empty;
    public Dictionary<string, decimal> RiskFactors { get; set; } = new();
}
