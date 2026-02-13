using WekezaEnterpriseBrain.Core.DataSources;
using WekezaEnterpriseBrain.Core.Interfaces;
using WekezaEnterpriseBrain.Core.Models;
using WekezaEnterpriseBrain.Core.Events;
using System.Diagnostics;

namespace WekezaEnterpriseBrain.Infrastructure;

/// <summary>
/// Service for aggregating data from multiple Wekeza systems
/// </summary>
public class DataAggregationService : IDataAggregationService
{
    private readonly IDataSourceRegistry _dataSourceRegistry;
    private readonly IIdentityResolutionService _identityService;
    private readonly ICustomer360Service _customer360Service;
    private readonly IEventPublisher _eventPublisher;

    public DataAggregationService(
        IDataSourceRegistry dataSourceRegistry,
        IIdentityResolutionService identityService,
        ICustomer360Service customer360Service,
        IEventPublisher eventPublisher)
    {
        _dataSourceRegistry = dataSourceRegistry;
        _identityService = identityService;
        _customer360Service = customer360Service;
        _eventPublisher = eventPublisher;
    }

    public async Task<DataSyncResult> SyncDataSourceAsync(Guid dataSourceId)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new DataSyncResult
        {
            DataSourceId = dataSourceId,
            CompletedAt = DateTime.UtcNow
        };

        try
        {
            var dataSource = await _dataSourceRegistry.GetDataSourceAsync(dataSourceId);
            if (dataSource == null)
            {
                result.IsSuccessful = false;
                result.ErrorMessage = "Data source not found";
                return result;
            }

            result.DataSourceName = dataSource.Name;

            if (!dataSource.IsEnabled)
            {
                result.IsSuccessful = false;
                result.ErrorMessage = "Data source is disabled";
                return result;
            }

            var connector = await _dataSourceRegistry.GetConnectorAsync(dataSourceId);
            if (connector == null)
            {
                result.IsSuccessful = false;
                result.ErrorMessage = "Connector not available";
                return result;
            }

            // Test connection first
            var connectionTest = await connector.TestConnectionAsync();
            if (!connectionTest.IsConnected)
            {
                result.IsSuccessful = false;
                result.ErrorMessage = $"Connection failed: {connectionTest.Message}";
                return result;
            }

            // Fetch and process customers
            var customers = await connector.FetchCustomersAsync();
            foreach (var customer in customers)
            {
                await ProcessCustomerDataAsync(customer);
                result.CustomersProcessed++;
            }

            // Fetch and process accounts
            var accounts = await connector.FetchAccountsAsync();
            foreach (var account in accounts)
            {
                await ProcessAccountDataAsync(account);
                result.AccountsProcessed++;
            }

            // Fetch and process transactions
            var transactions = await connector.FetchTransactionsAsync();
            foreach (var transaction in transactions)
            {
                await ProcessTransactionDataAsync(transaction);
                result.TransactionsProcessed++;
            }

            result.IsSuccessful = true;
        }
        catch (Exception ex)
        {
            result.IsSuccessful = false;
            result.ErrorMessage = ex.Message;
        }
        finally
        {
            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;
        }

        return result;
    }

    public async Task<IEnumerable<DataSyncResult>> SyncAllDataSourcesAsync()
    {
        var dataSources = await _dataSourceRegistry.GetAllDataSourcesAsync();
        var enabledSources = dataSources.Where(ds => ds.IsEnabled);

        var results = new List<DataSyncResult>();
        foreach (var source in enabledSources)
        {
            var result = await SyncDataSourceAsync(source.Id);
            results.Add(result);
        }

        return results;
    }

    public async Task ProcessCustomerDataAsync(CustomerData customerData)
    {
        // Resolve or create GCID
        var gcid = await _identityService.ResolveOrCreateAsync(
            customerData.NationalId,
            customerData.PhoneNumber,
            customerData.Email);

        if (gcid == null)
        {
            return;
        }

        // Check if Customer 360 exists
        var customer360 = await _customer360Service.GetCustomerByGlobalIdAsync(gcid.Id);
        
        if (customer360 == null)
        {
            // Create new Customer 360
            customer360 = new Customer360
            {
                GlobalCustomerId = gcid.Id,
                FirstName = customerData.FirstName ?? "",
                LastName = customerData.LastName ?? "",
                NationalId = customerData.NationalId,
                PrimaryPhone = customerData.PhoneNumber,
                PrimaryEmail = customerData.Email,
                DateOfBirth = customerData.DateOfBirth,
                Address = customerData.Address,
                Status = customerData.Status,
                CreatedAt = customerData.CreatedAt,
                UpdatedAt = customerData.UpdatedAt
            };

            await _customer360Service.CreateOrUpdateAsync(customer360);

            // Publish event
            await _eventPublisher.PublishAsync(new CustomerEvent
            {
                EventType = "CustomerCreated",
                SourceSystem = customerData.SourceSystem,
                GlobalCustomerId = gcid.Id,
                LocalCustomerId = customerData.LocalCustomerId,
                NationalId = customerData.NationalId,
                PhoneNumber = customerData.PhoneNumber,
                Email = customerData.Email,
                FirstName = customerData.FirstName ?? "",
                LastName = customerData.LastName ?? "",
                Action = "Created"
            });
        }
        else
        {
            // Update existing customer with new data
            if (!string.IsNullOrEmpty(customerData.FirstName))
                customer360.FirstName = customerData.FirstName;
            if (!string.IsNullOrEmpty(customerData.LastName))
                customer360.LastName = customerData.LastName;
            if (!string.IsNullOrEmpty(customerData.Address))
                customer360.Address = customerData.Address;
            
            customer360.UpdatedAt = DateTime.UtcNow;
            await _customer360Service.CreateOrUpdateAsync(customer360);

            // Publish event
            await _eventPublisher.PublishAsync(new CustomerEvent
            {
                EventType = "CustomerUpdated",
                SourceSystem = customerData.SourceSystem,
                GlobalCustomerId = gcid.Id,
                LocalCustomerId = customerData.LocalCustomerId,
                Action = "Updated"
            });
        }
    }

    public async Task ProcessAccountDataAsync(AccountData accountData)
    {
        // This is a simplified version - in production, we'd need to resolve customer first
        // For now, we'll publish an event that can be processed
        await _eventPublisher.PublishAsync(new AccountEvent
        {
            EventType = "AccountUpdated",
            SourceSystem = accountData.SourceSystem,
            AccountNumber = accountData.AccountNumber,
            AccountType = accountData.AccountType,
            CurrentBalance = accountData.CurrentBalance,
            Status = accountData.Status,
            Action = "Updated",
            GlobalCustomerId = Guid.Empty // Would need to resolve from LocalCustomerId
        });
    }

    public async Task ProcessTransactionDataAsync(TransactionData transactionData)
    {
        // Publish transaction event
        await _eventPublisher.PublishAsync(new TransactionEvent
        {
            EventType = "TransactionPosted",
            SourceSystem = transactionData.SourceSystem,
            TransactionId = transactionData.LocalTransactionId,
            AccountNumber = transactionData.LocalAccountId,
            Amount = transactionData.Amount,
            Currency = transactionData.Currency,
            TransactionType = transactionData.TransactionType,
            Channel = transactionData.Channel,
            BalanceAfter = transactionData.BalanceAfter,
            GlobalCustomerId = Guid.Empty // Would need to resolve
        });
    }

    public async Task<DataAggregationStats> GetStatisticsAsync()
    {
        var dataSources = await _dataSourceRegistry.GetAllDataSourcesAsync();
        var allDataSources = dataSources.ToList();
        
        var stats = new DataAggregationStats
        {
            TotalDataSources = allDataSources.Count,
            EnabledDataSources = allDataSources.Count(ds => ds.IsEnabled),
            LastSyncTime = DateTime.UtcNow // In production, track actual last sync
        };

        // Count by source system type
        foreach (var source in allDataSources)
        {
            var typeName = source.Type.ToString();
            if (!stats.SourceSystemCounts.ContainsKey(typeName))
            {
                stats.SourceSystemCounts[typeName] = 0;
            }
            stats.SourceSystemCounts[typeName]++;
        }

        return stats;
    }
}
