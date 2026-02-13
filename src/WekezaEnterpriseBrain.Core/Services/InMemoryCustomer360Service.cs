using WekezaEnterpriseBrain.Core.Interfaces;
using WekezaEnterpriseBrain.Core.Models;

namespace WekezaEnterpriseBrain.Core.Services;

/// <summary>
/// In-memory implementation of Customer 360 Service (for POC/demo)
/// In production, this would use a database
/// </summary>
public class InMemoryCustomer360Service : ICustomer360Service
{
    private readonly Dictionary<Guid, Customer360> _customersById = new();
    private readonly Dictionary<Guid, Customer360> _customersByGcid = new();
    private readonly Dictionary<Guid, List<Account360>> _accountsByGcid = new();
    private readonly Dictionary<Guid, List<Transaction360>> _transactionsByGcid = new();

    public Task<Customer360?> GetCustomerByGlobalIdAsync(Guid globalCustomerId)
    {
        _customersByGcid.TryGetValue(globalCustomerId, out var customer);
        return Task.FromResult(customer);
    }

    public Task<Customer360?> GetCustomerByIdAsync(Guid id)
    {
        _customersById.TryGetValue(id, out var customer);
        return Task.FromResult(customer);
    }

    public Task<Customer360> CreateOrUpdateAsync(Customer360 customer)
    {
        if (customer.Id == Guid.Empty)
        {
            customer.Id = Guid.NewGuid();
            customer.CreatedAt = DateTime.UtcNow;
        }
        
        customer.UpdatedAt = DateTime.UtcNow;
        
        _customersById[customer.Id] = customer;
        _customersByGcid[customer.GlobalCustomerId] = customer;
        
        return Task.FromResult(customer);
    }

    public Task<IEnumerable<Account360>> GetCustomerAccountsAsync(Guid globalCustomerId)
    {
        if (_accountsByGcid.TryGetValue(globalCustomerId, out var accounts))
        {
            return Task.FromResult<IEnumerable<Account360>>(accounts);
        }
        return Task.FromResult<IEnumerable<Account360>>(new List<Account360>());
    }

    public Task<IEnumerable<Transaction360>> GetRecentTransactionsAsync(Guid globalCustomerId, int count = 50)
    {
        if (_transactionsByGcid.TryGetValue(globalCustomerId, out var transactions))
        {
            var recent = transactions
                .OrderByDescending(t => t.TransactionDate)
                .Take(count);
            return Task.FromResult<IEnumerable<Transaction360>>(recent);
        }
        return Task.FromResult<IEnumerable<Transaction360>>(new List<Transaction360>());
    }

    public async Task UpdateRiskScoreAsync(Guid globalCustomerId, decimal riskScore, string riskCategory)
    {
        var customer = await GetCustomerByGlobalIdAsync(globalCustomerId);
        if (customer != null)
        {
            customer.RiskScore = riskScore;
            customer.RiskCategory = riskCategory;
            customer.UpdatedAt = DateTime.UtcNow;
        }
    }

    // Helper methods for demo data
    public Task AddAccountAsync(Guid globalCustomerId, Account360 account)
    {
        if (!_accountsByGcid.ContainsKey(globalCustomerId))
        {
            _accountsByGcid[globalCustomerId] = new List<Account360>();
        }
        _accountsByGcid[globalCustomerId].Add(account);
        return Task.CompletedTask;
    }

    public Task AddTransactionAsync(Guid globalCustomerId, Transaction360 transaction)
    {
        if (!_transactionsByGcid.ContainsKey(globalCustomerId))
        {
            _transactionsByGcid[globalCustomerId] = new List<Transaction360>();
        }
        _transactionsByGcid[globalCustomerId].Add(transaction);
        return Task.CompletedTask;
    }
}
