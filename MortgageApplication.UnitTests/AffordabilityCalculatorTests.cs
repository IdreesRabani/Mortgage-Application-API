using Mortage_Application_API.Domain;
using Mortage_Application_API.Domain.Affordability;

namespace MortgageApplications.UnitTests
{
    [TestFixture]
    public class AffordabilityCalculatorTests
    {
        private readonly AffordabilityCalculator _calculator = new(new AffordabilityOptions());

        private static MortgageApplication TestApplication(
            decimal annualIncome = 60_000m,
            decimal monthlyExpenses = 500m,
            decimal deposit = 50_000m,
            decimal propertyValue = 300_000m,
            int termYears = 25) =>
            new(annualIncome, monthlyExpenses, deposit, propertyValue, termYears);

        [Test]
        public void Approves_an_affordable_application_and_reports_the_metrics()
        {
            var result = _calculator.Assess(TestApplication());

            Assert.That(result.Decision, Is.EqualTo(AffordabilityDecision.Approved));
            Assert.That(result.LoanAmount, Is.EqualTo(250_000m));
            Assert.That(result.LoanToValuePercent, Is.EqualTo(83.34m)); // 83.333...% rounded up
                                                             
            // £250k over 25 years at 5.5% is a standard repayment of roughly £1,535 a month.
            Assert.That(result.EstimatedMonthlyRepayment, Is.InRange(1_535m, 1_536m));
            // (£500 expenses + ~£1,535 repayment) / £5,000 monthly income.
            Assert.That(result.DebtToIncomePercent, Is.InRange(40.7m, 40.8m));
            // The 4.5x income cap (£270k) is lower than the DTI and LTV caps, so it sets the maximum.
            Assert.That(result.MaximumBorrowing, Is.EqualTo(270_000m));
            Assert.That(result.Explanation, Does.StartWith("Approved:"));
        }

        [Test]
        public void Declines_when_loan_to_value_exceeds_the_limit()
        {
            var result = _calculator.Assess(TestApplication(deposit: 5_000m)); // 98.333...% LTV

            Assert.That(result.Decision, Is.EqualTo(AffordabilityDecision.Declined));
            Assert.That(result.LoanToValuePercent, Is.EqualTo(98.34m));
            Assert.That(result.Reasons, Has.Some.Contain("Loan-to-value"));
        }

        [Test]
        public void Declines_when_loan_to_value_is_only_fractionally_over_the_limit()
        {
            // £95,004 / £100,000 = 95.004%. Rounding to nearest would show 95.00% and wrongly pass.
            var result = _calculator.Assess(TestApplication(monthlyExpenses: 0m, deposit: 4_996m, propertyValue: 100_000m));

            Assert.That(result.Decision, Is.EqualTo(AffordabilityDecision.Declined));
            Assert.That(result.LoanToValuePercent, Is.EqualTo(95.01m));
            Assert.That(result.Reasons, Has.Some.Contain("Loan-to-value"));
            Assert.That(result.MaximumBorrowing < result.LoanAmount, Is.True);
        }

        [Test]
        public void An_approved_loan_never_exceeds_the_maximum_borrowing()
        {
            // Sweep a grid of awkward values to check the decision and the maximum borrowing never disagree.
            foreach (var income in new[] { 28_000m, 41_234.56m, 60_000m, 87_500.01m })
                foreach (var expenses in new[] { 0m, 333.33m, 1_250m })
                    foreach (var property in new[] { 150_000m, 249_999.99m, 333_333.33m })
                        for (var deposit = 1_000m; deposit < 80_000m; deposit += 1_234.57m)
                        {
                            var result = _calculator.Assess(TestApplication(income, expenses, deposit, property));

                            if (result.Decision == AffordabilityDecision.Approved)
                            {
                                Assert.That(result.LoanAmount <= result.MaximumBorrowing,
                                    $"Approved £{result.LoanAmount} above maximum £{result.MaximumBorrowing} (income {income}, expenses {expenses}, deposit {deposit}, property {property}).");
                            }
                        }
        }

        [Test]
        public void Approves_at_exactly_the_maximum_loan_to_value()
        {
            var result = _calculator.Assess(TestApplication(monthlyExpenses: 0m, deposit: 10_000m, propertyValue: 200_000m));

            Assert.That(result.LoanToValuePercent, Is.EqualTo(95m));
            Assert.That(result.Decision, Is.EqualTo(AffordabilityDecision.Approved));
        }

        [Test]
        public void Declines_when_debt_to_income_exceeds_the_limit()
        {
            var result = _calculator.Assess(TestApplication(monthlyExpenses: 1_500m)); // (1,500 + ~1,535) / 5,000 ≈ 60.7%

            Assert.That(result.Decision, Is.EqualTo(AffordabilityDecision.Declined));
            Assert.That(result.DebtToIncomePercent > 50m, Is.True);
            Assert.That(result.Reasons, Has.Some.Contain("Debt-to-income"));
            Assert.That(result.Reasons, Does.Not.Contain("Loan-to-value"));
        }

        [Test]
        public void Declines_when_loan_exceeds_the_income_multiple()
        {
            // £250k loan vs a £225k (4.5 x £50k) cap; LTV (62.5%) and DTI (~36.8%) are both fine.
            var result = _calculator.Assess(TestApplication(annualIncome: 50_000m, monthlyExpenses: 0m, deposit: 150_000m, propertyValue: 400_000m));

            Assert.That(result.Decision, Is.EqualTo(AffordabilityDecision.Declined));
            Assert.That(result.MaximumBorrowing, Is.EqualTo(225_000m));
            Assert.That(result.Reasons, Has.Some.Contain("annual income"));
            Assert.That(result.Reasons, Has.Some.Contain("4.5x"));
        }

        [Test]
        public void Lists_every_failed_rule_in_the_reasons()
        {
            var result = _calculator.Assess(TestApplication(annualIncome: 30_000m, monthlyExpenses: 1_000m, deposit: 5_000m));

            Assert.That(result.Decision, Is.EqualTo(AffordabilityDecision.Declined));
            Assert.That(result.Reasons, Has.Some.Contain("Loan-to-value"));
            Assert.That(result.Reasons, Has.Some.Contain("Debt-to-income"));
            Assert.That(result.Reasons, Has.Some.Contain("annual income"));
            Assert.That(result.Explanation, Does.StartWith("Declined:"));
        }

        [Test]
        public void Maximum_borrowing_is_capped_by_loan_to_value_for_high_earners()
        {
            // Income caps are far higher than the property can support, so 95% of £100k wins.
            var result = _calculator.Assess(TestApplication(annualIncome: 200_000m, monthlyExpenses: 0m, deposit: 10_000m, propertyValue: 100_000m));

            Assert.That(result.MaximumBorrowing, Is.EqualTo(95_000m));
        }

        [Test]
        public void Maximum_borrowing_is_capped_by_debt_to_income_when_expenses_are_high()
        {
            // £5,000/month income, 50% DTI => £2,500 for outgoings; £2,000 expenses leaves £500/month
            // for a repayment, which supports roughly £81k over 25 years at 5.5%.
            var result = _calculator.Assess(TestApplication(monthlyExpenses: 2_000m));

            Assert.That(result.MaximumBorrowing, Is.InRange(81_000m, 82_000m));
        }

        [Test]
        public void Maximum_borrowing_is_zero_when_expenses_use_up_the_debt_to_income_allowance()
        {
            var result = _calculator.Assess(TestApplication(annualIncome: 24_000m, monthlyExpenses: 1_200m)); // allowance is £1,000

            Assert.That(result.MaximumBorrowing, Is.EqualTo(0m));
            Assert.That(result.Decision, Is.EqualTo(AffordabilityDecision.Declined));
        }

        [Test]
        public void Longer_terms_reduce_the_monthly_repayment()
        {
            var shortTerm = _calculator.Assess(TestApplication(termYears: 15));
            var longTerm = _calculator.Assess(TestApplication(termYears: 35));

            Assert.That(longTerm.EstimatedMonthlyRepayment < shortTerm.EstimatedMonthlyRepayment, Is.True);
            Assert.That(longTerm.DebtToIncomePercent < shortTerm.DebtToIncomePercent, Is.True);
        }

        [Test]
        public void Uses_straight_line_repayment_when_the_stress_rate_is_zero()
        {
            var calculator = new AffordabilityCalculator(new AffordabilityOptions { StressInterestRatePercent = 0m });

            Assert.That(calculator.MonthlyRepayment(120_000m, 120), Is.EqualTo(1_000m));
        }

        [Test]
        public void Repayment_and_principal_calculations_are_inverses()
        {
            var repayment = _calculator.MonthlyRepayment(250_000m, 300);

            Assert.That(_calculator.PrincipalForRepayment(repayment, 300), Is.InRange(249_990m, 250_010m));
        }

        [Test]
        public void Respects_configured_thresholds()
        {
            var strict = new AffordabilityCalculator(new AffordabilityOptions { MaxLoanToValuePercent = 80m });

            var result = strict.Assess(TestApplication()); // 83.33% LTV

            Assert.That(result.Decision, Is.EqualTo(AffordabilityDecision.Declined));
            Assert.That(result.MaximumBorrowing, Is.EqualTo(240_000m)); // 80% of £300k
        }
    }

}
