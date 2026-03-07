using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace WekezaEnterpriseBrain.Infrastructure.DataHub.Etl;

/// <summary>
/// Orchestrates ETL pipelines across all Wekeza source systems.
/// Coordinates: WekezaBank, WekezaCRM, WekezaOpenBanking ETL services
/// and refreshes analytics materialized views after each sync.
/// </summary>
public class DataHubOrchestrator : IDataHubOrchestrator
{
    private readonly WekezaDataHubDbContext _hub;
    private readonly IEnumerable<IEtlService> _etlServices;
    private readonly ILogger<DataHubOrchestrator> _logger;

    public DataHubOrchestrator(
        WekezaDataHubDbContext hub,
        IEnumerable<IEtlService> etlServices,
        ILogger<DataHubOrchestrator> logger)
    {
        _hub = hub;
        _etlServices = etlServices;
        _logger = logger;
    }

    public async Task<IEnumerable<EtlSyncResult>> SyncAllSystemsAsync(
        DateTimeOffset? since = null,
        CancellationToken ct = default)
    {
        var allResults = new List<EtlSyncResult>();

        foreach (var etl in _etlServices)
        {
            try
            {
                _logger.LogInformation("Starting sync for {System}", etl.SourceSystemName);
                var results = await etl.SyncAllAsync(since, ct);
                allResults.AddRange(results);

                var succeeded = results.Count(r => r.Success);
                var failed = results.Count(r => !r.Success);
                _logger.LogInformation(
                    "Completed sync for {System}: {Succeeded} succeeded, {Failed} failed, " +
                    "{Inserted} inserted, {Updated} updated",
                    etl.SourceSystemName, succeeded, failed,
                    results.Sum(r => r.RecordsInserted),
                    results.Sum(r => r.RecordsUpdated));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ETL failed for {System}", etl.SourceSystemName);
                allResults.Add(new EtlSyncResult
                {
                    SourceSystem = etl.SourceSystemName,
                    EntityType = "all",
                    Success = false,
                    ErrorMessage = ex.Message,
                    StartedAt = DateTimeOffset.UtcNow,
                    EndedAt = DateTimeOffset.UtcNow
                });
            }
        }

        return allResults;
    }

    public async Task<IEnumerable<EtlSyncResult>> SyncSystemAsync(
        string sourceSystemName,
        DateTimeOffset? since = null,
        CancellationToken ct = default)
    {
        var etl = _etlServices.FirstOrDefault(s =>
            s.SourceSystemName.Equals(sourceSystemName, StringComparison.OrdinalIgnoreCase));

        if (etl == null)
        {
            return new[]
            {
                new EtlSyncResult
                {
                    SourceSystem = sourceSystemName,
                    EntityType = "all",
                    Success = false,
                    ErrorMessage = $"No ETL service registered for '{sourceSystemName}'",
                    StartedAt = DateTimeOffset.UtcNow,
                    EndedAt = DateTimeOffset.UtcNow
                }
            };
        }

        return await etl.SyncAllAsync(since, ct);
    }

    public async Task RefreshAnalyticsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Refreshing analytics materialized views");
            await _hub.Database.ExecuteSqlRawAsync(
                "REFRESH MATERIALIZED VIEW CONCURRENTLY analytics.customer_360", ct);
            await _hub.Database.ExecuteSqlRawAsync(
                "REFRESH MATERIALIZED VIEW CONCURRENTLY analytics.daily_transaction_summary", ct);
            await _hub.Database.ExecuteSqlRawAsync(
                "REFRESH MATERIALIZED VIEW CONCURRENTLY analytics.risk_dashboard", ct);
            _logger.LogInformation("Analytics views refreshed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh analytics views");
            throw;
        }
    }

    public async Task<Dictionary<string, DateTimeOffset?>> GetLastSyncTimestampsAsync()
    {
        var result = new Dictionary<string, DateTimeOffset?>();
        foreach (var etl in _etlServices)
        {
            // Check when the last record was ingested for this source system
            var lastTxn = await _hub.FactTransactions
                .Where(t => t.SourceSystem == etl.SourceSystemName)
                .MaxAsync(t => (DateTimeOffset?)t.IngestedAt);
            result[etl.SourceSystemName] = lastTxn;
        }
        return result;
    }
}
