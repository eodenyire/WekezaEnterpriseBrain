namespace WekezaEnterpriseBrain.Infrastructure.DataHub.Etl;

/// <summary>Result of a single ETL sync operation</summary>
public class EtlSyncResult
{
    public string SourceSystem { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int RecordsRead { get; set; }
    public int RecordsInserted { get; set; }
    public int RecordsUpdated { get; set; }
    public int RecordsSkipped { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset EndedAt { get; set; }
    public TimeSpan Duration => EndedAt - StartedAt;
}

/// <summary>Interface for ETL services that pull data from a source system into the Datahub</summary>
public interface IEtlService
{
    string SourceSystemName { get; }

    /// <summary>Sync all entity types from this source system</summary>
    Task<IEnumerable<EtlSyncResult>> SyncAllAsync(DateTimeOffset? since = null, CancellationToken ct = default);

    /// <summary>Sync customers only</summary>
    Task<EtlSyncResult> SyncCustomersAsync(DateTimeOffset? since = null, CancellationToken ct = default);

    /// <summary>Sync accounts only</summary>
    Task<EtlSyncResult> SyncAccountsAsync(DateTimeOffset? since = null, CancellationToken ct = default);

    /// <summary>Sync transactions only</summary>
    Task<EtlSyncResult> SyncTransactionsAsync(DateTimeOffset? since = null, CancellationToken ct = default);

    /// <summary>Test connectivity to the source system</summary>
    Task<bool> TestConnectionAsync(CancellationToken ct = default);
}

/// <summary>Orchestrates ETL across all source systems</summary>
public interface IDataHubOrchestrator
{
    /// <summary>Run full sync across all enabled source systems</summary>
    Task<IEnumerable<EtlSyncResult>> SyncAllSystemsAsync(DateTimeOffset? since = null, CancellationToken ct = default);

    /// <summary>Run sync for a specific source system</summary>
    Task<IEnumerable<EtlSyncResult>> SyncSystemAsync(string sourceSystemName, DateTimeOffset? since = null, CancellationToken ct = default);

    /// <summary>Refresh all materialized views and feature store</summary>
    Task RefreshAnalyticsAsync(CancellationToken ct = default);

    /// <summary>Get last sync timestamps for all source systems</summary>
    Task<Dictionary<string, DateTimeOffset?>> GetLastSyncTimestampsAsync();
}
