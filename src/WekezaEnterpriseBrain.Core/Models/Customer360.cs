namespace WekezaEnterpriseBrain.Core.Models;

/// <summary>
/// Customer 360 - Unified view of customer across all systems
/// </summary>
public class Customer360
{
    public Guid Id { get; set; }
    public Guid GlobalCustomerId { get; set; }
    
    // Personal Information
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? NationalId { get; set; }
    
    // Contact Information
    public string? PrimaryPhone { get; set; }
    public string? SecondaryPhone { get; set; }
    public string? PrimaryEmail { get; set; }
    public string? SecondaryEmail { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    
    // Customer Status
    public string Status { get; set; } = "Active"; // Active, Inactive, Suspended, Closed
    public string CustomerType { get; set; } = "Individual"; // Individual, Corporate, SME
    public string Segment { get; set; } = "Retail"; // Retail, Premium, VIP, Corporate
    
    // Risk Profile
    public decimal RiskScore { get; set; }
    public string RiskCategory { get; set; } = "Low"; // Low, Medium, High
    
    // Behavioral Metrics
    public DateTime? LastLoginDate { get; set; }
    public string? LastLoginChannel { get; set; }
    public int LoginCount { get; set; }
    public int FailedLoginAttempts { get; set; }
    
    // Financial Summary
    public decimal TotalBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public int AccountCount { get; set; }
    public decimal MonthlyInflow { get; set; }
    public decimal MonthlyOutflow { get; set; }
    public decimal AverageDailyBalance { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation
    public GlobalCustomerId? GlobalCustomer { get; set; }
    public ICollection<Account360> Accounts { get; set; } = new List<Account360>();
}
