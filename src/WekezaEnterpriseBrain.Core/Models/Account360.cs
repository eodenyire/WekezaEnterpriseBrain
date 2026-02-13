namespace WekezaEnterpriseBrain.Core.Models;

/// <summary>
/// Account 360 - Unified view of customer accounts
/// </summary>
public class Account360
{
    public Guid Id { get; set; }
    public Guid Customer360Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty; // Savings, Current, Loan, Investment
    public string Currency { get; set; } = "KES";
    
    // Balance Information
    public decimal CurrentBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public decimal OverdraftLimit { get; set; }
    
    // Account Status
    public string Status { get; set; } = "Active"; // Active, Dormant, Frozen, Closed
    public DateTime OpenedDate { get; set; }
    public DateTime? ClosedDate { get; set; }
    
    // Source System
    public string SourceSystem { get; set; } = string.Empty;
    public string SourceAccountId { get; set; } = string.Empty;
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation
    public Customer360? Customer { get; set; }
    public ICollection<Transaction360> Transactions { get; set; } = new List<Transaction360>();
}
