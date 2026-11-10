using System.Globalization;

namespace Mortage_Application_API.Domain.Affordability
{
    public sealed class AffordabilityCalculator(AffordabilityOptions options) : IAffordabilityCalculator
    {
        private static readonly CultureInfo Gbp = CultureInfo.GetCultureInfo("en-GB");

        public AffordabilityResult Assess(MortgageApplication application)
        {
            ArgumentNullException.ThrowIfNull(application);

            var loanAmount = application.PropertyValue - application.DepositAmount;
            var monthlyIncome = application.AnnualIncome / 12m;
            var months = application.TermYears * 12;

            var loanToValuePercent = RoundUp(loanAmount / application.PropertyValue * 100m);
            var monthlyRepayment = MonthlyRepayment(loanAmount, months);
            var debtToIncomePercent = RoundUp((application.MonthlyExpenses + monthlyRepayment) / monthlyIncome * 100m);

            // Three independent caps on how much we would lend; the lowest one wins.
            var incomeMultipleCap = application.AnnualIncome * options.MaxIncomeMultiple;
            var maxAffordableRepayment = monthlyIncome * options.MaxDebtToIncomePercent / 100m - application.MonthlyExpenses;
            var debtToIncomeCap = maxAffordableRepayment <= 0 ? 0m : PrincipalForRepayment(maxAffordableRepayment, months);
            var loanToValueCap = application.PropertyValue * options.MaxLoanToValuePercent / 100m;

            var maximumBorrowing = RoundDown(Math.Min(incomeMultipleCap, Math.Min(debtToIncomeCap, loanToValueCap)));

            var reasons = new List<string>();
            var approved = true;

            // Loan-to-value check
            if (loanToValuePercent > options.MaxLoanToValuePercent)
            {
                approved = false;

                reasons.Add(
                    $"Loan-to-value of {loanToValuePercent}% exceeds " +
                    $"the maximum permitted limit of {options.MaxLoanToValuePercent}%.");
            }
            else
            {
                reasons.Add(
                    $"Loan-to-value of {loanToValuePercent}% " +
                    $"is within the permitted limit of {options.MaxLoanToValuePercent}%.");
            }

            // Debt-to-income check
            if (debtToIncomePercent > options.MaxDebtToIncomePercent)
            {
                approved = false;

                reasons.Add(
                    $"Debt-to-income of {debtToIncomePercent}% exceeds " +
                    $"the maximum permitted limit of {options.MaxDebtToIncomePercent}%.");
            }
            else
            {
                reasons.Add(
                    $"Debt-to-income of {debtToIncomePercent}% " +
                    $"is within the permitted limit of {options.MaxDebtToIncomePercent}%.");
            }

            // Income multiple check
            if (loanAmount > incomeMultipleCap)
            {
                approved = false;

                reasons.Add(
                    $"Requested loan of {Money(loanAmount)} exceeds " +
                    $"the income-based lending limit of {Money(incomeMultipleCap)} " +
                    $"({options.MaxIncomeMultiple}x annual income).");
            }
            else
            {
                reasons.Add(
                    $"Requested loan of {Money(loanAmount)} is within " +
                    $"the income-based lending limit of {Money(incomeMultipleCap)} " +
                    $"({options.MaxIncomeMultiple}x annual income).");
            }

            // Overall explanation
            var explanation = approved
                ? "Approved: Your mortgage application meets all affordability criteria."
                : "Declined: Your mortgage application does not meet one or more affordability criteria.";

            // Return the assessment
            return new AffordabilityResult(
                LoanAmount: loanAmount,
                LoanToValuePercent: loanToValuePercent,
                DebtToIncomePercent: debtToIncomePercent,
                EstimatedMonthlyRepayment: monthlyRepayment,
                MaximumBorrowing: maximumBorrowing,
                Decision: approved
                    ? AffordabilityDecision.Approved
                    : AffordabilityDecision.Declined,
                Reasons: reasons,
                Explanation: explanation);
        }

        /// Standard capital-and-interest (annuity) repayment: P * r / (1 - (1 + r)^-n).
        internal decimal MonthlyRepayment(decimal principal, int months)
        {
            if (principal <= 0) return 0m;

            var monthlyRate = options.StressInterestRatePercent / 100m / 12m;
            if (monthlyRate == 0) return RoundUp(principal / months);

            return RoundUp(principal * monthlyRate / AnnuityDenominator(monthlyRate, months));
        }

        /// Inverse of "MonthlyRepayment": the loan a given monthly repayment can support.
        internal decimal PrincipalForRepayment(decimal monthlyRepayment, int months)
        {
            var monthlyRate = options.StressInterestRatePercent / 100m / 12m;
            if (monthlyRate == 0) return monthlyRepayment * months;

            return monthlyRepayment * AnnuityDenominator(monthlyRate, months) / monthlyRate;
        }

        /// 1 - (1 + r)^-n. decimal has no Pow, and double precision is ample for this factor.
        private static decimal AnnuityDenominator(decimal monthlyRate, int months) =>
            1m - (decimal)Math.Pow(1d + (double)monthlyRate, -months);

        private static decimal RoundUp(decimal value) => Math.Ceiling(value * 100m) / 100m;

        private static decimal RoundDown(decimal value) => Math.Floor(value * 100m) / 100m;

        private static string Money(decimal value) => value.ToString("C0", Gbp);
    }
}