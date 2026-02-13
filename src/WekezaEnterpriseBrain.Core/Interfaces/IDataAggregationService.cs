using WekezaEnterpriseBrain.Core.DataSources;

namespace WekezaEnterpriseBrain.Core.Interfaces;

/// <summary>
/// Service for aggregating data from multiple Wekeza systems
/// </summary>
public interface IDataAggregationService
{
    /// <summary>
    /// Sync data from a specific data source
    /// </summary>
    Task<DataSyncResult> SyncDataSourceAsync(Guid dataSourceId);
    
    /// <summary>
    /// Sync data from all enabled data sources
    /// </summary>
    Task<IEnumerable<DataSyncResult>> SyncAllDataSourcesAsync();
    
    /// <summary>
    /// Process incoming customer data and create/update Customer 360
    /// </summary>
    Task ProcessCustomerDataAsync(CustomerData customerData);
    
    /// <summary>
    /// Process incoming account data
    /// </summary>
    Task ProcessAccountDataAsync(AccountData accountData);
    
    /// <summary>
    /// Process incoming transaction data
    /// </summary>
    Task ProcessTransactionDataAsync(TransactionData transactionData);
    
    /// <summary>
    /// Get aggregation statistics
    /// </summary>
    Task<DataAggregationStats> GetStatisticsAsync();
}

/// <summary>
/// Result of a data synchronization operation
/// </summary>
public class DataSyncResult
{
    public Guid DataSourceId { get; set; }
    public string DataSourceName { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
    public int CustomersProcessed { get; set; }
    public int AccountsProcessed { get; set; }
    public int TransactionsProcessed { get; set; }
    public TimeSpan Duration { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CompletedAt { get; set; }
}

/// <summary>
/// Statistics about data aggregation
/// </summary>
public class DataAggregationStats
{
    public int TotalDataSources { get; set; }
    public int EnabledDataSources { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalAccounts { get; set; }
    public int TotalTransactions { get; set; }
    public DateTime? LastSyncTime { get; set; }
    public Dictionary<string, int> SourceSystemCounts { get; set; } = new();
}
