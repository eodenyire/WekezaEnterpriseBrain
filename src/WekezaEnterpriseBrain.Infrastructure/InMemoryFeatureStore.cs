using WekezaEnterpriseBrain.Core.Features;
using WekezaEnterpriseBrain.Core.Interfaces;

namespace WekezaEnterpriseBrain.Infrastructure;

/// <summary>
/// In-memory implementation of feature store
/// </summary>
public class InMemoryFeatureStore : IFeatureStore
{
    private readonly Dictionary<Guid, CustomerFeatures> _features = new();
    private readonly ICustomer360Service _customer360Service;
    private readonly object _lock = new();

    public InMemoryFeatureStore(ICustomer360Service customer360Service)
    {
        _customer360Service = customer360Service;
    }

    public Task<CustomerFeatures?> GetFeaturesAsync(Guid globalCustomerId)
    {
        lock (_lock)
        {
            _features.TryGetValue(globalCustomerId, out var features);
            return Task.FromResult(features);
        }
    }

    public async Task<CustomerFeatures> CalculateFeaturesAsync(Guid globalCustomerId)
    {
        var customer = await _customer360Service.GetCustomerByGlobalIdAsync(globalCustomerId);
        if (customer == null)
        {
            throw new ArgumentException($"Customer {globalCustomerId} not found");
        }

        var accounts = await _customer360Service.GetCustomerAccountsAsync(globalCustomerId);
        var transactions = await _customer360Service.GetRecentTransactionsAsync(globalCustomerId, 1000);

        var features = new CustomerFeatures
        {
            GlobalCustomerId = globalCustomerId,
            CalculatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Calculate transaction features
        var last30Days = transactions.Where(t => t.TransactionDate >= DateTime.UtcNow.AddDays(-30)).ToList();
        var last90Days = transactions.Where(t => t.TransactionDate >= DateTime.UtcNow.AddDays(-90)).ToList();

        features.TransactionCount30Days = last30Days.Count;
        features.TransactionCount90Days = last90Days.Count;
        
        var monthlyOutflows = last30Days.Where(t => t.TransactionType == "Debit").Sum(t => t.Amount);
        var monthlyInflows = last30Days.Where(t => t.TransactionType == "Credit").Sum(t => t.Amount);
        
        features.MonthlyOutflow = monthlyOutflows;
        features.MonthlyInflow = monthlyInflows;
        features.MonthlyAverageSpend = last30Days.Any() ? monthlyOutflows / 30 : 0;

        // Channel behavior
        features.MobileLoginCount30Days = customer.LoginCount; // Simplified
        features.LastLoginDate = customer.LastLoginDate;
        features.PreferredChannel = customer.LastLoginChannel ?? "Unknown";

        // Risk indicators
        features.CurrentRiskScore = customer.RiskScore;
        features.FailedLoginAttempts = customer.FailedLoginAttempts;
        
        // Velocity score (simplified calculation)
        features.VelocityScore = features.TransactionCount30Days > 100 ? 0.8m : 0.2m;

        // Financial health
        features.AverageDailyBalance = customer.AverageDailyBalance;
        var accountsList = accounts.ToList();
        if (accountsList.Any())
        {
            features.MinBalance30Days = accountsList.Min(a => a.CurrentBalance);
            features.MaxBalance30Days = accountsList.Max(a => a.CurrentBalance);
        }

        // Hourly transaction pattern
        for (int hour = 0; hour < 24; hour++)
        {
            var hourlyTxns = transactions
                .Where(t => t.TransactionDate.Hour == hour)
                .Sum(t => t.Amount);
            features.HourlyTransactionPattern[hour] = hourlyTxns;
        }

        // Day of week pattern
        for (int day = 0; day < 7; day++)
        {
            var dayTxns = transactions
                .Where(t => (int)t.TransactionDate.DayOfWeek == day)
                .Count();
            features.DayOfWeekPattern[day] = dayTxns;
        }

        // Customer segment
        features.IsHighValueCustomer = customer.TotalBalance > 1000000m;
        features.CustomerSegment = customer.Segment;

        lock (_lock)
        {
            _features[globalCustomerId] = features;
        }

        return features;
    }

    public async Task RefreshAllFeaturesAsync()
    {
        // This would be run as a batch job
        // For now, just a placeholder
        await Task.CompletedTask;
    }

    public Task<Dictionary<string, double>> GetFeatureImportanceAsync()
    {
        // Return static feature importance for now
        // In production, this would come from ML model
        var importance = new Dictionary<string, double>
        {
            ["CurrentRiskScore"] = 0.25,
            ["MonthlyAverageSpend"] = 0.20,
            ["TransactionCount30Days"] = 0.15,
            ["AverageDailyBalance"] = 0.15,
            ["VelocityScore"] = 0.10,
            ["FailedLoginAttempts"] = 0.08,
            ["IsHighValueCustomer"] = 0.07
        };

        return Task.FromResult(importance);
    }
}
