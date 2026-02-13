namespace WekezaEnterpriseBrain.Core.Features;

/// <summary>
/// Pre-computed features for AI and decision making
/// </summary>
public class CustomerFeatures
{
    public Guid GlobalCustomerId { get; set; }
    
    // Transaction Features
    public decimal MonthlyAverageSpend { get; set; }
    public decimal MonthlyInflow { get; set; }
    public decimal MonthlyOutflow { get; set; }
    public int TransactionCount30Days { get; set; }
    public int TransactionCount90Days { get; set; }
    
    // Channel Behavior
    public int MobileLoginCount30Days { get; set; }
    public int WebLoginCount30Days { get; set; }
    public int USSDUsageCount30Days { get; set; }
    public string PreferredChannel { get; set; } = string.Empty;
    public DateTime? LastLoginDate { get; set; }
    
    // Risk Indicators
    public decimal CurrentRiskScore { get; set; }
    public int FailedLoginAttempts { get; set; }
    public int FraudAlertsCount { get; set; }
    public decimal VelocityScore { get; set; }
    
    // Financial Health
    public decimal AverageDailyBalance { get; set; }
    public decimal MinBalance30Days { get; set; }
    public decimal MaxBalance30Days { get; set; }
    public bool HasActiveLoan { get; set; }
    public decimal TotalLoanAmount { get; set; }
    
    // Behavioral Patterns
    public decimal[] HourlyTransactionPattern { get; set; } = new decimal[24];
    public int[] DayOfWeekPattern { get; set; } = new int[7];
    public bool IsHighValueCustomer { get; set; }
    public string CustomerSegment { get; set; } = "Retail";
    
    // Timestamps
    public DateTime CalculatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Interface for feature store operations
/// </summary>
public interface IFeatureStore
{
    /// <summary>
    /// Get features for a customer
    /// </summary>
    Task<CustomerFeatures?> GetFeaturesAsync(Guid globalCustomerId);
    
    /// <summary>
    /// Calculate and store features for a customer
    /// </summary>
    Task<CustomerFeatures> CalculateFeaturesAsync(Guid globalCustomerId);
    
    /// <summary>
    /// Refresh features for all customers
    /// </summary>
    Task RefreshAllFeaturesAsync();
    
    /// <summary>
    /// Get feature importance scores
    /// </summary>
    Task<Dictionary<string, double>> GetFeatureImportanceAsync();
}
