namespace WekezaEnterpriseBrain.Infrastructure.DataHub.Models;

/// <summary>Global Customer Dimension - unified identity across all Wekeza systems</summary>
public class DimCustomer
{
    public Guid Gcid { get; set; }
    public string? PrimaryEmail { get; set; }
    public string? PrimaryPhone { get; set; }
    public string? NationalId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string Country { get; set; } = "KE";

    // Cross-system local IDs
    public string? CoreBankingId { get; set; }
    public string? CrmId { get; set; }
    public string? OpenBankingId { get; set; }
    public string? RiskSystemId { get; set; }
    public string? PersonalBankingId { get; set; }

    // Aggregated intelligence
    public string OverallKycStatus { get; set; } = "unknown";
    public string OverallRiskLevel { get; set; } = "unknown";
    public decimal? CreditScore { get; set; }
    public decimal? LifetimeValue { get; set; }
    public string? CustomerSegment { get; set; }
    public string? FinancialPersonality { get; set; }
    public int? StressLevel { get; set; }
    public int? FinancialHealthScore { get; set; }
    public decimal IdentityConfidence { get; set; } = 1.0m;
    public int SourcesCount { get; set; } = 1;
    public DateTimeOffset? FirstSeenAt { get; set; }
    public DateTimeOffset? LastActivityAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public ICollection<DimAccount> Accounts { get; set; } = new List<DimAccount>();
    public ICollection<FactTransaction> Transactions { get; set; } = new List<FactTransaction>();
    public ICollection<FactRiskAssessment> RiskAssessments { get; set; } = new List<FactRiskAssessment>();
    public ICollection<FactInteraction> Interactions { get; set; } = new List<FactInteraction>();
    public ICollection<FactCase> Cases { get; set; } = new List<FactCase>();
    public ICollection<FactPayment> Payments { get; set; } = new List<FactPayment>();
    public CustomerFeatures? Features { get; set; }
}
