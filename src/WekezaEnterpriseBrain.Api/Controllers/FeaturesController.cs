using Microsoft.AspNetCore.Mvc;
using WekezaEnterpriseBrain.Core.Features;

namespace WekezaEnterpriseBrain.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeaturesController : ControllerBase
{
    private readonly IFeatureStore _featureStore;
    private readonly ILogger<FeaturesController> _logger;

    public FeaturesController(
        IFeatureStore featureStore,
        ILogger<FeaturesController> logger)
    {
        _featureStore = featureStore;
        _logger = logger;
    }

    [HttpGet("{gcid}")]
    public async Task<ActionResult<CustomerFeatures>> GetFeatures(Guid gcid)
    {
        var features = await _featureStore.GetFeaturesAsync(gcid);
        if (features == null)
        {
            return NotFound(new { message = $"Features for customer {gcid} not found. Try calculating them first." });
        }
        return Ok(features);
    }

    [HttpPost("{gcid}/calculate")]
    public async Task<ActionResult<CustomerFeatures>> CalculateFeatures(Guid gcid)
    {
        try
        {
            _logger.LogInformation("Calculating features for customer {GlobalCustomerId}", gcid);
            var features = await _featureStore.CalculateFeaturesAsync(gcid);
            _logger.LogInformation("Features calculated successfully for customer {GlobalCustomerId}", gcid);
            return Ok(features);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Customer {GlobalCustomerId} not found", gcid);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating features for customer {GlobalCustomerId}", gcid);
            return StatusCode(500, new { message = "Error calculating features", error = ex.Message });
        }
    }

    [HttpGet("importance")]
    public async Task<ActionResult<Dictionary<string, double>>> GetFeatureImportance()
    {
        var importance = await _featureStore.GetFeatureImportanceAsync();
        return Ok(importance);
    }

    [HttpPost("refresh-all")]
    public async Task<ActionResult> RefreshAllFeatures()
    {
        _logger.LogInformation("Starting feature refresh for all customers");
        await _featureStore.RefreshAllFeaturesAsync();
        return Ok(new { message = "Feature refresh initiated" });
    }
}
