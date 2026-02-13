namespace WekezaEnterpriseBrain.Core.Models;

/// <summary>
/// Global Customer ID (GCID) - The unified customer identifier across all systems
/// </summary>
public class GlobalCustomerId
{
    public Guid Id { get; set; }
    public string GcidValue { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public ICollection<CustomerIdentityMapping> IdentityMappings { get; set; } = new List<CustomerIdentityMapping>();
}

/// <summary>
/// Maps local system customer IDs to the global customer ID
/// </summary>
public class CustomerIdentityMapping
{
    public Guid Id { get; set; }
    public Guid GlobalCustomerId { get; set; }
    public string SystemName { get; set; } = string.Empty;
    public string LocalCustomerId { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public GlobalCustomerId? GlobalCustomer { get; set; }
}
