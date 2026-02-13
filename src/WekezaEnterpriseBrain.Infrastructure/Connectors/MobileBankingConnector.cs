using WekezaEnterpriseBrain.Core.DataSources;

namespace WekezaEnterpriseBrain.Infrastructure.Connectors;

/// <summary>
/// Sample connector for Mobile Banking channel
/// </summary>
public class MobileBankingConnector : IDataSourceConnector
{
    public string Name => "Mobile Banking";
    public DataSourceType Type => DataSourceType.MobileBanking;
    
    private readonly DataSourceConfiguration _configuration;

    public MobileBankingConnector(DataSourceConfiguration configuration)
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
                ["Platform"] = "iOS/Android",
                ["ActiveUsers"] = 10000
            }
        });
    }

    public Task<IEnumerable<CustomerData>> FetchCustomersAsync(DateTime? since = null)
    {
        var customers = new List<CustomerData>
        {
            new CustomerData
            {
                SourceSystem = "MobileBanking",
                LocalCustomerId = "MB002",
                PhoneNumber = "+254700000002",
                Email = "jane.smith@example.com",
                FirstName = "Jane",
                LastName = "Smith",
                Status = "Active",
                CreatedAt = DateTime.UtcNow.AddMonths(-6),
                UpdatedAt = DateTime.UtcNow,
                AdditionalData = new Dictionary<string, object>
                {
                    ["DeviceType"] = "Android",
                    ["AppVersion"] = "2.5.1"
                }
            }
        };

        return Task.FromResult<IEnumerable<CustomerData>>(customers);
    }

    public Task<IEnumerable<AccountData>> FetchAccountsAsync(DateTime? since = null)
    {
        var accounts = new List<AccountData>
        {
            new AccountData
            {
                SourceSystem = "MobileBanking",
                LocalAccountId = "MB_ACC002",
                LocalCustomerId = "MB002",
                AccountNumber = "9876543210",
                AccountType = "Current",
                Currency = "KES",
                CurrentBalance = 75000m,
                AvailableBalance = 75000m,
                Status = "Active",
                OpenedDate = DateTime.UtcNow.AddMonths(-6),
                UpdatedAt = DateTime.UtcNow
            }
        };

        return Task.FromResult<IEnumerable<AccountData>>(accounts);
    }

    public Task<IEnumerable<TransactionData>> FetchTransactionsAsync(DateTime? since = null)
    {
        var transactions = new List<TransactionData>
        {
            new TransactionData
            {
                SourceSystem = "MobileBanking",
                LocalTransactionId = "MB_TXN002",
                LocalAccountId = "MB_ACC002",
                TransactionDate = DateTime.UtcNow.AddHours(-2),
                TransactionType = "Debit",
                Amount = 5000m,
                Currency = "KES",
                Channel = "Mobile",
                Description = "Bill Payment",
                Reference = "BILL202602",
                BalanceAfter = 70000m,
                AdditionalData = new Dictionary<string, object>
                {
                    ["BillerName"] = "KPLC",
                    ["BillerAccount"] = "123456"
                }
            }
        };

        return Task.FromResult<IEnumerable<TransactionData>>(transactions);
    }
}
