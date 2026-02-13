using WekezaEnterpriseBrain.Core.Interfaces;
using WekezaEnterpriseBrain.Core.Models;

namespace WekezaEnterpriseBrain.Core.Services;

/// <summary>
/// In-memory implementation of Identity Resolution Service (for POC/demo)
/// In production, this would use a database
/// </summary>
public class InMemoryIdentityResolutionService : IIdentityResolutionService
{
    private readonly Dictionary<Guid, GlobalCustomerId> _globalCustomers = new();
    private readonly Dictionary<string, Guid> _nationalIdIndex = new();
    private readonly Dictionary<string, Guid> _phoneIndex = new();
    private readonly Dictionary<string, Guid> _emailIndex = new();

    public Task<GlobalCustomerId?> GetGlobalCustomerIdAsync(Guid id)
    {
        _globalCustomers.TryGetValue(id, out var customer);
        return Task.FromResult(customer);
    }

    public Task<GlobalCustomerId?> FindByNationalIdAsync(string nationalId)
    {
        if (_nationalIdIndex.TryGetValue(nationalId, out var id))
        {
            return GetGlobalCustomerIdAsync(id);
        }
        return Task.FromResult<GlobalCustomerId?>(null);
    }

    public Task<GlobalCustomerId?> FindByPhoneAsync(string phoneNumber)
    {
        if (_phoneIndex.TryGetValue(phoneNumber, out var id))
        {
            return GetGlobalCustomerIdAsync(id);
        }
        return Task.FromResult<GlobalCustomerId?>(null);
    }

    public Task<GlobalCustomerId?> FindByEmailAsync(string email)
    {
        if (_emailIndex.TryGetValue(email, out var id))
        {
            return GetGlobalCustomerIdAsync(id);
        }
        return Task.FromResult<GlobalCustomerId?>(null);
    }

    public Task<GlobalCustomerId> CreateGlobalCustomerIdAsync()
    {
        var gcid = new GlobalCustomerId
        {
            Id = Guid.NewGuid(),
            GcidValue = $"GCID{Guid.NewGuid():N}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _globalCustomers[gcid.Id] = gcid;
        return Task.FromResult(gcid);
    }

    public Task AddIdentityMappingAsync(Guid globalCustomerId, string systemName, string localCustomerId,
        string? nationalId = null, string? phone = null, string? email = null)
    {
        if (!_globalCustomers.TryGetValue(globalCustomerId, out var gcid))
        {
            throw new ArgumentException($"Global Customer ID {globalCustomerId} not found");
        }

        var mapping = new CustomerIdentityMapping
        {
            Id = Guid.NewGuid(),
            GlobalCustomerId = globalCustomerId,
            SystemName = systemName,
            LocalCustomerId = localCustomerId,
            NationalId = nationalId,
            PhoneNumber = phone,
            Email = email,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        gcid.IdentityMappings.Add(mapping);

        // Update indexes
        if (!string.IsNullOrEmpty(nationalId) && !_nationalIdIndex.ContainsKey(nationalId))
        {
            _nationalIdIndex[nationalId] = globalCustomerId;
        }
        if (!string.IsNullOrEmpty(phone) && !_phoneIndex.ContainsKey(phone))
        {
            _phoneIndex[phone] = globalCustomerId;
        }
        if (!string.IsNullOrEmpty(email) && !_emailIndex.ContainsKey(email))
        {
            _emailIndex[email] = globalCustomerId;
        }

        return Task.CompletedTask;
    }

    public async Task<GlobalCustomerId?> ResolveOrCreateAsync(string? nationalId, string? phone, string? email)
    {
        // Try to find existing customer by any of the identifiers
        GlobalCustomerId? existing = null;
        
        if (!string.IsNullOrEmpty(nationalId))
        {
            existing = await FindByNationalIdAsync(nationalId);
        }
        
        if (existing == null && !string.IsNullOrEmpty(phone))
        {
            existing = await FindByPhoneAsync(phone);
        }
        
        if (existing == null && !string.IsNullOrEmpty(email))
        {
            existing = await FindByEmailAsync(email);
        }

        if (existing != null)
        {
            return existing;
        }

        // Create new global customer
        var newGcid = await CreateGlobalCustomerIdAsync();
        await AddIdentityMappingAsync(newGcid.Id, "EnterpriseBrain", newGcid.GcidValue, nationalId, phone, email);
        
        return newGcid;
    }
}
