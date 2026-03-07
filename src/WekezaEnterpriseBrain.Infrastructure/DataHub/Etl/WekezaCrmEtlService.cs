using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Dapper;
using Microsoft.Data.SqlClient;
using WekezaEnterpriseBrain.Infrastructure.DataHub.Models;

namespace WekezaEnterpriseBrain.Infrastructure.DataHub.Etl;

/// <summary>
/// ETL service for WekezaCRM (SQL Server database 'WekezaCRM').
/// Source: eodenyire/WekezaCRM - C# .NET 8 + Entity Framework Core + SQL Server
/// Tables: Customers, Accounts, Transactions, Cases, CaseNotes, Interactions,
///         SentimentAnalyses, WhatsAppMessages, USSDSessions, Campaigns,
///         NextBestActions, WorkflowInstances, Notifications
/// Pattern: Repository pattern - direct database reads
/// </summary>
public class WekezaCrmEtlService : IEtlService
{
    private readonly WekezaDataHubDbContext _hub;
    private readonly ILogger<WekezaCrmEtlService> _logger;
    private readonly string _sourceConnectionString;

    public string SourceSystemName => "WekezaCRM";

    public WekezaCrmEtlService(
        WekezaDataHubDbContext hub,
        ILogger<WekezaCrmEtlService> logger,
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
            await using var conn = new SqlConnection(_sourceConnectionString);
            await conn.OpenAsync(ct);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "WekezaCRM connection test failed");
            return false;
        }
    }

    public async Task<IEnumerable<EtlSyncResult>> SyncAllAsync(DateTimeOffset? since = null, CancellationToken ct = default)
    {
        return new[]
        {
            await SyncCustomersAsync(since, ct),
            await SyncAccountsAsync(since, ct),
            await SyncTransactionsAsync(since, ct),
            await SyncInteractionsAsync(since, ct),
            await SyncCasesAsync(since, ct)
        };
    }

    /// <summary>
    /// Sync CRM Customers → warehouse.dim_customers
    /// WekezaCRM Customer entity: Id (Guid), FirstName, LastName, Email (unique), PhoneNumber,
    ///   DateOfBirth, Address, City, Country, Segment (enum), KYCStatus (enum),
    ///   CustomerReference (unique), CreditScore (decimal 18,2), LifetimeValue (decimal 18,2),
    ///   RiskScore (int), IsActive, CreatedAt, UpdatedAt
    /// </summary>
    public async Task<EtlSyncResult> SyncCustomersAsync(DateTimeOffset? since = null, CancellationToken ct = default)
    {
        var result = new EtlSyncResult
        {
            SourceSystem = SourceSystemName,
            EntityType = "customers",
            StartedAt = DateTimeOffset.UtcNow
        };

        try
        {
            await using var conn = new SqlConnection(_sourceConnectionString);
            await conn.OpenAsync(ct);

            var sql = since.HasValue
                ? "SELECT * FROM Customers WHERE UpdatedAt > @since ORDER BY CreatedAt ASC"
                : "SELECT * FROM Customers ORDER BY CreatedAt ASC";

            var rows = (await conn.QueryAsync(sql, new { since = since?.UtcDateTime })).AsList();
            result.RecordsRead = rows.Count;

            foreach (var row in rows)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    var sourceId = ((Guid)row.Id).ToString();
                    var email = (string?)row.Email;
                    var phone = (string?)row.PhoneNumber;

                    // Identity resolution
                    var gcid = await TryResolveGcidAsync(email, phone, null, ct);

                    if (gcid.HasValue)
                    {
                        var existing = await _hub.DimCustomers.FirstAsync(c => c.Gcid == gcid.Value, ct);
                        existing.CrmId = sourceId;
                        existing.CreditScore = (decimal?)row.CreditScore;
                        existing.LifetimeValue = (decimal?)row.LifetimeValue;
                        existing.CustomerSegment = row.Segment?.ToString();
                        existing.OverallKycStatus = BestKycStatus(existing.OverallKycStatus, MapKycStatus((int?)row.KYCStatus));
                        if (string.IsNullOrEmpty(existing.City) && row.City != null)
                            existing.City = (string)row.City;
                        existing.SourcesCount++;
                        existing.UpdatedAt = DateTimeOffset.UtcNow;
                        result.RecordsUpdated++;
                    }
                    else
                    {
                        _hub.DimCustomers.Add(new DimCustomer
                        {
                            Gcid = Guid.NewGuid(),
                            CrmId = sourceId,
                            FirstName = (string?)row.FirstName,
                            LastName = (string?)row.LastName,
                            PrimaryEmail = email?.ToLower().Trim(),
                            PrimaryPhone = phone,
                            DateOfBirth = row.DateOfBirth != null ? DateOnly.FromDateTime((DateTime)row.DateOfBirth) : null,
                            Address = (string?)row.Address,
                            City = (string?)row.City,
                            Country = (string?)row.Country ?? "KE",
                            OverallKycStatus = MapKycStatus((int?)row.KYCStatus),
                            CreditScore = (decimal?)row.CreditScore,
                            LifetimeValue = (decimal?)row.LifetimeValue,
                            CustomerSegment = row.Segment?.ToString(),
                            FirstSeenAt = DateTimeOffset.FromUnixTimeSeconds(0).AddTicks(((DateTime)row.CreatedAt).ToUniversalTime().Ticks),
                            SourcesCount = 1,
                            IsActive = (bool?)row.IsActive ?? true
                        });
                        result.RecordsInserted++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Skipping WekezaCRM customer row");
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
            _logger.LogError(ex, "WekezaCRM customers ETL failed");
        }
        finally
        {
            result.EndedAt = DateTimeOffset.UtcNow;
        }

        return result;
    }

    /// <summary>
    /// Sync CRM Accounts → warehouse.dim_accounts
    /// WekezaCRM Account: Id (Guid), CustomerId (Guid FK), AccountNumber (unique),
    ///   AccountType, Balance (decimal 18,2), Currency (default KES), IsActive, ClosedAt
    /// </summary>
    public async Task<EtlSyncResult> SyncAccountsAsync(DateTimeOffset? since = null, CancellationToken ct = default)
    {
        var result = new EtlSyncResult
        {
            SourceSystem = SourceSystemName,
            EntityType = "accounts",
            StartedAt = DateTimeOffset.UtcNow
        };

        try
        {
            await using var conn = new SqlConnection(_sourceConnectionString);
            await conn.OpenAsync(ct);

            var sql = since.HasValue
                ? "SELECT a.*, c.Email, c.PhoneNumber FROM Accounts a JOIN Customers c ON a.CustomerId = c.Id WHERE a.UpdatedAt > @since ORDER BY a.CreatedAt ASC"
                : "SELECT a.*, c.Email, c.PhoneNumber FROM Accounts a JOIN Customers c ON a.CustomerId = c.Id ORDER BY a.CreatedAt ASC";

            var rows = (await conn.QueryAsync(sql, new { since = since?.UtcDateTime })).AsList();
            result.RecordsRead = rows.Count;

            foreach (var row in rows)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    var sourceAccountId = ((Guid)row.Id).ToString();
                    var email = (string?)row.Email;
                    var gcid = await TryResolveGcidAsync(email, (string?)row.PhoneNumber, null, ct);
                    if (!gcid.HasValue) { result.RecordsSkipped++; continue; }

                    var exists = await _hub.DimAccounts.AnyAsync(
                        a => a.SourceSystem == SourceSystemName && a.SourceAccountId == sourceAccountId, ct);

                    if (!exists)
                    {
                        _hub.DimAccounts.Add(new DimAccount
                        {
                            SourceSystem = SourceSystemName,
                            SourceAccountId = sourceAccountId,
                            Gcid = gcid.Value,
                            AccountNumber = (string)row.AccountNumber,
                            AccountType = (string?)row.AccountType,
                            Currency = (string?)row.Currency ?? "KES",
                            CurrentBalance = (decimal?)row.Balance,
                            AvailableBalance = (decimal?)row.Balance,
                            Status = (bool?)row.IsActive == true ? "Active" : "Closed",
                            ClosedDate = row.ClosedAt != null ? DateOnly.FromDateTime((DateTime)row.ClosedAt) : null
                        });
                        result.RecordsInserted++;
                    }
                    else
                    {
                        var existing = await _hub.DimAccounts.FirstAsync(
                            a => a.SourceSystem == SourceSystemName && a.SourceAccountId == sourceAccountId, ct);
                        existing.CurrentBalance = (decimal?)row.Balance;
                        existing.Status = (bool?)row.IsActive == true ? "Active" : "Closed";
                        existing.UpdatedAt = DateTimeOffset.UtcNow;
                        result.RecordsUpdated++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Skipping WekezaCRM account row");
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
            _logger.LogError(ex, "WekezaCRM accounts ETL failed");
        }
        finally
        {
            result.EndedAt = DateTimeOffset.UtcNow;
        }

        return result;
    }

    /// <summary>
    /// Sync CRM Transactions → warehouse.fact_transactions
    /// WekezaCRM Transaction: Id (Guid), AccountId (Guid FK), TransactionReference (unique),
    ///   TransactionType, Amount (decimal 18,2), BalanceAfter (decimal 18,2), TransactionDate, Description
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
            await using var conn = new SqlConnection(_sourceConnectionString);
            await conn.OpenAsync(ct);

            var sql = since.HasValue
                ? @"SELECT TOP 10000 t.*, a.AccountNumber, c.Email FROM Transactions t
                    JOIN Accounts a ON t.AccountId = a.Id
                    JOIN Customers c ON a.CustomerId = c.Id
                    WHERE t.TransactionDate > @since ORDER BY t.TransactionDate ASC"
                : @"SELECT t.*, a.AccountNumber, c.Email FROM Transactions t
                    JOIN Accounts a ON t.AccountId = a.Id
                    JOIN Customers c ON a.CustomerId = c.Id ORDER BY t.TransactionDate ASC";

            var rows = (await conn.QueryAsync(sql, new { since = since?.UtcDateTime })).AsList();
            result.RecordsRead = rows.Count;

            foreach (var row in rows)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    var sourceId = ((Guid)row.Id).ToString();
                    var gcid = await TryResolveGcidAsync((string?)row.Email, null, null, ct);
                    if (!gcid.HasValue) { result.RecordsSkipped++; continue; }

                    var accountNumber = (string?)row.AccountNumber;
                    var account = accountNumber != null
                        ? await _hub.DimAccounts
                            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber
                                                      && a.SourceSystem == SourceSystemName, ct)
                        : null;

                    var txnDate = DateTimeOffset.FromUnixTimeSeconds(0).AddTicks(
                        ((DateTime)row.TransactionDate).ToUniversalTime().Ticks);

                    var exists = await _hub.FactTransactions.AnyAsync(
                        t => t.SourceSystem == SourceSystemName && t.SourceTransactionId == sourceId, ct);

                    if (!exists)
                    {
                        _hub.FactTransactions.Add(new FactTransaction
                        {
                            SourceSystem = SourceSystemName,
                            SourceTransactionId = sourceId,
                            Gcid = gcid.Value,
                            AccountId = account?.Id,
                            TransactionDate = txnDate,
                            TransactionType = (string?)row.TransactionType,
                            Amount = (decimal)row.Amount,
                            Currency = "KES",
                            Description = (string?)row.Description,
                            Reference = (string?)row.TransactionReference,
                            BalanceAfter = (decimal?)row.BalanceAfter,
                            Status = "completed",
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
                    _logger.LogWarning(ex, "Skipping WekezaCRM transaction row");
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
            _logger.LogError(ex, "WekezaCRM transactions ETL failed");
        }
        finally
        {
            result.EndedAt = DateTimeOffset.UtcNow;
        }

        return result;
    }

    /// <summary>
    /// Sync Interactions + SentimentAnalysis → warehouse.fact_interactions
    /// WekezaCRM Interaction: Id, CustomerId, Channel (enum), Subject, Description, InteractionDate, DurationMinutes
    /// WekezaCRM SentimentAnalysis: CustomerId, InteractionId, SentimentType, SentimentScore, KeyPhrases
    /// </summary>
    public async Task<EtlSyncResult> SyncInteractionsAsync(DateTimeOffset? since = null, CancellationToken ct = default)
    {
        var result = new EtlSyncResult
        {
            SourceSystem = SourceSystemName,
            EntityType = "interactions",
            StartedAt = DateTimeOffset.UtcNow
        };

        try
        {
            await using var conn = new SqlConnection(_sourceConnectionString);
            await conn.OpenAsync(ct);

            var sql = since.HasValue
                ? @"SELECT i.*, c.Email, sa.SentimentType, sa.SentimentScore, sa.KeyPhrases
                    FROM Interactions i
                    JOIN Customers c ON i.CustomerId = c.Id
                    LEFT JOIN SentimentAnalyses sa ON sa.InteractionId = i.Id
                    WHERE i.InteractionDate > @since ORDER BY i.InteractionDate ASC"
                : @"SELECT i.*, c.Email, sa.SentimentType, sa.SentimentScore, sa.KeyPhrases
                    FROM Interactions i
                    JOIN Customers c ON i.CustomerId = c.Id
                    LEFT JOIN SentimentAnalyses sa ON sa.InteractionId = i.Id
                    ORDER BY i.InteractionDate ASC";

            var rows = (await conn.QueryAsync(sql, new { since = since?.UtcDateTime })).AsList();
            result.RecordsRead = rows.Count;

            foreach (var row in rows)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    var sourceId = ((Guid)row.Id).ToString();
                    var gcid = await TryResolveGcidAsync((string?)row.Email, null, null, ct);
                    if (!gcid.HasValue) { result.RecordsSkipped++; continue; }

                    var exists = await _hub.FactInteractions.AnyAsync(
                        i => i.SourceSystem == SourceSystemName && i.SourceInteractionId == sourceId, ct);

                    if (!exists)
                    {
                        var interactionDate = DateTimeOffset.FromUnixTimeSeconds(0).AddTicks(
                            ((DateTime)row.InteractionDate).ToUniversalTime().Ticks);

                        _hub.FactInteractions.Add(new FactInteraction
                        {
                            SourceSystem = SourceSystemName,
                            SourceInteractionId = sourceId,
                            Gcid = gcid.Value,
                            InteractionType = MapInteractionChannel((int?)row.Channel),
                            Channel = MapInteractionChannel((int?)row.Channel),
                            Subject = (string?)row.Subject,
                            SentimentType = row.SentimentType?.ToString(),
                            SentimentScore = (decimal?)row.SentimentScore,
                            KeyPhrases = (string?)row.KeyPhrases,
                            DurationMinutes = (int?)row.DurationMinutes,
                            InteractionDate = interactionDate,
                            DateKey = int.Parse(interactionDate.ToString("yyyyMMdd"))
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
                    _logger.LogWarning(ex, "Skipping WekezaCRM interaction row");
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
            _logger.LogError(ex, "WekezaCRM interactions ETL failed");
        }
        finally
        {
            result.EndedAt = DateTimeOffset.UtcNow;
        }

        return result;
    }

    /// <summary>
    /// Sync Cases → warehouse.fact_cases
    /// WekezaCRM Case: Id, CustomerId, CaseNumber (unique), Title, Description,
    ///   Status (enum: Open/InProgress/Resolved/Closed), Priority (enum), Category,
    ///   SubCategory, SLADurationHours, OpenedAt, ResolvedAt, ClosedAt, Resolution
    /// </summary>
    public async Task<EtlSyncResult> SyncCasesAsync(DateTimeOffset? since = null, CancellationToken ct = default)
    {
        var result = new EtlSyncResult
        {
            SourceSystem = SourceSystemName,
            EntityType = "cases",
            StartedAt = DateTimeOffset.UtcNow
        };

        try
        {
            await using var conn = new SqlConnection(_sourceConnectionString);
            await conn.OpenAsync(ct);

            var sql = since.HasValue
                ? @"SELECT ca.*, c.Email FROM Cases ca JOIN Customers c ON ca.CustomerId = c.Id WHERE ca.CreatedAt > @since ORDER BY ca.CreatedAt ASC"
                : @"SELECT ca.*, c.Email FROM Cases ca JOIN Customers c ON ca.CustomerId = c.Id ORDER BY ca.CreatedAt ASC";

            var rows = (await conn.QueryAsync(sql, new { since = since?.UtcDateTime })).AsList();
            result.RecordsRead = rows.Count;

            foreach (var row in rows)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    var sourceId = ((Guid)row.Id).ToString();
                    var gcid = await TryResolveGcidAsync((string?)row.Email, null, null, ct);
                    if (!gcid.HasValue) { result.RecordsSkipped++; continue; }

                    var openedAt = DateTimeOffset.FromUnixTimeSeconds(0).AddTicks(
                        ((DateTime)row.CreatedAt).ToUniversalTime().Ticks);

                    var exists = await _hub.FactCases.AnyAsync(
                        fc => fc.SourceSystem == SourceSystemName && fc.SourceCaseId == sourceId, ct);

                    if (!exists)
                    {
                        var resolvedAt = row.ResolvedAt != null
                            ? DateTimeOffset.FromUnixTimeSeconds(0).AddTicks(((DateTime)row.ResolvedAt).ToUniversalTime().Ticks)
                            : (DateTimeOffset?)null;

                        decimal? resHours = resolvedAt.HasValue
                            ? (decimal)(resolvedAt.Value - openedAt).TotalHours
                            : null;

                        _hub.FactCases.Add(new FactCase
                        {
                            SourceSystem = SourceSystemName,
                            SourceCaseId = sourceId,
                            Gcid = gcid.Value,
                            CaseNumber = (string?)row.CaseNumber,
                            Title = (string?)row.Title,
                            Category = (string?)row.Category,
                            SubCategory = (string?)row.SubCategory,
                            Status = row.Status?.ToString(),
                            Priority = row.Priority?.ToString(),
                            SlaHours = (int?)row.SLADurationHours,
                            ResolutionHours = resHours,
                            IsSlaBreached = resHours.HasValue && row.SLADurationHours != null
                                ? resHours > (int)row.SLADurationHours
                                : null,
                            OpenedAt = openedAt,
                            ResolvedAt = resolvedAt,
                            ClosedAt = row.ClosedAt != null
                                ? DateTimeOffset.FromUnixTimeSeconds(0).AddTicks(((DateTime)row.ClosedAt).ToUniversalTime().Ticks)
                                : null,
                            DateKey = int.Parse(openedAt.ToString("yyyyMMdd"))
                        });
                        result.RecordsInserted++;
                    }
                    else
                    {
                        // Update status for open cases
                        var existing = await _hub.FactCases.FirstAsync(
                            fc => fc.SourceSystem == SourceSystemName && fc.SourceCaseId == sourceId, ct);
                        existing.Status = row.Status?.ToString();
                        result.RecordsUpdated++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Skipping WekezaCRM case row");
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
            _logger.LogError(ex, "WekezaCRM cases ETL failed");
        }
        finally
        {
            result.EndedAt = DateTimeOffset.UtcNow;
        }

        return result;
    }

    private async Task<Guid?> TryResolveGcidAsync(string? email, string? phone, string? nationalId, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(email))
        {
            var c = await _hub.DimCustomers.FirstOrDefaultAsync(
                x => x.PrimaryEmail == email.ToLower().Trim(), ct);
            if (c != null) return c.Gcid;
        }
        if (!string.IsNullOrEmpty(phone))
        {
            var c = await _hub.DimCustomers.FirstOrDefaultAsync(x => x.PrimaryPhone == phone, ct);
            if (c != null) return c.Gcid;
        }
        return null;
    }

    private static string BestKycStatus(string current, string incoming)
    {
        var hierarchy = new[] { "verified", "pending", "rejected", "unknown" };
        var ci = Array.IndexOf(hierarchy, current.ToLower());
        var ii = Array.IndexOf(hierarchy, incoming.ToLower());
        return ii >= 0 && ii < ci ? incoming : current;
    }

    // WekezaCRM KYCStatus enum: 0=Pending, 1=Verified, 2=Rejected
    private static string MapKycStatus(int? status) => status switch
    {
        0 => "pending",
        1 => "verified",
        2 => "rejected",
        _ => "unknown"
    };

    // WekezaCRM InteractionChannel enum: 0=Phone, 1=Email, 2=SMS, 3=Chat
    private static string MapInteractionChannel(int? channel) => channel switch
    {
        0 => "Phone",
        1 => "Email",
        2 => "SMS",
        3 => "Chat",
        _ => "Other"
    };
}
