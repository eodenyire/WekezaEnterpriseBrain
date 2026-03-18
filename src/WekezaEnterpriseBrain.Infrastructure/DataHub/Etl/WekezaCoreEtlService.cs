using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Dapper;
using Npgsql;
using WekezaEnterpriseBrain.Infrastructure.DataHub.Models;

namespace WekezaEnterpriseBrain.Infrastructure.DataHub.Etl;

/// <summary>
/// ETL service for Wekeza Core Banking (ComprehensiveWekezaApi).
/// Source: PostgreSQL database 'WekezaCoreDB' (eodenyire/Wekeza - APIs/v3-Comprehensive)
/// Tables: "Customers", "Accounts", "Transactions" (EF Core Npgsql quoted PascalCase)
/// Schema: 18 banking modules - CIF, Accounts, Transactions, Loans, Teller, Branch,
///         Cards/ATM/POS, General Ledger, Payments, Products, Trade Finance, Treasury.
/// Pattern: Direct PostgreSQL polling with incremental watermark on UpdatedAt/ProcessedAt
/// </summary>
public class WekezaCoreEtlService : IEtlService
{
    private readonly WekezaDataHubDbContext _hub;
    private readonly ILogger<WekezaCoreEtlService> _logger;
    private readonly string _sourceConnectionString;

    public string SourceSystemName => "WekezaCore";

    public WekezaCoreEtlService(
        WekezaDataHubDbContext hub,
        ILogger<WekezaCoreEtlService> logger,
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
            await conn.ExecuteScalarAsync<bool>("SELECT EXISTS (SELECT 1 FROM \"Customers\")");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "WekezaCore connection test failed");
            return false;
        }
    }

    public async Task<IEnumerable<EtlSyncResult>> SyncAllAsync(
        DateTimeOffset? since = null, CancellationToken ct = default)
    {
        return new[]
        {
            await SyncCustomersAsync(since, ct),
            await SyncAccountsAsync(since, ct),
            await SyncTransactionsAsync(since, ct)
        };
    }

    /// <summary>
    /// Sync "Customers" → warehouse.dim_customers
    /// WekezaCore Customer schema:
    ///   Id (UUID), CustomerNumber, FirstName, MiddleName, LastName, Email, IdentificationNumber,
    ///   PrimaryPhone, SecondaryPhone, DateOfBirth, Gender, Nationality, KYCStatus, AMLRiskRating,
    ///   KYCCompletedAt, Status, CreatedAt, UpdatedAt
    /// </summary>
    public async Task<EtlSyncResult> SyncCustomersAsync(
        DateTimeOffset? since = null, CancellationToken ct = default)
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
                ? "SELECT * FROM \"Customers\" WHERE \"UpdatedAt\" > @since ORDER BY \"CreatedAt\" ASC LIMIT 5000"
                : "SELECT * FROM \"Customers\" ORDER BY \"CreatedAt\" ASC LIMIT 5000";

            var rows = (await conn.QueryAsync(sql, new { since = since?.UtcDateTime })).AsList();
            result.RecordsRead = rows.Count;

            foreach (var row in rows)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    var sourceId = ((Guid)row.Id).ToString();
                    var email = (string?)row.Email;
                    var phone = (string?)row.PrimaryPhone;
                    var nationalId = (string?)row.IdentificationNumber;

                    // Identity resolution: match existing customer by email, phone, or national ID
                    var existing = await FindExistingCustomerAsync(email, phone, nationalId, ct);

                    if (existing != null)
                    {
                        // Update existing customer with Core Banking ID
                        existing.CoreBankingId = ((string?)row.CustomerNumber) ?? sourceId;
                        if (string.IsNullOrEmpty(existing.FirstName))
                            existing.FirstName = (string?)row.FirstName;
                        if (string.IsNullOrEmpty(existing.LastName))
                            existing.LastName = (string?)row.LastName;
                        if (string.IsNullOrEmpty(existing.PrimaryEmail) && email != null)
                            existing.PrimaryEmail = email.ToLower().Trim();
                        if (string.IsNullOrEmpty(existing.PrimaryPhone) && phone != null)
                            existing.PrimaryPhone = phone;
                        if (string.IsNullOrEmpty(existing.NationalId) && nationalId != null)
                            existing.NationalId = nationalId;
                        existing.OverallKycStatus = BestKycStatus(existing.OverallKycStatus, (string?)row.KYCStatus);
                        existing.OverallRiskLevel = BestRiskLevel(existing.OverallRiskLevel, (string?)row.AMLRiskRating);
                        existing.SourcesCount++;
                        existing.UpdatedAt = DateTimeOffset.UtcNow;
                        result.RecordsUpdated++;
                    }
                    else
                    {
                        // New customer discovered in core banking
                        var dob = row.DateOfBirth != null
                            ? DateOnly.FromDateTime((DateTime)row.DateOfBirth)
                            : (DateOnly?)null;

                        _hub.DimCustomers.Add(new DimCustomer
                        {
                            Gcid = Guid.NewGuid(),
                            CoreBankingId = ((string?)row.CustomerNumber) ?? sourceId,
                            FirstName = (string?)row.FirstName,
                            LastName = (string?)row.LastName,
                            PrimaryEmail = email?.ToLower().Trim(),
                            PrimaryPhone = phone,
                            NationalId = nationalId,
                            DateOfBirth = dob,
                            Gender = (string?)row.Gender,
                            OverallKycStatus = (string?)row.KYCStatus ?? "Pending",
                            OverallRiskLevel = MapAmlRiskToRiskLevel((string?)row.AMLRiskRating),
                            FirstSeenAt = DateTimeOffset.FromUnixTimeSeconds(0).AddTicks(
                                ((DateTime)row.CreatedAt).ToUniversalTime().Ticks),
                            SourcesCount = 1,
                            IsActive = ((string?)row.Status) == "Active"
                        });
                        result.RecordsInserted++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Skipping WekezaCore customer row due to error");
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
            _logger.LogError(ex, "WekezaCore customers ETL failed");
        }
        finally
        {
            result.EndedAt = DateTimeOffset.UtcNow;
        }

        return result;
    }

    /// <summary>
    /// Sync "Accounts" → warehouse.dim_accounts
    /// WekezaCore Account schema:
    ///   Id (UUID), AccountNumber, CustomerId (FK), BusinessId, Currency, Balance, AvailableBalance,
    ///   OverdraftLimit, Status, AccountType, ProductCode, ProductName, InterestRate, MinimumBalance,
    ///   IsFrozen, IsClosed, CreatedAt, UpdatedAt
    /// </summary>
    public async Task<EtlSyncResult> SyncAccountsAsync(
        DateTimeOffset? since = null, CancellationToken ct = default)
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
                ? "SELECT a.*, c.\"CustomerNumber\", c.\"Email\" FROM \"Accounts\" a " +
                  "JOIN \"Customers\" c ON c.\"Id\" = a.\"CustomerId\" " +
                  "WHERE a.\"UpdatedAt\" > @since ORDER BY a.\"CreatedAt\" ASC LIMIT 5000"
                : "SELECT a.*, c.\"CustomerNumber\", c.\"Email\" FROM \"Accounts\" a " +
                  "JOIN \"Customers\" c ON c.\"Id\" = a.\"CustomerId\" " +
                  "ORDER BY a.\"CreatedAt\" ASC LIMIT 5000";

            var rows = (await conn.QueryAsync(sql, new { since = since?.UtcDateTime })).AsList();
            result.RecordsRead = rows.Count;

            foreach (var row in rows)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    var accountNumber = (string)row.AccountNumber;
                    var customerNumber = (string?)row.CustomerNumber;
                    var email = (string?)row.Email;

                    // Resolve GCID for this customer
                    var gcid = await ResolveGcidFromCoreBankingAsync(
                        customerNumber, email, ct);

                    if (gcid == Guid.Empty)
                    {
                        result.RecordsSkipped++;
                        continue;
                    }

                    var existingAccount = await _hub.DimAccounts
                        .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber, ct);

                    if (existingAccount == null)
                    {
                        _hub.DimAccounts.Add(new DimAccount
                        {
                            Id = Guid.NewGuid(),
                            Gcid = gcid,
                            SourceSystem = SourceSystemName,
                            SourceAccountId = ((Guid)row.Id).ToString(),
                            AccountNumber = accountNumber,
                            AccountType = (string?)row.AccountType ?? "Savings",
                            Currency = (string?)row.Currency ?? "KES",
                            CurrentBalance = (decimal)row.Balance,
                            AvailableBalance = (decimal)row.AvailableBalance,
                            Status = MapAccountStatus(row),
                            ProductName = (string?)row.ProductName,
                            OpenedDate = DateOnly.FromDateTime(((DateTime)row.CreatedAt).ToUniversalTime())
                        });
                        result.RecordsInserted++;
                    }
                    else
                    {
                        // Update balance and status
                        existingAccount.CurrentBalance = (decimal)row.Balance;
                        existingAccount.AvailableBalance = (decimal)row.AvailableBalance;
                        existingAccount.Status = MapAccountStatus(row);
                        result.RecordsUpdated++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Skipping WekezaCore account row due to error");
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
            _logger.LogError(ex, "WekezaCore accounts ETL failed");
        }
        finally
        {
            result.EndedAt = DateTimeOffset.UtcNow;
        }

        return result;
    }

    /// <summary>
    /// Sync "Transactions" → warehouse.fact_transactions
    /// WekezaCore Transaction schema:
    ///   Id (UUID), AccountId (FK), Type (Credit/Debit), Amount, Currency,
    ///   PreviousBalance, NewBalance, Status, Reference (unique), Description,
    ///   RelatedAccountNumber, ChequeNumber, ProcessedAt, ProcessedBy
    /// </summary>
    public async Task<EtlSyncResult> SyncTransactionsAsync(
        DateTimeOffset? since = null, CancellationToken ct = default)
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
                ? "SELECT t.*, a.\"AccountNumber\", a.\"CustomerId\", c.\"CustomerNumber\" " +
                  "FROM \"Transactions\" t " +
                  "JOIN \"Accounts\" a ON a.\"Id\" = t.\"AccountId\" " +
                  "JOIN \"Customers\" c ON c.\"Id\" = a.\"CustomerId\" " +
                  "WHERE t.\"ProcessedAt\" > @since ORDER BY t.\"ProcessedAt\" ASC LIMIT 5000"
                : "SELECT t.*, a.\"AccountNumber\", a.\"CustomerId\", c.\"CustomerNumber\" " +
                  "FROM \"Transactions\" t " +
                  "JOIN \"Accounts\" a ON a.\"Id\" = t.\"AccountId\" " +
                  "JOIN \"Customers\" c ON c.\"Id\" = a.\"CustomerId\" " +
                  "ORDER BY t.\"ProcessedAt\" ASC LIMIT 5000";

            var rows = (await conn.QueryAsync(sql, new { since = since?.UtcDateTime })).AsList();
            result.RecordsRead = rows.Count;

            foreach (var row in rows)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    var sourceId = ((Guid)row.Id).ToString();
                    var customerNumber = (string?)row.CustomerNumber;
                    var accountNumber = (string)row.AccountNumber;
                    var processedAt = DateTimeOffset.FromUnixTimeSeconds(0).AddTicks(
                        ((DateTime)row.ProcessedAt).ToUniversalTime().Ticks);

                    var gcid = await ResolveGcidFromCoreBankingAsync(customerNumber, null, ct);
                    if (gcid == Guid.Empty)
                    {
                        result.RecordsSkipped++;
                        continue;
                    }

                    var accountId = await ResolveAccountIdAsync(accountNumber, ct);

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
                            TransactionDate = processedAt,
                            TransactionType = MapTransactionType((string?)row.Type),
                            Amount = (decimal)row.Amount,
                            Currency = (string?)row.Currency ?? "KES",
                            Channel = "Branch",   // WekezaCore is primarily branch/teller operations
                            Description = (string?)row.Description,
                            Reference = (string?)row.Reference,
                            BalanceAfter = (decimal)row.NewBalance,
                            Status = ((string?)row.Status)?.ToLower(),
                            DateKey = int.Parse(processedAt.ToString("yyyyMMdd"))
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
                    _logger.LogWarning(ex, "Skipping WekezaCore transaction row due to error");
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
            _logger.LogError(ex, "WekezaCore transactions ETL failed");
        }
        finally
        {
            result.EndedAt = DateTimeOffset.UtcNow;
        }

        return result;
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private async Task<DimCustomer?> FindExistingCustomerAsync(
        string? email, string? phone, string? nationalId, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(email))
        {
            var byEmail = await _hub.DimCustomers
                .FirstOrDefaultAsync(c => c.PrimaryEmail == email.ToLower().Trim(), ct);
            if (byEmail != null) return byEmail;
        }

        if (!string.IsNullOrEmpty(phone))
        {
            var byPhone = await _hub.DimCustomers
                .FirstOrDefaultAsync(c => c.PrimaryPhone == phone, ct);
            if (byPhone != null) return byPhone;
        }

        if (!string.IsNullOrEmpty(nationalId))
        {
            var byNationalId = await _hub.DimCustomers
                .FirstOrDefaultAsync(c => c.NationalId == nationalId, ct);
            if (byNationalId != null) return byNationalId;
        }

        return null;
    }

    private async Task<Guid> ResolveGcidFromCoreBankingAsync(
        string? customerNumber, string? email, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(customerNumber))
        {
            var byCoreBankingId = await _hub.DimCustomers
                .FirstOrDefaultAsync(c => c.CoreBankingId == customerNumber, ct);
            if (byCoreBankingId != null) return byCoreBankingId.Gcid;
        }

        if (!string.IsNullOrEmpty(email))
        {
            var byEmail = await _hub.DimCustomers
                .FirstOrDefaultAsync(c => c.PrimaryEmail == email.ToLower().Trim(), ct);
            if (byEmail != null) return byEmail.Gcid;
        }

        // No existing customer found; create a placeholder so accounts and
        // transactions can be linked. Identity resolution will merge it later.
        if (string.IsNullOrEmpty(customerNumber))
            return Guid.Empty;

        var placeholder = new DimCustomer
        {
            Gcid = Guid.NewGuid(),
            CoreBankingId = customerNumber,
            OverallKycStatus = "unknown",
            OverallRiskLevel = "unknown",
            FirstSeenAt = DateTimeOffset.UtcNow,
            SourcesCount = 1
        };
        _hub.DimCustomers.Add(placeholder);
        await _hub.SaveChangesAsync(ct);
        return placeholder.Gcid;
    }

    private async Task<Guid?> ResolveAccountIdAsync(string accountNumber, CancellationToken ct)
    {
        var account = await _hub.DimAccounts
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber, ct);
        return account?.Id;
    }

    private static string MapTransactionType(string? type) => type?.ToLower() switch
    {
        "credit" => "Credit",
        "debit"  => "Debit",
        _        => type ?? "Unknown"
    };

    private static string MapAccountStatus(dynamic row)
    {
        if (row.IsClosed == true) return "Closed";
        if (row.IsFrozen == true) return "Frozen";
        return ((string?)row.Status) ?? "Active";
    }

    private static string BestKycStatus(string current, string? incoming)
    {
        if (string.IsNullOrEmpty(incoming)) return current;
        if (incoming.Equals("Full", StringComparison.OrdinalIgnoreCase) ||
            incoming.Equals("Verified", StringComparison.OrdinalIgnoreCase) ||
            incoming.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            return "Verified";
        if (current == "unknown" || current == "pending") return incoming;
        return current;
    }

    private static string BestRiskLevel(string current, string? incoming)
    {
        if (string.IsNullOrEmpty(incoming)) return current;
        var mapped = MapAmlRiskToRiskLevel(incoming);
        if (current == "unknown") return mapped;
        var priority = new[] { "HIGH", "MEDIUM", "LOW" };
        var ci = Array.IndexOf(priority, current.ToUpper());
        var ii = Array.IndexOf(priority, mapped.ToUpper());
        return ii < ci ? mapped : current;
    }

    private static string MapAmlRiskToRiskLevel(string? amlRating) => amlRating?.ToLower() switch
    {
        "high"   => "HIGH",
        "medium" => "MEDIUM",
        "low"    => "LOW",
        _        => "LOW"
    };
}
