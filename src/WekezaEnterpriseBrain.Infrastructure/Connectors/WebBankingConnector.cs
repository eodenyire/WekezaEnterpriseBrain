using WekezaEnterpriseBrain.Core.DataSources;

namespace WekezaEnterpriseBrain.Infrastructure.Connectors;

/// <summary>
/// Connector for Web Banking channel
/// </summary>
public class WebBankingConnector : IDataSourceConnector
{
    public string Name => "Web Banking";
    public DataSourceType Type => DataSourceType.WebBanking;
    
    private readonly DataSourceConfiguration _configuration;

    public WebBankingConnector(DataSourceConfiguration configuration)
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
                ["Platform"] = "Web Portal",
                ["ActiveSessions"] = 5000
            }
        });
    }

    public Task<IEnumerable<CustomerData>> FetchCustomersAsync(DateTime? since = null)
    {
        var customers = new List<CustomerData>
        {
            new CustomerData
            {
                SourceSystem = "WebBanking",
                LocalCustomerId = "WB001",
                PhoneNumber = "+254700000003",
                Email = "customer@webmail.com",
                FirstName = "Web",
                LastName = "Customer",
                Status = "Active",
                CreatedAt = DateTime.UtcNow.AddMonths(-3),
                UpdatedAt = DateTime.UtcNow,
                AdditionalData = new Dictionary<string, object>
                {
                    ["Browser"] = "Chrome",
                    ["LastLogin"] = DateTime.UtcNow.AddHours(-1)
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
                SourceSystem = "WebBanking",
                LocalAccountId = "WB_ACC001",
                LocalCustomerId = "WB001",
                AccountNumber = "5555666677",
                AccountType = "Savings",
                Currency = "KES",
                CurrentBalance = 120000m,
                AvailableBalance = 120000m,
                Status = "Active",
                OpenedDate = DateTime.UtcNow.AddMonths(-3),
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
                SourceSystem = "WebBanking",
                LocalTransactionId = "WB_TXN001",
                LocalAccountId = "WB_ACC001",
                TransactionDate = DateTime.UtcNow.AddHours(-3),
                TransactionType = "Debit",
                Amount = 8000m,
                Currency = "KES",
                Channel = "Web",
                Description = "Online Payment",
                Reference = "WEB202602",
                BalanceAfter = 112000m
            }
        };

        return Task.FromResult<IEnumerable<TransactionData>>(transactions);
    }
}
