using WekezaEnterpriseBrain.Core.DataSources;

namespace WekezaEnterpriseBrain.Infrastructure.Connectors;

/// <summary>
/// Generic connector for additional Core Banking system variants
/// </summary>
public class GenericCoreBankingConnector : IDataSourceConnector
{
    private readonly string _systemName;
    private readonly DataSourceConfiguration _configuration;

    public string Name => _systemName;
    public DataSourceType Type => DataSourceType.CoreBanking;

    public GenericCoreBankingConnector(DataSourceConfiguration configuration, string systemName)
    {
        _configuration = configuration;
        _systemName = systemName;
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
        var prefix = _systemName.Replace(" ", "").Substring(0, 3).ToUpper();
        var customers = new List<CustomerData>
        {
            new CustomerData
            {
                SourceSystem = _systemName,
                LocalCustomerId = $"{prefix}001",
                NationalId = $"ID{prefix}001",
                PhoneNumber = $"+25470000{prefix.GetHashCode() % 10000:D4}",
                Email = $"customer@{prefix.ToLower()}.com",
                FirstName = _systemName.Split(' ')[0],
                LastName = "Customer",
                DateOfBirth = new DateTime(1990, 1, 1),
                Address = "Nairobi, Kenya",
                Status = "Active",
                CreatedAt = DateTime.UtcNow.AddYears(-1),
                UpdatedAt = DateTime.UtcNow,
                AdditionalData = new Dictionary<string, object>
                {
                    ["KYCLevel"] = "Full",
                    ["System"] = _systemName
                }
            }
        };

        return Task.FromResult<IEnumerable<CustomerData>>(customers);
    }

    public Task<IEnumerable<AccountData>> FetchAccountsAsync(DateTime? since = null)
    {
        var prefix = _systemName.Replace(" ", "").Substring(0, 3).ToUpper();
        var accounts = new List<AccountData>
        {
            new AccountData
            {
                SourceSystem = _systemName,
                LocalAccountId = $"{prefix}_ACC001",
                LocalCustomerId = $"{prefix}001",
                AccountNumber = $"{prefix}{Random.Shared.Next(100000, 999999)}",
                AccountType = "Savings",
                Currency = "KES",
                CurrentBalance = Random.Shared.Next(10000, 200000),
                AvailableBalance = Random.Shared.Next(10000, 200000),
                Status = "Active",
                OpenedDate = DateTime.UtcNow.AddYears(-1),
                UpdatedAt = DateTime.UtcNow
            }
        };

        return Task.FromResult<IEnumerable<AccountData>>(accounts);
    }

    public Task<IEnumerable<TransactionData>> FetchTransactionsAsync(DateTime? since = null)
    {
        var prefix = _systemName.Replace(" ", "").Substring(0, 3).ToUpper();
        var transactions = new List<TransactionData>
        {
            new TransactionData
            {
                SourceSystem = _systemName,
                LocalTransactionId = $"{prefix}_TXN001",
                LocalAccountId = $"{prefix}_ACC001",
                TransactionDate = DateTime.UtcNow.AddDays(-1),
                TransactionType = "Credit",
                Amount = 5000m,
                Currency = "KES",
                Channel = "Branch",
                Description = "Deposit",
                Reference = $"{prefix}REF001",
                BalanceAfter = Random.Shared.Next(10000, 200000)
            }
        };

        return Task.FromResult<IEnumerable<TransactionData>>(transactions);
    }
}
