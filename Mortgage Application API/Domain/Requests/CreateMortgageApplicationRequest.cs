namespace Mortgage_Application_API.Domain.Requests
{
    public class CreateMortgageApplicationRequest
    {
        public decimal AnnualIncome { get; set; }
        public decimal MonthlyExpenses { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal PropertyValue { get; set; }
        public int TermYears { get; set; }
    }
}
