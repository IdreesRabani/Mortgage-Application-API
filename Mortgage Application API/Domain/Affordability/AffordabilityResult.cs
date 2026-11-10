namespace Mortage_Application_API.Domain.Affordability
{
    public sealed record AffordabilityResult(
        decimal LoanAmount,
        decimal LoanToValuePercent,
        decimal DebtToIncomePercent,
        decimal EstimatedMonthlyRepayment,
        decimal MaximumBorrowing,
        AffordabilityDecision Decision,
        IReadOnlyList<string> Reasons,
        string Explanation);
}
