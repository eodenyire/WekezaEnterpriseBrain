using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Dapper;
using Npgsql;
using WekezaEnterpriseBrain.Infrastructure.DataHub.Models;

namespace WekezaEnterpriseBrain.Infrastructure.DataHub.Etl;

/// <summary>
/// ETL service for WekezaOpenBanking.
/// Source: PostgreSQL database 'wekeza_banking' (eodenyire/WekezaOpenBanking)
/// Tables: customers, accounts, transactions, payments, oauth_tokens, webhook_deliveries
/// Pattern: Event-driven with webhooks (HMAC-SHA256, 7-retry exponential backoff)
/// </summary>
public class WekezaOpenBankingEtlService : IEtlService
{
    private readonly WekezaDataHubDbContext _hub;
    private readonly ILogger<WekezaOpenBankingEtlService> _logger;
    private readonly string _sourceConnectionString;

    public string SourceSystemName => "WekezaOpenBanking";

    public WekezaOpenBankingEtlService(
        WekezaDataHubDbContext hub,
        ILogger<WekezaOpenBankingEtlService> logger,
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
            await conn.ExecuteScalarAsync<bool>("SELECT EXISTS (SELECT 1 FROM customers)");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "WekezaOpenBanking connection test failed");
            return false;
        }
    }

    public async Task<IEnumerable<EtlSyncResult>> SyncAllAsync(DateTimeOffset? since = null, CancellationToken ct = default)
    {
        var results = new List<EtlSyncResult>
        {
            await SyncCustomersAsync(since, ct),
            await SyncAccountsAsync(since, ct),
            await SyncTransactionsAsync(since, ct),
            await SyncPaymentsAsync(since, ct)
        };
        return results;
    }

    /// <summary>
    /// Sync customers table → warehouse.dim_customers
    /// WekezaOpenBanking customers schema (PostgreSQL):
    ///   id (UUID), customer_number (unique), first_name, last_name, email (unique),
    ///   phone, date_of_birth, kyc_status, created_at, updated_at
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
            await using var conn = new NpgsqlConnection(_sourceConnectionString);
            await conn.OpenAsync(ct);

            var sql = since.HasValue
                ? "SELECT * FROM customers WHERE updated_at > @since ORDER BY created_at ASC"
                : "SELECT * FROM customers ORDER BY created_at ASC";

            var rows = (await conn.QueryAsync(sql, new { since = since?.UtcDateTime })).AsList();
            result.RecordsRead = rows.Count;

            foreach (var row in rows)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    var sourceId = ((Guid)row.id).ToString();
                    var email = (string?)row.email;
                    var phone = (string?)row.phone;
                    var customerNumber = (string)row.customer_number;

                    // Identity resolution: try to match existing customer
                    var gcid = await TryResolveGcidAsync(email, phone, null, ct);

                    if (gcid.HasValue)
                    {
                        // Update existing customer with OpenBanking ID
                        var existing = await _hub.DimCustomers.FirstAsync(c => c.Gcid == gcid.Value, ct);
                        existing.OpenBankingId = customerNumber;
                        if (string.IsNullOrEmpty(existing.PrimaryEmail) && email != null)
                            existing.PrimaryEmail = email.ToLower().Trim();
                        if (string.IsNullOrEmpty(existing.PrimaryPhone) && phone != null)
                            existing.PrimaryPhone = phone;
                        existing.OverallKycStatus = BestKycStatus(existing.OverallKycStatus, (string?)row.kyc_status);
                        existing.SourcesCount = Math.Max(existing.SourcesCount, 1) + 1;
                        existing.UpdatedAt = DateTimeOffset.UtcNow;
                        result.RecordsUpdated++;
                    }
                    else
                    {
                        // New customer
                        _hub.DimCustomers.Add(new DimCustomer
                        {
                            Gcid = Guid.NewGuid(),
                            OpenBankingId = customerNumber,
                            FirstName = (string?)row.first_name,
                            LastName = (string?)row.last_name,
                            PrimaryEmail = email?.ToLower().Trim(),
                            PrimaryPhone = phone,
                            DateOfBirth = row.date_of_birth != null ? DateOnly.FromDateTime((DateTime)row.date_of_birth) : null,
                            OverallKycStatus = (string?)row.kyc_status ?? "pending",
                            FirstSeenAt = DateTimeOffset.FromUnixTimeSeconds(0).AddTicks(((DateTime)row.created_at).ToUniversalTime().Ticks),
                            SourcesCount = 1
                        });
                        result.RecordsInserted++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Skipping OpenBanking customer row");
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
            _logger.LogError(ex, "WekezaOpenBanking customers ETL failed");
        }
        finally
        {
            result.EndedAt = DateTimeOffset.UtcNow;
        }

        return result;
    }

    /// <summary>
    /// Sync accounts table → warehouse.dim_accounts
    /// WekezaOpenBanking accounts schema:
    ///   id (UUID), account_number (unique), customer_id (FK→customers.id), account_type,
    ///   currency (default KES), balance, available_balance, status, created_at, updated_at
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
            await using var conn = new NpgsqlConnection(_sourceConnectionString);
            await conn.OpenAsync(ct);

            var sql = since.HasValue
                ? @"SELECT a.*, c.customer_number, c.email, c.phone FROM accounts a
                    JOIN customers c ON a.customer_id = c.id
                    WHERE a.updated_at > @since ORDER BY a.created_at ASC"
                : @"SELECT a.*, c.customer_number, c.email, c.phone FROM accounts a
                    JOIN customers c ON a.customer_id = c.id ORDER BY a.created_at ASC";

            var rows = (await conn.QueryAsync(sql, new { since = since?.UtcDateTime })).AsList();
            result.RecordsRead = rows.Count;

            foreach (var row in rows)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    var sourceAccountId = ((Guid)row.id).ToString();
                    var customerNumber = (string)row.customer_number;

                    // Find GCID by open_banking_id
                    var customer = await _hub.DimCustomers
                        .FirstOrDefaultAsync(c => c.OpenBankingId == customerNumber, ct);
                    if (customer == null) { result.RecordsSkipped++; continue; }

                    var exists = await _hub.DimAccounts.AnyAsync(
                        a => a.SourceSystem == SourceSystemName && a.SourceAccountId == sourceAccountId, ct);

                    if (!exists)
                    {
                        _hub.DimAccounts.Add(new DimAccount
                        {
                            SourceSystem = SourceSystemName,
                            SourceAccountId = sourceAccountId,
                            Gcid = customer.Gcid,
                            AccountNumber = (string)row.account_number,
                            AccountType = (string?)row.account_type,
                            Currency = (string?)row.currency ?? "KES",
                            CurrentBalance = (decimal?)row.balance,
                            AvailableBalance = (decimal?)row.available_balance,
                            Status = (string?)row.status,
                            OpenedDate = DateOnly.FromDateTime((DateTime)row.created_at)
                        });
                        result.RecordsInserted++;
                    }
                    else
                    {
                        // Update balance
                        var existing = await _hub.DimAccounts.FirstAsync(
                            a => a.SourceSystem == SourceSystemName && a.SourceAccountId == sourceAccountId, ct);
                        existing.CurrentBalance = (decimal?)row.balance;
                        existing.AvailableBalance = (decimal?)row.available_balance;
                        existing.Status = (string?)row.status;
                        existing.UpdatedAt = DateTimeOffset.UtcNow;
                        result.RecordsUpdated++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Skipping OpenBanking account row");
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
            _logger.LogError(ex, "WekezaOpenBanking accounts ETL failed");
        }
        finally
        {
            result.EndedAt = DateTimeOffset.UtcNow;
        }

        return result;
    }

    /// <summary>
    /// Sync transactions table → warehouse.fact_transactions
    /// WekezaOpenBanking transactions schema:
    ///   id (UUID), transaction_ref (unique), account_id (FK), transaction_type,
    ///   amount, currency (KES), balance_after, description, status, transaction_date, created_at
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
                ? @"SELECT t.*, a.account_number, c.customer_number FROM transactions t
                    JOIN accounts a ON t.account_id = a.id
                    JOIN customers c ON a.customer_id = c.id
                    WHERE t.transaction_date > @since ORDER BY t.transaction_date ASC LIMIT 10000"
                : @"SELECT t.*, a.account_number, c.customer_number FROM transactions t
                    JOIN accounts a ON t.account_id = a.id
                    JOIN customers c ON a.customer_id = c.id
                    ORDER BY t.transaction_date ASC LIMIT 10000";

            var rows = (await conn.QueryAsync(sql, new { since = since?.UtcDateTime })).AsList();
            result.RecordsRead = rows.Count;

            foreach (var row in rows)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    var sourceId = ((Guid)row.id).ToString();
                    var customerNumber = (string)row.customer_number;
                    var accountNumber = (string)row.account_number;

                    var customer = await _hub.DimCustomers
                        .FirstOrDefaultAsync(c => c.OpenBankingId == customerNumber, ct);
                    if (customer == null) { result.RecordsSkipped++; continue; }

                    var account = await _hub.DimAccounts
                        .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber && a.SourceSystem == SourceSystemName, ct);

                    var txnDate = DateTimeOffset.FromUnixTimeSeconds(0).AddTicks(
                        ((DateTime)row.transaction_date).ToUniversalTime().Ticks);

                    var exists = await _hub.FactTransactions.AnyAsync(
                        t => t.SourceSystem == SourceSystemName && t.SourceTransactionId == sourceId, ct);

                    if (!exists)
                    {
                        _hub.FactTransactions.Add(new FactTransaction
                        {
                            SourceSystem = SourceSystemName,
                            SourceTransactionId = sourceId,
                            Gcid = customer.Gcid,
                            AccountId = account?.Id,
                            TransactionDate = txnDate,
                            TransactionType = (string?)row.transaction_type,
                            Amount = (decimal)row.amount,
                            Currency = (string?)row.currency ?? "KES",
                            Channel = "API",
                            Description = (string?)row.description,
                            Reference = (string?)row.transaction_ref,
                            BalanceAfter = (decimal?)row.balance_after,
                            Status = (string?)row.status,
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
                    _logger.LogWarning(ex, "Skipping OpenBanking transaction row");
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
            _logger.LogError(ex, "WekezaOpenBanking transactions ETL failed");
        }
        finally
        {
            result.EndedAt = DateTimeOffset.UtcNow;
        }

        return result;
    }

    /// <summary>
    /// Sync payments table → warehouse.fact_payments
    /// WekezaOpenBanking payments schema:
    ///   id (UUID), payment_ref (unique), source_account_id, destination_account_number,
    ///   amount, currency (KES), reference, description, status, risk_score,
    ///   idempotency_key, completed_at, created_at, updated_at
    /// </summary>
    public async Task<EtlSyncResult> SyncPaymentsAsync(DateTimeOffset? since = null, CancellationToken ct = default)
    {
        var result = new EtlSyncResult
        {
            SourceSystem = SourceSystemName,
            EntityType = "payments",
            StartedAt = DateTimeOffset.UtcNow
        };

        try
        {
            await using var conn = new NpgsqlConnection(_sourceConnectionString);
            await conn.OpenAsync(ct);

            var sql = since.HasValue
                ? @"SELECT p.*, a.account_number, c.customer_number, oc.name AS client_name
                    FROM payments p
                    JOIN accounts a ON p.source_account_id = a.id
                    JOIN customers c ON a.customer_id = c.id
                    LEFT JOIN oauth_tokens ot ON ot.user_id = c.id
                    LEFT JOIN oauth_clients oc ON ot.client_id = oc.client_id
                    WHERE p.created_at > @since ORDER BY p.created_at ASC LIMIT 5000"
                : @"SELECT p.*, a.account_number, c.customer_number, NULL AS client_name
                    FROM payments p
                    JOIN accounts a ON p.source_account_id = a.id
                    JOIN customers c ON a.customer_id = c.id
                    ORDER BY p.created_at ASC LIMIT 5000";

            var rows = (await conn.QueryAsync(sql, new { since = since?.UtcDateTime })).AsList();
            result.RecordsRead = rows.Count;

            foreach (var row in rows)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    var sourcePaymentId = (Guid)row.id;
                    var customerNumber = (string)row.customer_number;

                    var customer = await _hub.DimCustomers
                        .FirstOrDefaultAsync(c => c.OpenBankingId == customerNumber, ct);
                    if (customer == null) { result.RecordsSkipped++; continue; }

                    var acctNumber = (string?)row.account_number;
                    var account = acctNumber != null
                        ? await _hub.DimAccounts
                            .FirstOrDefaultAsync(a => a.AccountNumber == acctNumber
                                                      && a.SourceSystem == SourceSystemName, ct)
                        : null;

                    var exists = await _hub.FactPayments.AnyAsync(
                        p => p.SourceSystem == SourceSystemName && p.SourcePaymentId == sourcePaymentId, ct);

                    if (!exists)
                    {
                        var completedAt = row.completed_at != null
                            ? DateTimeOffset.FromUnixTimeSeconds(0).AddTicks(((DateTime)row.completed_at).ToUniversalTime().Ticks)
                            : (DateTimeOffset?)null;

                        _hub.FactPayments.Add(new FactPayment
                        {
                            SourceSystem = SourceSystemName,
                            SourcePaymentId = sourcePaymentId,
                            Gcid = customer.Gcid,
                            AccountId = account?.Id,
                            PaymentRef = (string)row.payment_ref,
                            DestinationAccountNumber = (string?)row.destination_account_number,
                            Amount = (decimal)row.amount,
                            Currency = (string?)row.currency ?? "KES",
                            Status = (string?)row.status,
                            RiskScore = (decimal?)(double?)row.risk_score,
                            OauthClientName = (string?)row.client_name,
                            CompletedAt = completedAt,
                            DateKey = completedAt.HasValue ? int.Parse(completedAt.Value.ToString("yyyyMMdd")) : null
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
                    _logger.LogWarning(ex, "Skipping OpenBanking payment row");
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
            _logger.LogError(ex, "WekezaOpenBanking payments ETL failed");
        }
        finally
        {
            result.EndedAt = DateTimeOffset.UtcNow;
        }

        return result;
    }

    // Identity resolution helper
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
            var c = await _hub.DimCustomers.FirstOrDefaultAsync(
                x => x.PrimaryPhone == phone, ct);
            if (c != null) return c.Gcid;
        }
        return null;
    }

    private static string BestKycStatus(string current, string? incoming)
    {
        // Hierarchy: verified > pending > unknown
        var hierarchy = new[] { "verified", "pending", "unknown" };
        var currentRank = Array.IndexOf(hierarchy, current.ToLower());
        var incomingRank = incoming != null ? Array.IndexOf(hierarchy, incoming.ToLower()) : int.MaxValue;
        return incomingRank < currentRank ? incoming! : current;
    }
}
