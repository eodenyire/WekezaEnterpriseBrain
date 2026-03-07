namespace WekezaEnterpriseBrain.Infrastructure.DataHub.Models;

/// <summary>Transaction Fact - every transaction from every Wekeza system</summary>
public class FactTransaction
{
    public Guid Id { get; set; }
    public Guid Gcid { get; set; }
    public Guid? AccountId { get; set; }
    public int? DateKey { get; set; }
    public string SourceSystem { get; set; } = string.Empty;
    public string SourceTransactionId { get; set; } = string.Empty;
    public DateTimeOffset TransactionDate { get; set; }
    public string? TransactionType { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "KES";
    public string? Channel { get; set; }
    public string? Description { get; set; }
    public string? Reference { get; set; }
    public decimal? BalanceAfter { get; set; }
    public string? Status { get; set; }
    public decimal? RiskScore { get; set; }
    public string? AiCategory { get; set; }
    public string? MerchantName { get; set; }
    public string? MerchantCategory { get; set; }
    public string? Location { get; set; }
    public string? RelatedAccountNumber { get; set; }
    public DateTimeOffset IngestedAt { get; set; } = DateTimeOffset.UtcNow;

    public DimCustomer? Customer { get; set; }
    public DimAccount? Account { get; set; }
}

/// <summary>Payment Fact - open banking payments</summary>
public class FactPayment
{
    public Guid Id { get; set; }
    public Guid Gcid { get; set; }
    public Guid? AccountId { get; set; }
    public int? DateKey { get; set; }
    public string SourceSystem { get; set; } = "WekezaOpenBanking";
    public Guid SourcePaymentId { get; set; }
    public string PaymentRef { get; set; } = string.Empty;
    public string? DestinationAccountNumber { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "KES";
    public string? Status { get; set; }
    public decimal? RiskScore { get; set; }
    public string? OauthClientName { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset IngestedAt { get; set; } = DateTimeOffset.UtcNow;

    public DimCustomer? Customer { get; set; }
    public DimAccount? Account { get; set; }
}

/// <summary>Risk Assessment Fact - risk scores from WekezaBank</summary>
public class FactRiskAssessment
{
    public Guid Id { get; set; }
    public Guid Gcid { get; set; }
    public int? DateKey { get; set; }
    public string SourceSystem { get; set; } = "WekezaBank";
    public int? SourceCaseId { get; set; }
    public string SourceTransactionId { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public string Currency { get; set; } = "KES";
    public decimal RiskScore { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public string? MetricType { get; set; }
    public string[]? FlaggedReasons { get; set; }
    public string? Outcome { get; set; }
    public bool AnalystReviewed { get; set; } = false;
    public decimal? TazamaFraudScore { get; set; }
    public string[]? TazamaTypologies { get; set; }
    public string? TazamaRecommendation { get; set; }
    public DateTimeOffset AssessedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public DateTimeOffset IngestedAt { get; set; } = DateTimeOffset.UtcNow;

    public DimCustomer? Customer { get; set; }
}

/// <summary>Interaction Fact - CRM interactions from WekezaCRM</summary>
public class FactInteraction
{
    public Guid Id { get; set; }
    public Guid Gcid { get; set; }
    public int? DateKey { get; set; }
    public string SourceSystem { get; set; } = "WekezaCRM";
    public string SourceInteractionId { get; set; } = string.Empty;
    public string? InteractionType { get; set; }
    public string? Channel { get; set; }
    public string? Subject { get; set; }
    public string? SentimentType { get; set; }
    public decimal? SentimentScore { get; set; }
    public string? KeyPhrases { get; set; }
    public int? DurationMinutes { get; set; }
    public string? CaseNumber { get; set; }
    public bool? Resolved { get; set; }
    public DateTimeOffset InteractionDate { get; set; }
    public DateTimeOffset IngestedAt { get; set; } = DateTimeOffset.UtcNow;

    public DimCustomer? Customer { get; set; }
}

/// <summary>Case Fact - support cases from WekezaCRM</summary>
public class FactCase
{
    public Guid Id { get; set; }
    public Guid Gcid { get; set; }
    public int? DateKey { get; set; }
    public string SourceSystem { get; set; } = "WekezaCRM";
    public string SourceCaseId { get; set; } = string.Empty;
    public string? CaseNumber { get; set; }
    public string? Title { get; set; }
    public string? Category { get; set; }
    public string? SubCategory { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public int? SlaHours { get; set; }
    public decimal? ResolutionHours { get; set; }
    public bool? IsSlaBreached { get; set; }
    public DateTimeOffset OpenedAt { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public DateTimeOffset IngestedAt { get; set; } = DateTimeOffset.UtcNow;

    public DimCustomer? Customer { get; set; }
}

/// <summary>Customer Feature Store - pre-computed ML features for each customer</summary>
public class CustomerFeatures
{
    public Guid Gcid { get; set; }
    public DateTimeOffset ComputedAt { get; set; } = DateTimeOffset.UtcNow;

    // Transaction velocity
    public int TxnCount7d { get; set; }
    public int TxnCount30d { get; set; }
    public int TxnCount90d { get; set; }
    public decimal TotalSpend30d { get; set; }
    public decimal TotalIncome30d { get; set; }
    public decimal AvgTxnAmount30d { get; set; }
    public decimal MaxSingleTxn30d { get; set; }
    public decimal NetCashflow30d { get; set; }

    // Channel behaviour
    public decimal MobileTxnPct { get; set; }
    public decimal WebTxnPct { get; set; }
    public decimal UssdTxnPct { get; set; }
    public decimal AtmTxnPct { get; set; }
    public decimal BranchTxnPct { get; set; }
    public decimal ApiTxnPct { get; set; }

    // Risk
    public decimal AvgRiskScore30d { get; set; }
    public int HighRiskTxnCount30d { get; set; }
    public int FraudFlags90d { get; set; }
    public string? RiskLevelCurrent { get; set; }

    // Account
    public int AccountCount { get; set; }
    public decimal TotalBalance { get; set; }
    public decimal MaxAccountBalance { get; set; }
    public int? MonthsSinceAccountOpened { get; set; }

    // CRM
    public int OpenCasesCount { get; set; }
    public decimal? AvgSentimentScore90d { get; set; }
    public int InteractionCount90d { get; set; }
    public decimal? CaseResolutionRate { get; set; }

    // Open Banking
    public int ObPaymentCount30d { get; set; }
    public decimal ObPaymentAmount30d { get; set; }
    public decimal ObFailedPaymentPct30d { get; set; }

    // AI (WekezaNextGen)
    public int? FinancialHealthScore { get; set; }
    public int? StressLevel { get; set; }
    public string? FinancialPersonality { get; set; }

    // Time-based
    public int? DaysSinceLastTxn { get; set; }
    public int? DaysSinceLastLogin { get; set; }
    public decimal? AvgTxnHourOfDay { get; set; }
    public decimal WeekendTxnPct { get; set; }

    public string FeatureVersion { get; set; } = "v1.0";

    public DimCustomer? Customer { get; set; }
}
