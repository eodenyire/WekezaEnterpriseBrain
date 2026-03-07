namespace WekezaEnterpriseBrain.Infrastructure.DataHub.Models;

/// <summary>Account Dimension - one row per bank account across all Wekeza systems</summary>
public class DimAccount
{
    public Guid Id { get; set; }
    public Guid Gcid { get; set; }
    public string SourceSystem { get; set; } = string.Empty;
    public string SourceAccountId { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string? AccountType { get; set; }
    public string Currency { get; set; } = "KES";
    public decimal? CurrentBalance { get; set; }
    public decimal? AvailableBalance { get; set; }
    public decimal OverdraftLimit { get; set; } = 0;
    public string? Status { get; set; }
    public string? ProductName { get; set; }
    public decimal? InterestRate { get; set; }
    public decimal? MinimumBalance { get; set; }
    public DateOnly? OpenedDate { get; set; }
    public DateOnly? ClosedDate { get; set; }
    public DateTimeOffset? LastTransactionAt { get; set; }
    public int TransactionCount { get; set; } = 0;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public DimCustomer? Customer { get; set; }
    public ICollection<FactTransaction> Transactions { get; set; } = new List<FactTransaction>();
    public ICollection<FactPayment> Payments { get; set; } = new List<FactPayment>();
}
