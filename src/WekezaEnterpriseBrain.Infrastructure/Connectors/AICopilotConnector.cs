using WekezaEnterpriseBrain.Core.DataSources;

namespace WekezaEnterpriseBrain.Infrastructure.Connectors;

/// <summary>
/// Connector for AI Copilot System
/// </summary>
public class AICopilotConnector : IDataSourceConnector
{
    public string Name => "AI Copilot";
    public DataSourceType Type => DataSourceType.AICopilot;
    
    private readonly DataSourceConfiguration _configuration;

    public AICopilotConnector(DataSourceConfiguration configuration)
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
                ["AIModel"] = "GPT-4",
                ["Interactions"] = 10000
            }
        });
    }

    public Task<IEnumerable<CustomerData>> FetchCustomersAsync(DateTime? since = null)
    {
        var customers = new List<CustomerData>
        {
            new CustomerData
            {
                SourceSystem = "AICopilot",
                LocalCustomerId = "AI001",
                Email = "ai.user@wekeza.com",
                FirstName = "AI",
                LastName = "User",
                Status = "Active",
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow,
                AdditionalData = new Dictionary<string, object>
                {
                    ["InteractionCount"] = 50,
                    ["PreferredLanguage"] = "English"
                }
            }
        };

        return Task.FromResult<IEnumerable<CustomerData>>(customers);
    }

    public Task<IEnumerable<AccountData>> FetchAccountsAsync(DateTime? since = null)
    {
        // AI Copilot doesn't manage accounts directly
        return Task.FromResult<IEnumerable<AccountData>>(new List<AccountData>());
    }

    public Task<IEnumerable<TransactionData>> FetchTransactionsAsync(DateTime? since = null)
    {
        // AI Copilot tracks interactions, not financial transactions
        return Task.FromResult<IEnumerable<TransactionData>>(new List<TransactionData>());
    }
}
