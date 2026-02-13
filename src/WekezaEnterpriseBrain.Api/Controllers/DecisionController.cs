using Microsoft.AspNetCore.Mvc;
using WekezaEnterpriseBrain.Core.Interfaces;
using WekezaEnterpriseBrain.Core.Models;

namespace WekezaEnterpriseBrain.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DecisionController : ControllerBase
{
    private readonly IDecisionEngineService _decisionEngineService;
    private readonly ILogger<DecisionController> _logger;

    public DecisionController(
        IDecisionEngineService decisionEngineService,
        ILogger<DecisionController> logger)
    {
        _decisionEngineService = decisionEngineService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<DecisionResponse>> MakeDecision([FromBody] DecisionRequest request)
    {
        if (request.GlobalCustomerId == Guid.Empty)
        {
            return BadRequest(new { message = "GlobalCustomerId is required" });
        }

        if (string.IsNullOrEmpty(request.EventType))
        {
            return BadRequest(new { message = "EventType is required" });
        }

        var startTime = DateTime.UtcNow;
        var response = await _decisionEngineService.MakeDecisionAsync(request);
        var latency = (DateTime.UtcNow - startTime).TotalMilliseconds;

        _logger.LogInformation(
            "Decision made for GCID {GlobalCustomerId}, Event {EventType}: {Decision} (Latency: {Latency}ms)",
            request.GlobalCustomerId, request.EventType, response.Decision, latency);

        return Ok(response);
    }

    [HttpGet("risk-score/{gcid}")]
    public async Task<ActionResult<object>> GetRiskScore(Guid gcid, [FromQuery] string eventType = "GENERAL")
    {
        var riskScore = await _decisionEngineService.CalculateRiskScoreAsync(gcid, eventType);
        return Ok(new { globalCustomerId = gcid, riskScore, eventType });
    }
}
