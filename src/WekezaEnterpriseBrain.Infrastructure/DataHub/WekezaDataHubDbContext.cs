using Microsoft.EntityFrameworkCore;
using WekezaEnterpriseBrain.Infrastructure.DataHub.Models;

namespace WekezaEnterpriseBrain.Infrastructure.DataHub;

/// <summary>
/// EF Core DbContext for the Wekeza Main Datahub (PostgreSQL data warehouse).
/// Organises all dimension tables, fact tables, and feature store into schemas:
///   - warehouse: dim_customers, dim_accounts
///   - warehouse: fact_transactions, fact_payments, fact_risk_assessments, fact_interactions, fact_cases
///   - analytics: customer_features
/// </summary>
public class WekezaDataHubDbContext : DbContext
{
    public WekezaDataHubDbContext(DbContextOptions<WekezaDataHubDbContext> options)
        : base(options) { }

    // Dimension tables
    public DbSet<DimCustomer> DimCustomers { get; set; }
    public DbSet<DimAccount> DimAccounts { get; set; }

    // Fact tables
    public DbSet<FactTransaction> FactTransactions { get; set; }
    public DbSet<FactPayment> FactPayments { get; set; }
    public DbSet<FactRiskAssessment> FactRiskAssessments { get; set; }
    public DbSet<FactInteraction> FactInteractions { get; set; }
    public DbSet<FactCase> FactCases { get; set; }

    // Feature Store
    public DbSet<CustomerFeatures> CustomerFeatures { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- DimCustomer ---
        modelBuilder.Entity<DimCustomer>(e =>
        {
            e.ToTable("dim_customers", "warehouse");
            e.HasKey(x => x.Gcid);
            e.Property(x => x.Gcid).HasColumnName("gcid").HasDefaultValueSql("uuid_generate_v4()");
            e.Property(x => x.PrimaryEmail).HasColumnName("primary_email").HasMaxLength(255);
            e.Property(x => x.PrimaryPhone).HasColumnName("primary_phone").HasMaxLength(30);
            e.Property(x => x.NationalId).HasColumnName("national_id").HasMaxLength(50);
            e.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(100);
            e.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(100);
            e.Property(x => x.DateOfBirth).HasColumnName("date_of_birth");
            e.Property(x => x.Gender).HasColumnName("gender").HasMaxLength(20);
            e.Property(x => x.Address).HasColumnName("address");
            e.Property(x => x.City).HasColumnName("city").HasMaxLength(100);
            e.Property(x => x.Country).HasColumnName("country").HasMaxLength(10).HasDefaultValue("KE");
            e.Property(x => x.CoreBankingId).HasColumnName("core_banking_id").HasMaxLength(100);
            e.Property(x => x.CrmId).HasColumnName("crm_id").HasMaxLength(100);
            e.Property(x => x.OpenBankingId).HasColumnName("open_banking_id").HasMaxLength(100);
            e.Property(x => x.RiskSystemId).HasColumnName("risk_system_id").HasMaxLength(100);
            e.Property(x => x.PersonalBankingId).HasColumnName("personal_banking_id").HasMaxLength(100);
            e.Property(x => x.OverallKycStatus).HasColumnName("overall_kyc_status").HasMaxLength(30).HasDefaultValue("unknown");
            e.Property(x => x.OverallRiskLevel).HasColumnName("overall_risk_level").HasMaxLength(20).HasDefaultValue("unknown");
            e.Property(x => x.CreditScore).HasColumnName("credit_score").HasPrecision(5, 2);
            e.Property(x => x.LifetimeValue).HasColumnName("lifetime_value").HasPrecision(18, 2);
            e.Property(x => x.CustomerSegment).HasColumnName("customer_segment").HasMaxLength(50);
            e.Property(x => x.FinancialPersonality).HasColumnName("financial_personality").HasMaxLength(50);
            e.Property(x => x.StressLevel).HasColumnName("stress_level");
            e.Property(x => x.FinancialHealthScore).HasColumnName("financial_health_score");
            e.Property(x => x.IdentityConfidence).HasColumnName("identity_confidence").HasPrecision(5, 4).HasDefaultValue(1.0m);
            e.Property(x => x.SourcesCount).HasColumnName("sources_count").HasDefaultValue(1);
            e.Property(x => x.FirstSeenAt).HasColumnName("first_seen_at");
            e.Property(x => x.LastActivityAt).HasColumnName("last_activity_at");
            e.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            e.HasIndex(x => x.PrimaryEmail).IsUnique().HasFilter("primary_email IS NOT NULL");
            e.HasIndex(x => x.PrimaryPhone);
            e.HasIndex(x => x.NationalId);
            e.HasIndex(x => x.OverallRiskLevel);
            e.HasIndex(x => x.CustomerSegment);
            e.HasMany(x => x.Accounts).WithOne(x => x.Customer).HasForeignKey(x => x.Gcid);
            e.HasMany(x => x.Transactions).WithOne(x => x.Customer).HasForeignKey(x => x.Gcid);
            e.HasMany(x => x.RiskAssessments).WithOne(x => x.Customer).HasForeignKey(x => x.Gcid);
            e.HasMany(x => x.Interactions).WithOne(x => x.Customer).HasForeignKey(x => x.Gcid);
            e.HasMany(x => x.Cases).WithOne(x => x.Customer).HasForeignKey(x => x.Gcid);
            e.HasMany(x => x.Payments).WithOne(x => x.Customer).HasForeignKey(x => x.Gcid);
            e.HasOne(x => x.Features).WithOne(x => x.Customer).HasForeignKey<CustomerFeatures>(x => x.Gcid);
        });

        // --- DimAccount ---
        modelBuilder.Entity<DimAccount>(e =>
        {
            e.ToTable("dim_accounts", "warehouse");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            e.Property(x => x.Gcid).HasColumnName("gcid");
            e.Property(x => x.SourceSystem).HasColumnName("source_system").HasMaxLength(50).IsRequired();
            e.Property(x => x.SourceAccountId).HasColumnName("source_account_id").HasMaxLength(100).IsRequired();
            e.Property(x => x.AccountNumber).HasColumnName("account_number").HasMaxLength(60).IsRequired();
            e.Property(x => x.AccountType).HasColumnName("account_type").HasMaxLength(50);
            e.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(5).HasDefaultValue("KES");
            e.Property(x => x.CurrentBalance).HasColumnName("current_balance").HasPrecision(18, 2);
            e.Property(x => x.AvailableBalance).HasColumnName("available_balance").HasPrecision(18, 2);
            e.Property(x => x.OverdraftLimit).HasColumnName("overdraft_limit").HasPrecision(18, 2).HasDefaultValue(0m);
            e.Property(x => x.Status).HasColumnName("status").HasMaxLength(30);
            e.Property(x => x.ProductName).HasColumnName("product_name").HasMaxLength(100);
            e.Property(x => x.InterestRate).HasColumnName("interest_rate").HasPrecision(6, 4);
            e.Property(x => x.MinimumBalance).HasColumnName("minimum_balance").HasPrecision(18, 2);
            e.Property(x => x.OpenedDate).HasColumnName("opened_date");
            e.Property(x => x.ClosedDate).HasColumnName("closed_date");
            e.Property(x => x.LastTransactionAt).HasColumnName("last_transaction_at");
            e.Property(x => x.TransactionCount).HasColumnName("transaction_count").HasDefaultValue(0);
            e.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
            e.HasIndex(x => new { x.SourceSystem, x.SourceAccountId }).IsUnique();
            e.HasIndex(x => x.Gcid);
            e.HasIndex(x => x.AccountNumber);
            e.HasMany(x => x.Transactions).WithOne(x => x.Account).HasForeignKey(x => x.AccountId);
            e.HasMany(x => x.Payments).WithOne(x => x.Account).HasForeignKey(x => x.AccountId);
        });

        // --- FactTransaction ---
        modelBuilder.Entity<FactTransaction>(e =>
        {
            e.ToTable("fact_transactions", "warehouse");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            e.Property(x => x.Gcid).HasColumnName("gcid");
            e.Property(x => x.AccountId).HasColumnName("account_id");
            e.Property(x => x.DateKey).HasColumnName("date_key");
            e.Property(x => x.SourceSystem).HasColumnName("source_system").HasMaxLength(50).IsRequired();
            e.Property(x => x.SourceTransactionId).HasColumnName("source_transaction_id").HasMaxLength(100).IsRequired();
            e.Property(x => x.TransactionDate).HasColumnName("transaction_date").IsRequired();
            e.Property(x => x.TransactionType).HasColumnName("transaction_type").HasMaxLength(50);
            e.Property(x => x.Amount).HasColumnName("amount").HasPrecision(18, 2).IsRequired();
            e.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(5).HasDefaultValue("KES");
            e.Property(x => x.Channel).HasColumnName("channel").HasMaxLength(50);
            e.Property(x => x.Description).HasColumnName("description");
            e.Property(x => x.Reference).HasColumnName("reference").HasMaxLength(150);
            e.Property(x => x.BalanceAfter).HasColumnName("balance_after").HasPrecision(18, 2);
            e.Property(x => x.Status).HasColumnName("status").HasMaxLength(30);
            e.Property(x => x.RiskScore).HasColumnName("risk_score").HasPrecision(5, 4);
            e.Property(x => x.AiCategory).HasColumnName("ai_category").HasMaxLength(100);
            e.Property(x => x.MerchantName).HasColumnName("merchant_name").HasMaxLength(255);
            e.Property(x => x.MerchantCategory).HasColumnName("merchant_category").HasMaxLength(100);
            e.Property(x => x.Location).HasColumnName("location").HasMaxLength(150);
            e.Property(x => x.RelatedAccountNumber).HasColumnName("related_account_number").HasMaxLength(60);
            e.Property(x => x.IngestedAt).HasColumnName("ingested_at").HasDefaultValueSql("NOW()");
            e.HasIndex(x => new { x.SourceSystem, x.SourceTransactionId }).IsUnique();
            e.HasIndex(x => x.Gcid);
            e.HasIndex(x => x.TransactionDate);
            e.HasIndex(x => x.AccountId);
        });

        // --- FactPayment ---
        modelBuilder.Entity<FactPayment>(e =>
        {
            e.ToTable("fact_payments", "warehouse");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            e.Property(x => x.Gcid).HasColumnName("gcid");
            e.Property(x => x.AccountId).HasColumnName("account_id");
            e.Property(x => x.DateKey).HasColumnName("date_key");
            e.Property(x => x.SourceSystem).HasColumnName("source_system").HasMaxLength(50).HasDefaultValue("WekezaOpenBanking");
            e.Property(x => x.SourcePaymentId).HasColumnName("source_payment_id");
            e.Property(x => x.PaymentRef).HasColumnName("payment_ref").HasMaxLength(100).IsRequired();
            e.Property(x => x.DestinationAccountNumber).HasColumnName("destination_account_number").HasMaxLength(60);
            e.Property(x => x.Amount).HasColumnName("amount").HasPrecision(18, 2).IsRequired();
            e.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(5).HasDefaultValue("KES");
            e.Property(x => x.Status).HasColumnName("status").HasMaxLength(20);
            e.Property(x => x.RiskScore).HasColumnName("risk_score").HasPrecision(5, 4);
            e.Property(x => x.OauthClientName).HasColumnName("oauth_client_name").HasMaxLength(255);
            e.Property(x => x.CompletedAt).HasColumnName("completed_at");
            e.Property(x => x.IngestedAt).HasColumnName("ingested_at").HasDefaultValueSql("NOW()");
            e.HasIndex(x => new { x.SourceSystem, x.SourcePaymentId }).IsUnique();
            e.HasIndex(x => x.Gcid);
        });

        // --- FactRiskAssessment ---
        modelBuilder.Entity<FactRiskAssessment>(e =>
        {
            e.ToTable("fact_risk_assessments", "warehouse");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            e.Property(x => x.Gcid).HasColumnName("gcid");
            e.Property(x => x.DateKey).HasColumnName("date_key");
            e.Property(x => x.SourceSystem).HasColumnName("source_system").HasMaxLength(50).HasDefaultValue("WekezaBank");
            e.Property(x => x.SourceCaseId).HasColumnName("source_case_id");
            e.Property(x => x.SourceTransactionId).HasColumnName("source_transaction_id").HasMaxLength(100).IsRequired();
            e.Property(x => x.Amount).HasColumnName("amount").HasPrecision(18, 2);
            e.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(5).HasDefaultValue("KES");
            e.Property(x => x.RiskScore).HasColumnName("risk_score").HasPrecision(5, 4).IsRequired();
            e.Property(x => x.RiskLevel).HasColumnName("risk_level").HasMaxLength(20).IsRequired();
            e.Property(x => x.MetricType).HasColumnName("metric_type").HasMaxLength(50);
            e.Property(x => x.FlaggedReasons).HasColumnName("flagged_reasons");
            e.Property(x => x.Outcome).HasColumnName("outcome").HasMaxLength(50);
            e.Property(x => x.AnalystReviewed).HasColumnName("analyst_reviewed").HasDefaultValue(false);
            e.Property(x => x.TazamaFraudScore).HasColumnName("tazama_fraud_score").HasPrecision(5, 4);
            e.Property(x => x.TazamaTypologies).HasColumnName("tazama_typologies");
            e.Property(x => x.TazamaRecommendation).HasColumnName("tazama_recommendation").HasMaxLength(50);
            e.Property(x => x.AssessedAt).HasColumnName("assessed_at").IsRequired();
            e.Property(x => x.ClosedAt).HasColumnName("closed_at");
            e.Property(x => x.IngestedAt).HasColumnName("ingested_at").HasDefaultValueSql("NOW()");
            e.HasIndex(x => new { x.SourceSystem, x.SourceTransactionId }).IsUnique();
            e.HasIndex(x => x.Gcid);
            e.HasIndex(x => x.RiskLevel);
            e.HasIndex(x => x.AssessedAt);
        });

        // --- FactInteraction ---
        modelBuilder.Entity<FactInteraction>(e =>
        {
            e.ToTable("fact_interactions", "warehouse");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            e.Property(x => x.Gcid).HasColumnName("gcid");
            e.Property(x => x.DateKey).HasColumnName("date_key");
            e.Property(x => x.SourceSystem).HasColumnName("source_system").HasMaxLength(50).HasDefaultValue("WekezaCRM");
            e.Property(x => x.SourceInteractionId).HasColumnName("source_interaction_id").HasMaxLength(100).IsRequired();
            e.Property(x => x.InteractionType).HasColumnName("interaction_type").HasMaxLength(50);
            e.Property(x => x.Channel).HasColumnName("channel").HasMaxLength(50);
            e.Property(x => x.Subject).HasColumnName("subject").HasMaxLength(255);
            e.Property(x => x.SentimentType).HasColumnName("sentiment_type").HasMaxLength(30);
            e.Property(x => x.SentimentScore).HasColumnName("sentiment_score").HasPrecision(5, 4);
            e.Property(x => x.KeyPhrases).HasColumnName("key_phrases");
            e.Property(x => x.DurationMinutes).HasColumnName("duration_minutes");
            e.Property(x => x.CaseNumber).HasColumnName("case_number").HasMaxLength(50);
            e.Property(x => x.Resolved).HasColumnName("resolved");
            e.Property(x => x.InteractionDate).HasColumnName("interaction_date").IsRequired();
            e.Property(x => x.IngestedAt).HasColumnName("ingested_at").HasDefaultValueSql("NOW()");
            e.HasIndex(x => new { x.SourceSystem, x.SourceInteractionId }).IsUnique();
            e.HasIndex(x => x.Gcid);
            e.HasIndex(x => x.InteractionDate);
        });

        // --- FactCase ---
        modelBuilder.Entity<FactCase>(e =>
        {
            e.ToTable("fact_cases", "warehouse");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            e.Property(x => x.Gcid).HasColumnName("gcid");
            e.Property(x => x.DateKey).HasColumnName("date_key");
            e.Property(x => x.SourceSystem).HasColumnName("source_system").HasMaxLength(50).HasDefaultValue("WekezaCRM");
            e.Property(x => x.SourceCaseId).HasColumnName("source_case_id").HasMaxLength(100).IsRequired();
            e.Property(x => x.CaseNumber).HasColumnName("case_number").HasMaxLength(50);
            e.Property(x => x.Title).HasColumnName("title").HasMaxLength(255);
            e.Property(x => x.Category).HasColumnName("category").HasMaxLength(100);
            e.Property(x => x.SubCategory).HasColumnName("sub_category").HasMaxLength(100);
            e.Property(x => x.Status).HasColumnName("status").HasMaxLength(30);
            e.Property(x => x.Priority).HasColumnName("priority").HasMaxLength(20);
            e.Property(x => x.SlaHours).HasColumnName("sla_hours");
            e.Property(x => x.ResolutionHours).HasColumnName("resolution_hours").HasPrecision(8, 2);
            e.Property(x => x.IsSlaBreached).HasColumnName("is_sla_breached");
            e.Property(x => x.OpenedAt).HasColumnName("opened_at").IsRequired();
            e.Property(x => x.ResolvedAt).HasColumnName("resolved_at");
            e.Property(x => x.ClosedAt).HasColumnName("closed_at");
            e.Property(x => x.IngestedAt).HasColumnName("ingested_at").HasDefaultValueSql("NOW()");
            e.HasIndex(x => new { x.SourceSystem, x.SourceCaseId }).IsUnique();
            e.HasIndex(x => x.Gcid);
            e.HasIndex(x => x.Status);
        });

        // --- CustomerFeatures ---
        modelBuilder.Entity<CustomerFeatures>(e =>
        {
            e.ToTable("customer_features", "analytics");
            e.HasKey(x => x.Gcid);
            e.Property(x => x.Gcid).HasColumnName("gcid");
            e.Property(x => x.ComputedAt).HasColumnName("computed_at").HasDefaultValueSql("NOW()");
            e.Property(x => x.TxnCount7d).HasColumnName("txn_count_7d").HasDefaultValue(0);
            e.Property(x => x.TxnCount30d).HasColumnName("txn_count_30d").HasDefaultValue(0);
            e.Property(x => x.TxnCount90d).HasColumnName("txn_count_90d").HasDefaultValue(0);
            e.Property(x => x.TotalSpend30d).HasColumnName("total_spend_30d").HasPrecision(18, 2).HasDefaultValue(0m);
            e.Property(x => x.TotalIncome30d).HasColumnName("total_income_30d").HasPrecision(18, 2).HasDefaultValue(0m);
            e.Property(x => x.AvgTxnAmount30d).HasColumnName("avg_txn_amount_30d").HasPrecision(18, 2).HasDefaultValue(0m);
            e.Property(x => x.MaxSingleTxn30d).HasColumnName("max_single_txn_30d").HasPrecision(18, 2).HasDefaultValue(0m);
            e.Property(x => x.NetCashflow30d).HasColumnName("net_cashflow_30d").HasPrecision(18, 2).HasDefaultValue(0m);
            e.Property(x => x.MobileTxnPct).HasColumnName("mobile_txn_pct").HasPrecision(5, 4).HasDefaultValue(0m);
            e.Property(x => x.WebTxnPct).HasColumnName("web_txn_pct").HasPrecision(5, 4).HasDefaultValue(0m);
            e.Property(x => x.UssdTxnPct).HasColumnName("ussd_txn_pct").HasPrecision(5, 4).HasDefaultValue(0m);
            e.Property(x => x.AtmTxnPct).HasColumnName("atm_txn_pct").HasPrecision(5, 4).HasDefaultValue(0m);
            e.Property(x => x.BranchTxnPct).HasColumnName("branch_txn_pct").HasPrecision(5, 4).HasDefaultValue(0m);
            e.Property(x => x.ApiTxnPct).HasColumnName("api_txn_pct").HasPrecision(5, 4).HasDefaultValue(0m);
            e.Property(x => x.AvgRiskScore30d).HasColumnName("avg_risk_score_30d").HasPrecision(5, 4).HasDefaultValue(0m);
            e.Property(x => x.HighRiskTxnCount30d).HasColumnName("high_risk_txn_count_30d").HasDefaultValue(0);
            e.Property(x => x.FraudFlags90d).HasColumnName("fraud_flags_90d").HasDefaultValue(0);
            e.Property(x => x.RiskLevelCurrent).HasColumnName("risk_level_current").HasMaxLength(20);
            e.Property(x => x.AccountCount).HasColumnName("account_count").HasDefaultValue(0);
            e.Property(x => x.TotalBalance).HasColumnName("total_balance").HasPrecision(18, 2).HasDefaultValue(0m);
            e.Property(x => x.MaxAccountBalance).HasColumnName("max_account_balance").HasPrecision(18, 2).HasDefaultValue(0m);
            e.Property(x => x.MonthsSinceAccountOpened).HasColumnName("months_since_account_opened");
            e.Property(x => x.OpenCasesCount).HasColumnName("open_cases_count").HasDefaultValue(0);
            e.Property(x => x.AvgSentimentScore90d).HasColumnName("avg_sentiment_score_90d").HasPrecision(5, 4);
            e.Property(x => x.InteractionCount90d).HasColumnName("interaction_count_90d").HasDefaultValue(0);
            e.Property(x => x.CaseResolutionRate).HasColumnName("case_resolution_rate").HasPrecision(5, 4);
            e.Property(x => x.ObPaymentCount30d).HasColumnName("ob_payment_count_30d").HasDefaultValue(0);
            e.Property(x => x.ObPaymentAmount30d).HasColumnName("ob_payment_amount_30d").HasPrecision(18, 2).HasDefaultValue(0m);
            e.Property(x => x.ObFailedPaymentPct30d).HasColumnName("ob_failed_payment_pct_30d").HasPrecision(5, 4).HasDefaultValue(0m);
            e.Property(x => x.FinancialHealthScore).HasColumnName("financial_health_score");
            e.Property(x => x.StressLevel).HasColumnName("stress_level");
            e.Property(x => x.FinancialPersonality).HasColumnName("financial_personality").HasMaxLength(50);
            e.Property(x => x.DaysSinceLastTxn).HasColumnName("days_since_last_txn");
            e.Property(x => x.DaysSinceLastLogin).HasColumnName("days_since_last_login");
            e.Property(x => x.AvgTxnHourOfDay).HasColumnName("avg_txn_hour_of_day").HasPrecision(4, 2);
            e.Property(x => x.WeekendTxnPct).HasColumnName("weekend_txn_pct").HasPrecision(5, 4).HasDefaultValue(0m);
            e.Property(x => x.FeatureVersion).HasColumnName("feature_version").HasMaxLength(20).HasDefaultValue("v1.0");
        });
    }
}
