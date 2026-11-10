namespace Mortage_Application_API.Domain
{
    public class MortgageApplication
    {
        public const int MinTermYears = 5;
        public const int MaxTermYears = 40;

        public MortgageApplication(
            decimal annualIncome,
            decimal monthlyExpenses,
            decimal depositAmount,
            decimal propertyValue,
            int termYears)
        {
            Id = Guid.NewGuid();
            AnnualIncome = annualIncome;
            MonthlyExpenses = monthlyExpenses;
            DepositAmount = depositAmount;
            PropertyValue = propertyValue;
            TermYears = termYears;
        }

        public Guid Id { get; private set; }
        public decimal AnnualIncome { get; private set; }
        public decimal MonthlyExpenses { get; private set; }
        public decimal DepositAmount { get; private set; }
        public decimal PropertyValue { get; private set; }
        public int TermYears { get; private set; }
    }
}
