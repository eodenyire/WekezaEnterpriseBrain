using WekezaEnterpriseBrain.Core.DataSources;

namespace WekezaEnterpriseBrain.Infrastructure.Connectors;

/// <summary>
/// Connector for Risk Management System (ERMS)
/// </summary>
public class RiskSystemConnector : IDataSourceConnector
{
    public string Name => "Risk Management (ERMS)";
    public DataSourceType Type => DataSourceType.RiskSystem;
    
    private readonly DataSourceConfiguration _configuration;

    public RiskSystemConnector(DataSourceConfiguration configuration)
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
                ["Version"] = "2.0",
                ["RiskAssessments"] = 200
            }
        });
    }

    public Task<IEnumerable<CustomerData>> FetchCustomersAsync(DateTime? since = null)
    {
        // Risk system doesn't manage customer master data
        return Task.FromResult<IEnumerable<CustomerData>>(new List<CustomerData>());
    }

    public Task<IEnumerable<AccountData>> FetchAccountsAsync(DateTime? since = null)
    {
        // Risk system doesn't manage account master data
        return Task.FromResult<IEnumerable<AccountData>>(new List<AccountData>());
    }

    public Task<IEnumerable<TransactionData>> FetchTransactionsAsync(DateTime? since = null)
    {
        // Risk system captures risk-flagged transactions
        var transactions = new List<TransactionData>
        {
            new TransactionData
            {
                SourceSystem = "RiskSystem",
                LocalTransactionId = "RISK_001",
                LocalAccountId = "ACC002",
                TransactionDate = DateTime.UtcNow.AddHours(-2),
                TransactionType = "Debit",
                Amount = 50000m,
                Currency = "KES",
                Channel = "Mobile",
                Description = "High Risk Transaction",
                Reference = "RISK001",
                BalanceAfter = 0m,
                AdditionalData = new Dictionary<string, object>
                {
                    ["RiskScore"] = 0.75,
                    ["RiskReason"] = "Unusual transaction pattern",
                    ["Status"] = "Under Review"
                }
            }
        };

        return Task.FromResult<IEnumerable<TransactionData>>(transactions);
    }
}
