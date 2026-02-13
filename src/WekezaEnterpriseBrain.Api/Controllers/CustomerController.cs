using Microsoft.AspNetCore.Mvc;
using WekezaEnterpriseBrain.Core.Interfaces;
using WekezaEnterpriseBrain.Core.Models;

namespace WekezaEnterpriseBrain.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomer360Service _customer360Service;
    private readonly IIdentityResolutionService _identityResolutionService;
    private readonly ILogger<CustomerController> _logger;

    public CustomerController(
        ICustomer360Service customer360Service,
        IIdentityResolutionService identityResolutionService,
        ILogger<CustomerController> logger)
    {
        _customer360Service = customer360Service;
        _identityResolutionService = identityResolutionService;
        _logger = logger;
    }

    [HttpGet("{gcid}")]
    public async Task<ActionResult<Customer360>> GetCustomer(Guid gcid)
    {
        var customer = await _customer360Service.GetCustomerByGlobalIdAsync(gcid);
        if (customer == null)
        {
            return NotFound(new { message = $"Customer with GCID {gcid} not found" });
        }
        return Ok(customer);
    }

    [HttpGet("{gcid}/accounts")]
    public async Task<ActionResult<IEnumerable<Account360>>> GetCustomerAccounts(Guid gcid)
    {
        var accounts = await _customer360Service.GetCustomerAccountsAsync(gcid);
        return Ok(accounts);
    }

    [HttpGet("{gcid}/transactions")]
    public async Task<ActionResult<IEnumerable<Transaction360>>> GetRecentTransactions(
        Guid gcid, 
        [FromQuery] int count = 50)
    {
        var transactions = await _customer360Service.GetRecentTransactionsAsync(gcid, count);
        return Ok(transactions);
    }

    [HttpGet("resolve")]
    public async Task<ActionResult<GlobalCustomerId>> ResolveIdentity(
        [FromQuery] string? nationalId,
        [FromQuery] string? phone,
        [FromQuery] string? email)
    {
        if (string.IsNullOrEmpty(nationalId) && string.IsNullOrEmpty(phone) && string.IsNullOrEmpty(email))
        {
            return BadRequest(new { message = "At least one identifier (nationalId, phone, or email) is required" });
        }

        var globalCustomer = await _identityResolutionService.ResolveOrCreateAsync(nationalId, phone, email);
        return Ok(globalCustomer);
    }
}
