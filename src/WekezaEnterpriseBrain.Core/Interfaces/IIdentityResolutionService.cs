using WekezaEnterpriseBrain.Core.Models;

namespace WekezaEnterpriseBrain.Core.Interfaces;

/// <summary>
/// Service for identity resolution and Global Customer ID management
/// </summary>
public interface IIdentityResolutionService
{
    Task<GlobalCustomerId?> GetGlobalCustomerIdAsync(Guid id);
    Task<GlobalCustomerId?> FindByNationalIdAsync(string nationalId);
    Task<GlobalCustomerId?> FindByPhoneAsync(string phoneNumber);
    Task<GlobalCustomerId?> FindByEmailAsync(string email);
    Task<GlobalCustomerId> CreateGlobalCustomerIdAsync();
    Task AddIdentityMappingAsync(Guid globalCustomerId, string systemName, string localCustomerId, 
        string? nationalId = null, string? phone = null, string? email = null);
    Task<GlobalCustomerId?> ResolveOrCreateAsync(string? nationalId, string? phone, string? email);
}
