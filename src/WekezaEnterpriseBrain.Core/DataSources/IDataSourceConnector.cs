namespace WekezaEnterpriseBrain.Core.DataSources;

/// <summary>
/// Interface for connecting to external Wekeza data sources
/// </summary>
public interface IDataSourceConnector
{
    string Name { get; }
    DataSourceType Type { get; }
    Task<DataSourceConnectionResult> TestConnectionAsync();
    Task<IEnumerable<CustomerData>> FetchCustomersAsync(DateTime? since = null);
    Task<IEnumerable<AccountData>> FetchAccountsAsync(DateTime? since = null);
    Task<IEnumerable<TransactionData>> FetchTransactionsAsync(DateTime? since = null);
}

/// <summary>
/// Raw customer data from external sources
/// </summary>
public class CustomerData
{
    public string SourceSystem { get; set; } = string.Empty;
    public string LocalCustomerId { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}

/// <summary>
/// Raw account data from external sources
/// </summary>
public class AccountData
{
    public string SourceSystem { get; set; } = string.Empty;
    public string LocalAccountId { get; set; } = string.Empty;
    public string LocalCustomerId { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public string Currency { get; set; } = "KES";
    public decimal CurrentBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime OpenedDate { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}

/// <summary>
/// Raw transaction data from external sources
/// </summary>
public class TransactionData
{
    public string SourceSystem { get; set; } = string.Empty;
    public string LocalTransactionId { get; set; } = string.Empty;
    public string LocalAccountId { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "KES";
    public string Channel { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Reference { get; set; }
    public decimal BalanceAfter { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}
