using WekezaEnterpriseBrain.Core.DataSources;

namespace WekezaEnterpriseBrain.Infrastructure.Connectors;

/// <summary>
/// Sample connector for Core Banking system (ComprehensiveWekezaApi)
/// In production, this would connect to actual database or API
/// </summary>
public class CoreBankingConnector : IDataSourceConnector
{
    public string Name => "Core Banking System";
    public DataSourceType Type => DataSourceType.CoreBanking;
    
    private readonly DataSourceConfiguration _configuration;

    public CoreBankingConnector(DataSourceConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<DataSourceConnectionResult> TestConnectionAsync()
    {
        // In production, test actual connection
        return Task.FromResult(new DataSourceConnectionResult
        {
            IsConnected = true,
            Message = $"Successfully connected to {Name}",
            TestedAt = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                ["Version"] = "1.0",
                ["DatabaseType"] = "PostgreSQL"
            }
        });
    }

    public Task<IEnumerable<CustomerData>> FetchCustomersAsync(DateTime? since = null)
    {
        // In production, fetch from actual database
        // For now, return sample data
        var customers = new List<CustomerData>
        {
            new CustomerData
            {
                SourceSystem = "CoreBanking",
                LocalCustomerId = "CB001",
                NationalId = "12345678",
                PhoneNumber = "+254700000001",
                Email = "john.doe@example.com",
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateTime(1985, 5, 15),
                Address = "Nairobi, Kenya",
                Status = "Active",
                CreatedAt = DateTime.UtcNow.AddYears(-2),
                UpdatedAt = DateTime.UtcNow,
                AdditionalData = new Dictionary<string, object>
                {
                    ["KYCLevel"] = "Full",
                    ["Branch"] = "Main Branch"
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
                SourceSystem = "CoreBanking",
                LocalAccountId = "ACC001",
                LocalCustomerId = "CB001",
                AccountNumber = "1234567890",
                AccountType = "Savings",
                Currency = "KES",
                CurrentBalance = 50000m,
                AvailableBalance = 50000m,
                Status = "Active",
                OpenedDate = DateTime.UtcNow.AddYears(-2),
                UpdatedAt = DateTime.UtcNow,
                AdditionalData = new Dictionary<string, object>
                {
                    ["InterestRate"] = 3.5,
                    ["AccountTier"] = "Silver"
                }
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
                SourceSystem = "CoreBanking",
                LocalTransactionId = "TXN001",
                LocalAccountId = "ACC001",
                TransactionDate = DateTime.UtcNow.AddDays(-1),
                TransactionType = "Credit",
                Amount = 10000m,
                Currency = "KES",
                Channel = "Branch",
                Description = "Salary Payment",
                Reference = "SAL202602",
                BalanceAfter = 50000m,
                AdditionalData = new Dictionary<string, object>
                {
                    ["TellerID"] = "TLR001"
                }
            }
        };

        return Task.FromResult<IEnumerable<TransactionData>>(transactions);
    }
}
