using WekezaEnterpriseBrain.Core.Models;

namespace WekezaEnterpriseBrain.Core.Interfaces;

/// <summary>
/// Real-time decision engine service
/// </summary>
public interface IDecisionEngineService
{
    Task<DecisionResponse> MakeDecisionAsync(DecisionRequest request);
    Task<decimal> CalculateRiskScoreAsync(Guid globalCustomerId, string eventType, decimal? amount = null);
}
