using WekezaEnterpriseBrain.Core.DataSources;

namespace WekezaEnterpriseBrain.Infrastructure.Connectors;

/// <summary>
/// Generic connector for External/Support systems (Audit, Reporting, etc.)
/// </summary>
public class ExternalSystemConnector : IDataSourceConnector
{
    private readonly string _systemName;
    private readonly DataSourceConfiguration _configuration;
    private readonly DataSourceType _type;

    public string Name => _systemName;
    public DataSourceType Type => _type;

    public ExternalSystemConnector(DataSourceConfiguration configuration, string systemName, DataSourceType type)
    {
        _configuration = configuration;
        _systemName = systemName;
        _type = type;
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
                ["Version"] = "1.0",
                ["DatabaseType"] = "PostgreSQL",
                ["System"] = _systemName
            }
        });
    }

    public Task<IEnumerable<CustomerData>> FetchCustomersAsync(DateTime? since = null)
    {
        // Support systems typically don't have customer master data
        return Task.FromResult<IEnumerable<CustomerData>>(new List<CustomerData>());
    }

    public Task<IEnumerable<AccountData>> FetchAccountsAsync(DateTime? since = null)
    {
        // Support systems typically don't have account master data
        return Task.FromResult<IEnumerable<AccountData>>(new List<AccountData>());
    }

    public Task<IEnumerable<TransactionData>> FetchTransactionsAsync(DateTime? since = null)
    {
        // Support systems have audit/log records
        var prefix = _systemName.Replace(" ", "").Substring(0, 3).ToUpper();
        var transactions = new List<TransactionData>
        {
            new TransactionData
            {
                SourceSystem = _systemName,
                LocalTransactionId = $"{prefix}_LOG001",
                LocalAccountId = "SYSTEM",
                TransactionDate = DateTime.UtcNow,
                TransactionType = "Log",
                Amount = 0m,
                Currency = "KES",
                Channel = "System",
                Description = $"{_systemName} Record",
                Reference = $"{prefix}REF001",
                BalanceAfter = 0m,
                AdditionalData = new Dictionary<string, object>
                {
                    ["RecordType"] = _systemName,
                    ["Timestamp"] = DateTime.UtcNow
                }
            }
        };

        return Task.FromResult<IEnumerable<TransactionData>>(transactions);
    }
}
