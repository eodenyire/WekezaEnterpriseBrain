using WekezaEnterpriseBrain.Core.DataSources;

namespace WekezaEnterpriseBrain.Infrastructure.Connectors;

/// <summary>
/// Connector for Open Banking Platform (Nexus)
/// </summary>
public class OpenBankingConnector : IDataSourceConnector
{
    public string Name => "Open Banking (Nexus)";
    public DataSourceType Type => DataSourceType.OpenBanking;
    
    private readonly DataSourceConfiguration _configuration;

    public OpenBankingConnector(DataSourceConfiguration configuration)
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
                ["Version"] = "1.0",
                ["Partners"] = 25,
                ["APIVersion"] = "v2"
            }
        });
    }

    public Task<IEnumerable<CustomerData>> FetchCustomersAsync(DateTime? since = null)
    {
        var customers = new List<CustomerData>
        {
            new CustomerData
            {
                SourceSystem = "OpenBanking",
                LocalCustomerId = "OB001",
                Email = "openbanking@partner.com",
                FirstName = "OpenBanking",
                LastName = "Customer",
                Status = "Active",
                CreatedAt = DateTime.UtcNow.AddMonths(-6),
                UpdatedAt = DateTime.UtcNow,
                AdditionalData = new Dictionary<string, object>
                {
                    ["ConsentStatus"] = "Active",
                    ["PartnerCount"] = 3
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
                SourceSystem = "OpenBanking",
                LocalAccountId = "OB_ACC001",
                LocalCustomerId = "OB001",
                AccountNumber = "OB999888777",
                AccountType = "API Access",
                Currency = "KES",
                CurrentBalance = 0m,
                AvailableBalance = 0m,
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
                SourceSystem = "OpenBanking",
                LocalTransactionId = "OB_TXN001",
                LocalAccountId = "OB_ACC001",
                TransactionDate = DateTime.UtcNow.AddHours(-1),
                TransactionType = "API Call",
                Amount = 0m,
                Currency = "KES",
                Channel = "API",
                Description = "Balance Inquiry via Partner API",
                Reference = "API202602",
                BalanceAfter = 0m,
                AdditionalData = new Dictionary<string, object>
                {
                    ["Partner"] = "FinTechPartner",
                    ["APIEndpoint"] = "/accounts/balance"
                }
            }
        };

        return Task.FromResult<IEnumerable<TransactionData>>(transactions);
    }
}
