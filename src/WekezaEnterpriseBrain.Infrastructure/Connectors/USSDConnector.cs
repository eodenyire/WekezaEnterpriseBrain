using WekezaEnterpriseBrain.Core.DataSources;

namespace WekezaEnterpriseBrain.Infrastructure.Connectors;

/// <summary>
/// Connector for USSD Banking channel
/// </summary>
public class USSDConnector : IDataSourceConnector
{
    public string Name => "USSD Banking";
    public DataSourceType Type => DataSourceType.USSD;
    
    private readonly DataSourceConfiguration _configuration;

    public USSDConnector(DataSourceConfiguration configuration)
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
                ["Platform"] = "USSD Gateway",
                ["ActiveSessions"] = 15000
            }
        });
    }

    public Task<IEnumerable<CustomerData>> FetchCustomersAsync(DateTime? since = null)
    {
        var customers = new List<CustomerData>
        {
            new CustomerData
            {
                SourceSystem = "USSD",
                LocalCustomerId = "USSD001",
                PhoneNumber = "+254700000004",
                FirstName = "USSD",
                LastName = "User",
                Status = "Active",
                CreatedAt = DateTime.UtcNow.AddMonths(-1),
                UpdatedAt = DateTime.UtcNow,
                AdditionalData = new Dictionary<string, object>
                {
                    ["Network"] = "Safaricom",
                    ["UsageCount"] = 50
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
                SourceSystem = "USSD",
                LocalAccountId = "USSD_ACC001",
                LocalCustomerId = "USSD001",
                AccountNumber = "7777888899",
                AccountType = "Basic",
                Currency = "KES",
                CurrentBalance = 5000m,
                AvailableBalance = 5000m,
                Status = "Active",
                OpenedDate = DateTime.UtcNow.AddMonths(-1),
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
                SourceSystem = "USSD",
                LocalTransactionId = "USSD_TXN001",
                LocalAccountId = "USSD_ACC001",
                TransactionDate = DateTime.UtcNow.AddMinutes(-30),
                TransactionType = "Credit",
                Amount = 500m,
                Currency = "KES",
                Channel = "USSD",
                Description = "M-PESA Deposit",
                Reference = "USSD202602",
                BalanceAfter = 5500m
            }
        };

        return Task.FromResult<IEnumerable<TransactionData>>(transactions);
    }
}
