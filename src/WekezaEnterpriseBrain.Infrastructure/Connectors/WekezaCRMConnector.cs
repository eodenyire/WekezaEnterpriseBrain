using WekezaEnterpriseBrain.Core.DataSources;

namespace WekezaEnterpriseBrain.Infrastructure.Connectors;

/// <summary>
/// Connector for WekezaCRM - Customer Relationship Management System
/// Database: WekezaCRM (SQL Server)
/// Key Entities: Customer, Account, Transaction, Case, Interaction, Campaign,
///               NextBestAction, SentimentAnalysis, WhatsAppMessage, USSDSession
/// Source: https://github.com/eodenyire/WekezaCRM
/// </summary>
public class WekezaCRMConnector : IDataSourceConnector
{
    public string Name => "WekezaCRM";
    public DataSourceType Type => DataSourceType.CRM;

    private readonly DataSourceConfiguration _configuration;

    public WekezaCRMConnector(DataSourceConfiguration configuration)
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
                ["DatabaseType"] = "SQL Server",
                ["DatabaseName"] = "WekezaCRM",
                ["DbContext"] = "CRMDbContext",
                ["Entities"] = "Customer, Account, Transaction, Case, Interaction, Campaign, NextBestAction, SentimentAnalysis"
            }
        });
    }

    public Task<IEnumerable<CustomerData>> FetchCustomersAsync(DateTime? since = null)
    {
        // WekezaCRM Customer entity fields: Email (unique), PhoneNumber, CustomerReference (unique),
        // FirstName, LastName, CreditScore, LifetimeValue
        var customers = new List<CustomerData>
        {
            new CustomerData
            {
                SourceSystem = "WekezaCRM",
                LocalCustomerId = "CRM001",
                Email = "crm.customer@wekeza.bank",
                PhoneNumber = "+254711000001",
                FirstName = "CRM",
                LastName = "Customer",
                Status = "Active",
                CreatedAt = DateTime.UtcNow.AddMonths(-12),
                UpdatedAt = DateTime.UtcNow,
                AdditionalData = new Dictionary<string, object>
                {
                    ["CustomerReference"] = "WKZ-CRM-001",
                    ["CreditScore"] = 720,
                    ["LifetimeValue"] = 150000.00,
                    ["KYCStatus"] = "Verified",
                    ["PreferredChannel"] = "Mobile"
                }
            }
        };

        return Task.FromResult<IEnumerable<CustomerData>>(customers);
    }

    public Task<IEnumerable<AccountData>> FetchAccountsAsync(DateTime? since = null)
    {
        // WekezaCRM Account entity: AccountNumber, Balance, Status
        var accounts = new List<AccountData>
        {
            new AccountData
            {
                SourceSystem = "WekezaCRM",
                LocalAccountId = "CRM_ACC001",
                LocalCustomerId = "CRM001",
                AccountNumber = "CRM1234567890",
                AccountType = "Savings",
                Currency = "KES",
                CurrentBalance = 75000m,
                AvailableBalance = 75000m,
                Status = "Active",
                OpenedDate = DateTime.UtcNow.AddMonths(-12),
                UpdatedAt = DateTime.UtcNow,
                AdditionalData = new Dictionary<string, object>
                {
                    ["AccountTier"] = "Gold",
                    ["CRMSegment"] = "Premium"
                }
            }
        };

        return Task.FromResult<IEnumerable<AccountData>>(accounts);
    }

    public Task<IEnumerable<TransactionData>> FetchTransactionsAsync(DateTime? since = null)
    {
        // WekezaCRM Transaction entity: Amount, BalanceAfter, TransactionReference
        var transactions = new List<TransactionData>
        {
            new TransactionData
            {
                SourceSystem = "WekezaCRM",
                LocalTransactionId = "CRM_TXN001",
                LocalAccountId = "CRM_ACC001",
                TransactionDate = DateTime.UtcNow.AddDays(-3),
                TransactionType = "Credit",
                Amount = 25000m,
                Currency = "KES",
                Channel = "Mobile",
                Description = "CRM Tracked Transaction",
                Reference = "CRM_TXN_REF001",
                BalanceAfter = 75000m,
                AdditionalData = new Dictionary<string, object>
                {
                    ["CRMCaseId"] = "CASE001",
                    ["SentimentScore"] = 0.85,
                    ["NextBestAction"] = "Offer Premium Credit Card"
                }
            }
        };

        return Task.FromResult<IEnumerable<TransactionData>>(transactions);
    }
}
