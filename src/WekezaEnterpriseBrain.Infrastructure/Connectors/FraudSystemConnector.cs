using WekezaEnterpriseBrain.Core.DataSources;

namespace WekezaEnterpriseBrain.Infrastructure.Connectors;

/// <summary>
/// Sample connector for Fraud Detection system
/// </summary>
public class FraudSystemConnector : IDataSourceConnector
{
    public string Name => "Fraud Detection System";
    public DataSourceType Type => DataSourceType.FraudSystem;
    
    private readonly DataSourceConfiguration _configuration;

    public FraudSystemConnector(DataSourceConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<DataSourceConnectionResult> TestConnectionAsync()
    {
        return Task.FromResult(new DataSourceConnectionResult
        {
            IsConnected = true,
            Message = $"Successfully connected to {Name}",
            TestedAt = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                ["Version"] = "3.0",
                ["AlertsToday"] = 15
            }
        });
    }

    public Task<IEnumerable<CustomerData>> FetchCustomersAsync(DateTime? since = null)
    {
        // Fraud system doesn't manage customer master data
        return Task.FromResult<IEnumerable<CustomerData>>(new List<CustomerData>());
    }

    public Task<IEnumerable<AccountData>> FetchAccountsAsync(DateTime? since = null)
    {
        // Fraud system doesn't manage account master data
        return Task.FromResult<IEnumerable<AccountData>>(new List<AccountData>());
    }

    public Task<IEnumerable<TransactionData>> FetchTransactionsAsync(DateTime? since = null)
    {
        // Fraud system captures flagged transactions
        var transactions = new List<TransactionData>
        {
            new TransactionData
            {
                SourceSystem = "FraudSystem",
                LocalTransactionId = "FRAUD_001",
                LocalAccountId = "ACC001",
                TransactionDate = DateTime.UtcNow.AddHours(-5),
                TransactionType = "Debit",
                Amount = 100000m,
                Currency = "KES",
                Channel = "Web",
                Description = "Flagged Transaction",
                Reference = "ALERT001",
                BalanceAfter = 0m,
                AdditionalData = new Dictionary<string, object>
                {
                    ["FraudScore"] = 0.85,
                    ["FraudReason"] = "Unusual amount and location",
                    ["Status"] = "Blocked"
                }
            }
        };

        return Task.FromResult<IEnumerable<TransactionData>>(transactions);
    }
}
