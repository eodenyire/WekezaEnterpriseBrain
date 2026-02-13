using WekezaEnterpriseBrain.Core.Models;

namespace WekezaEnterpriseBrain.Core.Interfaces;

/// <summary>
/// Service for Customer 360 operations
/// </summary>
public interface ICustomer360Service
{
    Task<Customer360?> GetCustomerByGlobalIdAsync(Guid globalCustomerId);
    Task<Customer360?> GetCustomerByIdAsync(Guid id);
    Task<Customer360> CreateOrUpdateAsync(Customer360 customer);
    Task<IEnumerable<Account360>> GetCustomerAccountsAsync(Guid globalCustomerId);
    Task<IEnumerable<Transaction360>> GetRecentTransactionsAsync(Guid globalCustomerId, int count = 50);
    Task UpdateRiskScoreAsync(Guid globalCustomerId, decimal riskScore, string riskCategory);
}
