using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Dapper;
using Npgsql;
using WekezaEnterpriseBrain.Infrastructure.DataHub.Models;

namespace WekezaEnterpriseBrain.Infrastructure.DataHub.Etl;

/// <summary>
/// ETL service for WekezaBank Risk Management System.
/// Source: PostgreSQL database 'risk_management' (eodenyire/WekezaBank)
/// Tables: analyst_cases, risk_metrics, transaction_history
/// Pattern: Polling every 30s, Isolation Forest ML for risk scoring
/// </summary>
public class WekezaBankEtlService : IEtlService
{
    private readonly WekezaDataHubDbContext _hub;
    private readonly ILogger<WekezaBankEtlService> _logger;
    private readonly string _sourceConnectionString;

    public string SourceSystemName => "WekezaBank";

    public WekezaBankEtlService(
        WekezaDataHubDbContext hub,
        ILogger<WekezaBankEtlService> logger,
        string sourceConnectionString)
    {
        _hub = hub;
        _logger = logger;
        _sourceConnectionString = sourceConnectionString;
    }

    public async Task<bool> TestConnectionAsync(CancellationToken ct = default)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_sourceConnectionString);
            await conn.OpenAsync(ct);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "WekezaBank connection test failed");
            return false;
        }
    }

    public async Task<IEnumerable<EtlSyncResult>> SyncAllAsync(DateTimeOffset? since = null, CancellationToken ct = default)
    {
        var results = new List<EtlSyncResult>();
        results.Add(await SyncRiskAssessmentsAsync(since, ct));
        results.Add(await SyncTransactionsAsync(since, ct));
        return results;
    }

    public Task<EtlSyncResult> SyncCustomersAsync(DateTimeOffset? since = null, CancellationToken ct = default)
    {
        // WekezaBank does not maintain customer master data - customers are referenced by ID only
        return Task.FromResult(new EtlSyncResult
        {
            SourceSystem = SourceSystemName,
            EntityType = "customers",
            Success = true,
            RecordsRead = 0,
            RecordsInserted = 0,
            StartedAt = DateTimeOffset.UtcNow,
            EndedAt = DateTimeOffset.UtcNow
        });
    }

    public Task<EtlSyncResult> SyncAccountsAsync(DateTimeOffset? since = null, CancellationToken ct = default)
    {
        // WekezaBank does not maintain account master data
        return Task.FromResult(new EtlSyncResult
        {
            SourceSystem = SourceSystemName,
            EntityType = "accounts",
            Success = true,
            RecordsRead = 0,
            RecordsInserted = 0,
            StartedAt = DateTimeOffset.UtcNow,
            EndedAt = DateTimeOffset.UtcNow
        });
    }

    /// <summary>
    /// Sync transaction_history table → staging.transactions + warehouse.fact_transactions
    /// WekezaBank transaction_history schema:
    ///   id, transaction_id (unique), customer_id, account_number, amount, currency,
    ///   transaction_type (TRANSFER/PAYMENT/WITHDRAWAL/DEPOSIT), merchant_name, merchant_category,
    ///   location, channel (MOBILE/ONLINE/ATM/BRANCH), timestamp, status (PENDING/APPROVED/REJECTED/BLOCKED)
    /// </summary>
    public async Task<EtlSyncResult> SyncTransactionsAsync(DateTimeOffset? since = null, CancellationToken ct = default)
    {
        var result = new EtlSyncResult
        {
            SourceSystem = SourceSystemName,
            EntityType = "transactions",
            StartedAt = DateTimeOffset.UtcNow
        };

        try
        {
            await using var conn = new NpgsqlConnection(_sourceConnectionString);
            await conn.OpenAsync(ct);

            var sql = since.HasValue
                ? "SELECT * FROM transaction_history WHERE timestamp > @since ORDER BY timestamp ASC LIMIT 5000"
                : "SELECT * FROM transaction_history ORDER BY timestamp ASC LIMIT 5000";

            var rows = (await conn.QueryAsync(sql, new { since = since?.UtcDateTime })).AsList();
            result.RecordsRead = rows.Count;

            foreach (var row in rows)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    var sourceId = (string)row.transaction_id;
                    var customerId = (string?)row.customer_id;
                    var txnDate = DateTimeOffset.FromUnixTimeSeconds(0).AddTicks(
                        ((DateTime)row.timestamp).ToUniversalTime().Ticks);

                    // Resolve GCID from customer_id
                    var gcid = await ResolveGcidFromCustomerIdAsync(customerId, ct);
                    if (gcid == Guid.Empty)
                    {
                        result.RecordsSkipped++;
                        continue;
                    }

                    // Find account
                    var accountId = await ResolveAccountIdAsync((string?)row.account_number, ct);

                    var exists = await _hub.FactTransactions.AnyAsync(
                        t => t.SourceSystem == SourceSystemName && t.SourceTransactionId == sourceId, ct);

                    if (!exists)
                    {
                        _hub.FactTransactions.Add(new FactTransaction
                        {
                            SourceSystem = SourceSystemName,
                            SourceTransactionId = sourceId,
                            Gcid = gcid,
                            AccountId = accountId,
                            TransactionDate = txnDate,
                            TransactionType = MapTransactionType((string?)row.transaction_type),
                            Amount = (decimal)row.amount,
                            Currency = (string?)row.currency ?? "KES",
                            Channel = MapChannel((string?)row.channel),
                            MerchantName = (string?)row.merchant_name,
                            MerchantCategory = (string?)row.merchant_category,
                            Location = (string?)row.location,
                            Status = ((string?)row.status)?.ToLower(),
                            DateKey = int.Parse(txnDate.ToString("yyyyMMdd"))
                        });
                        result.RecordsInserted++;
                    }
                    else
                    {
                        result.RecordsSkipped++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Skipping WekezaBank transaction row due to error");
                    result.RecordsSkipped++;
                }
            }

            await _hub.SaveChangesAsync(ct);
            result.Success = true;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "WekezaBank transactions ETL failed");
        }
        finally
        {
            result.EndedAt = DateTimeOffset.UtcNow;
        }

        return result;
    }

    /// <summary>
    /// Sync analyst_cases table → warehouse.fact_risk_assessments
    /// WekezaBank analyst_cases schema:
    ///   case_id, transaction_id, customer_id, amount, currency, merchant_name, transaction_type,
    ///   risk_score (0-1), risk_level (LOW/MEDIUM/HIGH), status (ASSIGNED/REVIEWED/CLOSED),
    ///   analyst_id, analyst_comment, flagged_reason, created_at, updated_at, closed_at
    /// </summary>
    public async Task<EtlSyncResult> SyncRiskAssessmentsAsync(DateTimeOffset? since = null, CancellationToken ct = default)
    {
        var result = new EtlSyncResult
        {
            SourceSystem = SourceSystemName,
            EntityType = "risk_assessments",
            StartedAt = DateTimeOffset.UtcNow
        };

        try
        {
            await using var conn = new NpgsqlConnection(_sourceConnectionString);
            await conn.OpenAsync(ct);

            var sql = since.HasValue
                ? "SELECT * FROM analyst_cases WHERE created_at > @since ORDER BY created_at ASC LIMIT 5000"
                : "SELECT * FROM analyst_cases ORDER BY created_at ASC LIMIT 5000";

            var rows = (await conn.QueryAsync(sql, new { since = since?.UtcDateTime })).AsList();
            result.RecordsRead = rows.Count;

            foreach (var row in rows)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    var sourceTransactionId = (string)row.transaction_id;
                    var customerId = (string?)row.customer_id;
                    var gcid = await ResolveGcidFromCustomerIdAsync(customerId, ct);

                    if (gcid == Guid.Empty)
                    {
                        result.RecordsSkipped++;
                        continue;
                    }

                    var exists = await _hub.FactRiskAssessments.AnyAsync(
                        r => r.SourceSystem == SourceSystemName && r.SourceTransactionId == sourceTransactionId, ct);

                    if (!exists)
                    {
                        var assessedAt = DateTimeOffset.FromUnixTimeSeconds(0).AddTicks(
                            ((DateTime)row.created_at).ToUniversalTime().Ticks);

                        _hub.FactRiskAssessments.Add(new FactRiskAssessment
                        {
                            SourceSystem = SourceSystemName,
                            SourceCaseId = (int?)row.case_id,
                            SourceTransactionId = sourceTransactionId,
                            Gcid = gcid,
                            Amount = (decimal?)row.amount,
                            Currency = (string?)row.currency ?? "KES",
                            RiskScore = (decimal)(double)row.risk_score,
                            RiskLevel = (string)row.risk_level,
                            FlaggedReasons = row.flagged_reason != null ? new[] { (string)row.flagged_reason } : null,
                            Outcome = MapRiskOutcome((string?)row.status),
                            AnalystReviewed = row.analyst_comment != null,
                            AssessedAt = assessedAt,
                            ClosedAt = row.closed_at != null
                                ? DateTimeOffset.FromUnixTimeSeconds(0).AddTicks(((DateTime)row.closed_at).ToUniversalTime().Ticks)
                                : null,
                            DateKey = int.Parse(assessedAt.ToString("yyyyMMdd"))
                        });
                        result.RecordsInserted++;
                    }
                    else
                    {
                        result.RecordsSkipped++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Skipping WekezaBank risk assessment row");
                    result.RecordsSkipped++;
                }
            }

            await _hub.SaveChangesAsync(ct);
            result.Success = true;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "WekezaBank risk assessments ETL failed");
        }
        finally
        {
            result.EndedAt = DateTimeOffset.UtcNow;
        }

        return result;
    }

    // Helper: Resolve GCID from WekezaBank customer_id (which is just a reference string)
    private async Task<Guid> ResolveGcidFromCustomerIdAsync(string? customerId, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(customerId)) return Guid.Empty;

        var existing = await _hub.DimCustomers
            .FirstOrDefaultAsync(c => c.RiskSystemId == customerId, ct);

        if (existing != null) return existing.Gcid;

        // Create placeholder customer record for identity resolution later
        var customer = new DimCustomer
        {
            Gcid = Guid.NewGuid(),
            RiskSystemId = customerId,
            OverallKycStatus = "unknown",
            OverallRiskLevel = "unknown",
            FirstSeenAt = DateTimeOffset.UtcNow,
            SourcesCount = 1
        };
        _hub.DimCustomers.Add(customer);
        await _hub.SaveChangesAsync(ct);
        return customer.Gcid;
    }

    private async Task<Guid?> ResolveAccountIdAsync(string? accountNumber, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(accountNumber)) return null;
        var account = await _hub.DimAccounts
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber, ct);
        return account?.Id;
    }

    private static string MapTransactionType(string? type) => type?.ToUpper() switch
    {
        "TRANSFER"   => "Transfer",
        "PAYMENT"    => "Debit",
        "WITHDRAWAL" => "Debit",
        "DEPOSIT"    => "Credit",
        _            => type ?? "Unknown"
    };

    private static string MapChannel(string? channel) => channel?.ToUpper() switch
    {
        "MOBILE" => "Mobile",
        "ONLINE" => "Web",
        "ATM"    => "ATM",
        "BRANCH" => "Branch",
        _        => channel ?? "Unknown"
    };

    private static string MapRiskOutcome(string? status) => status?.ToUpper() switch
    {
        "ASSIGNED" => "UNDER_REVIEW",
        "REVIEWED" => "REVIEWED",
        "CLOSED"   => "CLOSED",
        _          => status ?? "UNKNOWN"
    };
}
