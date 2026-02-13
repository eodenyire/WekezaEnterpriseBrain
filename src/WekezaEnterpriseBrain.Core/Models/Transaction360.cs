namespace WekezaEnterpriseBrain.Core.Models;

/// <summary>
/// Transaction 360 - Unified transaction view
/// </summary>
public class Transaction360
{
    public Guid Id { get; set; }
    public Guid Account360Id { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    
    // Transaction Details
    public DateTime TransactionDate { get; set; }
    public string TransactionType { get; set; } = string.Empty; // Debit, Credit, Transfer
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "KES";
    public decimal BalanceAfter { get; set; }
    
    // Transaction Context
    public string Channel { get; set; } = string.Empty; // Mobile, Web, ATM, Branch, USSD
    public string? Description { get; set; }
    public string? Reference { get; set; }
    public string? CounterpartyAccount { get; set; }
    public string? CounterpartyName { get; set; }
    
    // Risk Indicators
    public bool IsFlagged { get; set; }
    public decimal RiskScore { get; set; }
    public string? FraudIndicators { get; set; }
    
    // Source System
    public string SourceSystem { get; set; } = string.Empty;
    public string SourceTransactionId { get; set; } = string.Empty;
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    
    // Navigation
    public Account360? Account { get; set; }
}
