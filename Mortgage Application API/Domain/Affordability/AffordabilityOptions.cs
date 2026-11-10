namespace Mortage_Application_API.Domain.Affordability
{
    public sealed class AffordabilityOptions
    {
        public const string SectionName = "Affordability";

        /// The Highest loan-to-value that will be lent at, e.g. 95 means a minimum 5% deposit.
        public decimal MaxLoanToValuePercent { get; set; } = 95m;

        /// The highest share of gross monthly income that may go on monthly expenses plus the estimated mortgage repayment.
        public decimal MaxDebtToIncomePercent { get; set; } = 50m;

        /// The most that will be lent as a multiple of gross annual income.
        public decimal MaxIncomeMultiple { get; set; } = 4.5m;

        /// Annual interest rate used to estimate repayments. 
        /// Deliberately above typical market rates so the applicant could still afford the loan if rates rise ("stress test").
        public decimal StressInterestRatePercent { get; set; } = 5.5m;
    }
}
