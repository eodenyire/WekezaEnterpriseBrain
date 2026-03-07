using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WekezaEnterpriseBrain.Infrastructure.DataHub;
using WekezaEnterpriseBrain.Infrastructure.DataHub.Etl;

namespace WekezaEnterpriseBrain.Api.Controllers;

/// <summary>
/// Wekeza Main Datahub - Data warehouse management endpoints.
/// Manages ETL sync from all Wekeza source systems into the unified PostgreSQL datahub.
/// </summary>
[ApiController]
[Route("api/datahub")]
public class DataHubController : ControllerBase
{
    private readonly IDataHubOrchestrator _orchestrator;
    private readonly WekezaDataHubDbContext _hub;
    private readonly ILogger<DataHubController> _logger;

    public DataHubController(
        IDataHubOrchestrator orchestrator,
        WekezaDataHubDbContext hub,
        ILogger<DataHubController> logger)
    {
        _orchestrator = orchestrator;
        _hub = hub;
        _logger = logger;
    }

    /// <summary>Get datahub overview: source system status, record counts, last sync times</summary>
    [HttpGet]
    public async Task<IActionResult> GetOverview(CancellationToken ct)
    {
        var customerCount = await _hub.DimCustomers.CountAsync(ct);
        var accountCount = await _hub.DimAccounts.CountAsync(ct);
        var transactionCount = await _hub.FactTransactions.CountAsync(ct);
        var paymentCount = await _hub.FactPayments.CountAsync(ct);
        var riskCount = await _hub.FactRiskAssessments.CountAsync(ct);
        var interactionCount = await _hub.FactInteractions.CountAsync(ct);
        var caseCount = await _hub.FactCases.CountAsync(ct);

        var lastSyncs = await _orchestrator.GetLastSyncTimestampsAsync();

        var txnBySource = await _hub.FactTransactions
            .GroupBy(t => t.SourceSystem)
            .Select(g => new { SourceSystem = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        return Ok(new
        {
            name = "Wekeza Main Datahub",
            description = "Unified data warehouse consolidating all Wekeza banking systems",
            database = "PostgreSQL",
            warehouse = new
            {
                dim_customers = customerCount,
                dim_accounts = accountCount,
                fact_transactions = transactionCount,
                fact_payments = paymentCount,
                fact_risk_assessments = riskCount,
                fact_interactions = interactionCount,
                fact_cases = caseCount
            },
            transactionsBySource = txnBySource,
            lastSyncTimestamps = lastSyncs,
            sourceSystems = new[]
            {
                new { name = "WekezaBank",            type = "RiskManagement",  tech = "Python/SQLAlchemy", database = "risk_management (PostgreSQL)",   github = "https://github.com/eodenyire/WekezaBank" },
                new { name = "WekezaCRM",             type = "CRM",             tech = ".NET 8/EF Core",   database = "WekezaCRM (SQL Server)",           github = "https://github.com/eodenyire/WekezaCRM" },
                new { name = "WekezaOpenBanking",     type = "OpenBanking",     tech = "Node.js/pg",       database = "wekeza_banking (PostgreSQL)",       github = "https://github.com/eodenyire/WekezaOpenBanking" },
                new { name = "Wekeza",                type = "CoreBanking",     tech = ".NET 8/EF Core",   database = "WekezaCoreDB (PostgreSQL)",         github = "https://github.com/eodenyire/Wekeza" },
                new { name = "WekezaNextGenPersonal", type = "PersonalBanking", tech = ".NET Core",        database = "API Aggregation Layer",             github = "https://github.com/eodenyire/WekezaNextGenPersonalBanking" }
            },
            generatedAt = DateTimeOffset.UtcNow
        });
    }

    /// <summary>Trigger full ETL sync across all source systems</summary>
    [HttpPost("sync")]
    public async Task<IActionResult> SyncAll(
        [FromQuery] DateTimeOffset? since = null,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Starting full datahub sync. Since: {Since}", since);
        var results = await _orchestrator.SyncAllSystemsAsync(since, ct);
        var resultList = results.ToList();

        return Ok(new
        {
            syncedAt = DateTimeOffset.UtcNow,
            since = since,
            totalSystems = resultList.Select(r => r.SourceSystem).Distinct().Count(),
            totalResults = resultList.Count,
            succeeded = resultList.Count(r => r.Success),
            failed = resultList.Count(r => !r.Success),
            totalInserted = resultList.Sum(r => r.RecordsInserted),
            totalUpdated = resultList.Sum(r => r.RecordsUpdated),
            totalSkipped = resultList.Sum(r => r.RecordsSkipped),
            results = resultList.Select(r => new
            {
                r.SourceSystem,
                r.EntityType,
                r.Success,
                r.RecordsRead,
                r.RecordsInserted,
                r.RecordsUpdated,
                r.RecordsSkipped,
                r.ErrorMessage,
                durationMs = r.Duration.TotalMilliseconds
            })
        });
    }

    /// <summary>Trigger ETL sync for a specific source system</summary>
    [HttpPost("sync/{sourceSystem}")]
    public async Task<IActionResult> SyncSystem(
        string sourceSystem,
        [FromQuery] DateTimeOffset? since = null,
        CancellationToken ct = default)
    {
        var results = await _orchestrator.SyncSystemAsync(sourceSystem, since, ct);
        var resultList = results.ToList();

        return Ok(new
        {
            sourceSystem,
            syncedAt = DateTimeOffset.UtcNow,
            results = resultList
        });
    }

    /// <summary>Refresh analytics materialized views (customer_360, daily summaries, risk dashboard)</summary>
    [HttpPost("analytics/refresh")]
    public async Task<IActionResult> RefreshAnalytics(CancellationToken ct)
    {
        await _orchestrator.RefreshAnalyticsAsync(ct);
        return Ok(new { refreshedAt = DateTimeOffset.UtcNow, message = "Analytics views refreshed" });
    }

    /// <summary>Test connectivity to all source systems</summary>
    [HttpGet("connections")]
    public async Task<IActionResult> TestConnections(
        [FromServices] IEnumerable<IEtlService> etlServices,
        CancellationToken ct)
    {
        var tests = new List<object>();
        foreach (var etl in etlServices)
        {
            var connected = await etl.TestConnectionAsync(ct);
            tests.Add(new
            {
                sourceSystem = etl.SourceSystemName,
                connected,
                testedAt = DateTimeOffset.UtcNow
            });
        }

        return Ok(new
        {
            testedAt = DateTimeOffset.UtcNow,
            totalSystems = tests.Count,
            connected = tests.Count(t => (bool)((dynamic)t).connected),
            failed = tests.Count(t => !(bool)((dynamic)t).connected),
            results = tests
        });
    }

    /// <summary>Get customer 360 view from the datahub</summary>
    [HttpGet("customers/{gcid}")]
    public async Task<IActionResult> GetCustomer360(Guid gcid, CancellationToken ct)
    {
        var customer = await _hub.DimCustomers
            .Include(c => c.Accounts)
            .Include(c => c.Features)
            .FirstOrDefaultAsync(c => c.Gcid == gcid, ct);

        if (customer == null) return NotFound(new { error = $"Customer {gcid} not found in datahub" });

        var recentTransactions = await _hub.FactTransactions
            .Where(t => t.Gcid == gcid)
            .OrderByDescending(t => t.TransactionDate)
            .Take(10)
            .ToListAsync(ct);

        var riskSummary = await _hub.FactRiskAssessments
            .Where(r => r.Gcid == gcid)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                totalAssessments = g.Count(),
                highRiskCount = g.Count(r => r.RiskLevel == "HIGH"),
                avgRiskScore = g.Average(r => r.RiskScore)
            })
            .FirstOrDefaultAsync(ct);

        var openCases = await _hub.FactCases
            .Where(c => c.Gcid == gcid && c.Status != "Resolved" && c.Status != "Closed")
            .CountAsync(ct);

        return Ok(new
        {
            gcid = customer.Gcid,
            identity = new
            {
                customer.FirstName,
                customer.LastName,
                customer.PrimaryEmail,
                customer.PrimaryPhone,
                customer.NationalId,
                customer.DateOfBirth,
                customer.City,
                customer.Country
            },
            crossSystemIds = new
            {
                customer.CoreBankingId,
                customer.CrmId,
                customer.OpenBankingId,
                customer.RiskSystemId,
                customer.PersonalBankingId
            },
            intelligence = new
            {
                customer.OverallKycStatus,
                customer.OverallRiskLevel,
                customer.CreditScore,
                customer.LifetimeValue,
                customer.CustomerSegment,
                customer.FinancialPersonality,
                customer.StressLevel,
                customer.FinancialHealthScore,
                customer.SourcesCount
            },
            accounts = customer.Accounts.Select(a => new
            {
                a.AccountNumber,
                a.AccountType,
                a.Currency,
                a.CurrentBalance,
                a.Status,
                a.SourceSystem
            }),
            recentTransactions = recentTransactions.Select(t => new
            {
                t.TransactionDate,
                t.TransactionType,
                t.Amount,
                t.Currency,
                t.Channel,
                t.Description,
                t.AiCategory,
                t.SourceSystem
            }),
            riskSummary,
            openCases,
            features = customer.Features != null ? new
            {
                customer.Features.TxnCount30d,
                customer.Features.TotalSpend30d,
                customer.Features.TotalIncome30d,
                customer.Features.NetCashflow30d,
                customer.Features.AvgRiskScore30d,
                customer.Features.FraudFlags90d,
                customer.Features.InteractionCount90d,
                customer.Features.AvgSentimentScore90d,
                customer.Features.ComputedAt
            } : null
        });
    }

    /// <summary>Search customers in the datahub by email, phone, or national ID</summary>
    [HttpGet("customers/search")]
    public async Task<IActionResult> SearchCustomers(
        [FromQuery] string? email,
        [FromQuery] string? phone,
        [FromQuery] string? nationalId,
        CancellationToken ct)
    {
        var query = _hub.DimCustomers.AsQueryable();

        if (!string.IsNullOrEmpty(email))
            query = query.Where(c => c.PrimaryEmail == email.ToLower().Trim());
        if (!string.IsNullOrEmpty(phone))
            query = query.Where(c => c.PrimaryPhone == phone);
        if (!string.IsNullOrEmpty(nationalId))
            query = query.Where(c => c.NationalId == nationalId);

        var results = await query
            .Select(c => new
            {
                c.Gcid,
                c.FirstName,
                c.LastName,
                c.PrimaryEmail,
                c.PrimaryPhone,
                c.NationalId,
                c.OverallKycStatus,
                c.OverallRiskLevel,
                c.CustomerSegment,
                c.SourcesCount,
                c.LastActivityAt
            })
            .Take(20)
            .ToListAsync(ct);

        return Ok(new { count = results.Count, customers = results });
    }

    /// <summary>Get datahub statistics: transaction volumes, risk distribution, channel usage</summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics(CancellationToken ct)
    {
        var txnByChannel = await _hub.FactTransactions
            .Where(t => t.TransactionDate >= DateTimeOffset.UtcNow.AddDays(-30))
            .GroupBy(t => t.Channel)
            .Select(g => new { channel = g.Key ?? "Unknown", count = g.Count(), volume = g.Sum(t => t.Amount) })
            .ToListAsync(ct);

        var riskDistribution = await _hub.FactRiskAssessments
            .Where(r => r.AssessedAt >= DateTimeOffset.UtcNow.AddDays(-30))
            .GroupBy(r => r.RiskLevel)
            .Select(g => new { level = g.Key, count = g.Count(), totalAmount = g.Sum(r => r.Amount ?? 0) })
            .ToListAsync(ct);

        var txnByCategory = await _hub.FactTransactions
            .Where(t => t.TransactionDate >= DateTimeOffset.UtcNow.AddDays(-30) && t.AiCategory != null)
            .GroupBy(t => t.AiCategory)
            .Select(g => new { category = g.Key, count = g.Count(), volume = g.Sum(t => t.Amount) })
            .OrderByDescending(g => g.volume)
            .Take(10)
            .ToListAsync(ct);

        var sentimentSummary = await _hub.FactInteractions
            .Where(i => i.InteractionDate >= DateTimeOffset.UtcNow.AddDays(-30))
            .GroupBy(i => i.SentimentType)
            .Select(g => new { sentiment = g.Key ?? "Unknown", count = g.Count() })
            .ToListAsync(ct);

        return Ok(new
        {
            period = "Last 30 days",
            transactionsByChannel = txnByChannel,
            riskDistribution,
            spendingByCategory = txnByCategory,
            sentimentDistribution = sentimentSummary,
            generatedAt = DateTimeOffset.UtcNow
        });
    }
}
