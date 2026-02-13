using WekezaEnterpriseBrain.Core.Interfaces;
using WekezaEnterpriseBrain.Core.Models;

namespace WekezaEnterpriseBrain.Core.Services;

/// <summary>
/// Decision Engine Service for real-time decisioning
/// </summary>
public class DecisionEngineService : IDecisionEngineService
{
    private readonly ICustomer360Service _customer360Service;

    public DecisionEngineService(ICustomer360Service customer360Service)
    {
        _customer360Service = customer360Service;
    }

    public async Task<DecisionResponse> MakeDecisionAsync(DecisionRequest request)
    {
        var customer = await _customer360Service.GetCustomerByGlobalIdAsync(request.GlobalCustomerId);
        
        if (customer == null)
        {
            return new DecisionResponse
            {
                Decision = "DECLINE",
                RiskScore = 1.0m,
                Reason = "Customer not found",
                Timestamp = DateTime.UtcNow
            };
        }

        var riskScore = await CalculateRiskScoreAsync(request.GlobalCustomerId, request.EventType, request.Amount);
        var flags = new List<string>();

        // Apply decision rules based on event type
        string decision = request.EventType.ToUpper() switch
        {
            "PAYMENT" => DeterminePaymentDecision(customer, request.Amount ?? 0, riskScore, flags),
            "LOAN_REQUEST" => DetermineLoanDecision(customer, request.Amount ?? 0, riskScore, flags),
            "LOGIN" => DetermineLoginDecision(customer, riskScore, flags),
            "ACCOUNT_OPENING" => DetermineAccountOpeningDecision(customer, riskScore, flags),
            _ => DetermineGeneralDecision(riskScore, flags)
        };

        return new DecisionResponse
        {
            Decision = decision,
            RiskScore = riskScore,
            Reason = GetDecisionReason(decision, flags, request.EventType),
            Flags = flags,
            Timestamp = DateTime.UtcNow
        };
    }

    public async Task<decimal> CalculateRiskScoreAsync(Guid globalCustomerId, string eventType, decimal? amount = null)
    {
        var customer = await _customer360Service.GetCustomerByGlobalIdAsync(globalCustomerId);
        
        if (customer == null)
        {
            return 1.0m; // Maximum risk for unknown customers
        }

        decimal score = customer.RiskScore;

        // Adjust score based on behavioral patterns
        if (customer.FailedLoginAttempts > 3)
        {
            score += 0.2m;
        }

        if (customer.Status != "Active")
        {
            score += 0.3m;
        }

        // Adjust based on transaction patterns (if amount provided)
        if (amount.HasValue && customer.AverageDailyBalance > 0)
        {
            var amountRatio = amount.Value / customer.AverageDailyBalance;
            if (amountRatio > 0.5m) // Transaction is > 50% of average balance
            {
                score += 0.15m;
            }
        }

        return Math.Min(score, 1.0m); // Cap at 1.0
    }

    private string DeterminePaymentDecision(Customer360 customer, decimal amount, decimal riskScore, List<string> flags)
    {
        if (riskScore > 0.7m)
        {
            flags.Add("HIGH_RISK_SCORE");
            return "DECLINE";
        }

        if (amount > customer.AvailableBalance)
        {
            flags.Add("INSUFFICIENT_BALANCE");
            return "DECLINE";
        }

        if (amount > customer.AverageDailyBalance * 2)
        {
            flags.Add("UNUSUAL_AMOUNT");
            return "REVIEW";
        }

        if (riskScore > 0.4m)
        {
            flags.Add("MODERATE_RISK");
            return "REVIEW";
        }

        return "APPROVE";
    }

    private string DetermineLoanDecision(Customer360 customer, decimal amount, decimal riskScore, List<string> flags)
    {
        if (riskScore > 0.6m)
        {
            flags.Add("HIGH_RISK_PROFILE");
            return "DECLINE";
        }

        if (customer.MonthlyInflow < amount * 0.1m) // Loan > 10x monthly inflow
        {
            flags.Add("INSUFFICIENT_INCOME");
            return "DECLINE";
        }

        if (riskScore > 0.3m)
        {
            flags.Add("REQUIRES_MANUAL_REVIEW");
            return "REVIEW";
        }

        return "APPROVE";
    }

    private string DetermineLoginDecision(Customer360 customer, decimal riskScore, List<string> flags)
    {
        if (customer.FailedLoginAttempts > 5)
        {
            flags.Add("MULTIPLE_FAILED_ATTEMPTS");
            return "DECLINE";
        }

        if (riskScore > 0.5m)
        {
            flags.Add("SUSPICIOUS_ACTIVITY");
            return "REVIEW";
        }

        return "APPROVE";
    }

    private string DetermineAccountOpeningDecision(Customer360 customer, decimal riskScore, List<string> flags)
    {
        if (riskScore > 0.5m)
        {
            flags.Add("HIGH_RISK_APPLICANT");
            return "REVIEW";
        }

        return "APPROVE";
    }

    private string DetermineGeneralDecision(decimal riskScore, List<string> flags)
    {
        if (riskScore > 0.7m)
        {
            flags.Add("HIGH_RISK");
            return "DECLINE";
        }

        if (riskScore > 0.4m)
        {
            flags.Add("MODERATE_RISK");
            return "REVIEW";
        }

        return "APPROVE";
    }

    private string GetDecisionReason(string decision, List<string> flags, string eventType)
    {
        if (decision == "APPROVE")
        {
            return $"{eventType} approved - Normal behavior pattern";
        }

        if (decision == "DECLINE")
        {
            return flags.Any() ? $"Declined: {string.Join(", ", flags)}" : "Declined due to risk assessment";
        }

        return flags.Any() ? $"Manual review required: {string.Join(", ", flags)}" : "Manual review required";
    }
}
