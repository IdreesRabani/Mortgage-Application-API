using Mortgage_Application_API.Domain.Requests;
using Mortgage_Application_API.Domain.Validators;

namespace MortgageApplications.UnitTests
{
    [TestFixture]
    public class CreateMortgageApplicationValidatorTests
    {
        private CreateMortgageApplicationValidator _validator = null!;

        [SetUp]
        public void SetUp()
        {
            _validator = new CreateMortgageApplicationValidator();
        }

        private static CreateMortgageApplicationRequest ValidRequest()
        {
            return new CreateMortgageApplicationRequest
            {
                AnnualIncome = 60_000m,
                MonthlyExpenses = 500m,
                DepositAmount = 50_000m,
                PropertyValue = 300_000m,
                TermYears = 25
            };
        }

        [Test]
        public void Given_Valid_Request_When_Validated_Then_No_Errors()
        {
            var request = ValidRequest();

            var result = _validator.Validate(request);

            Assert.That(result.IsValid, Is.True);
            Assert.That(result.Errors, Is.Empty);
        }

        [TestCase(0)]
        [TestCase(-1000)]
        public void Given_Invalid_Income_Then_Validation_Fails(
            int income)
        {
            var request = ValidRequest();
            request.AnnualIncome = income;

            var result = _validator.Validate(request);

            Assert.That(
                result.Errors.Any(x =>
                    x.PropertyName == nameof(request.AnnualIncome)),
                Is.True);
        }

        [Test]
        public void Given_Negative_Expenses_Then_Validation_Fails()
        {
            var request = ValidRequest();
            request.MonthlyExpenses = -1;

            var result = _validator.Validate(request);

            Assert.That(
                result.Errors.Any(x =>
                    x.PropertyName == nameof(request.MonthlyExpenses)),
                Is.True);
        }

        [TestCase(-1)]
        [TestCase(300000)]
        [TestCase(350000)]
        public void Given_Invalid_Deposit_Then_Validation_Fails(
            int deposit)
        {
            var request = ValidRequest();
            request.DepositAmount = deposit;

            var result = _validator.Validate(request);

            Assert.That(
                result.Errors.Any(x =>
                    x.PropertyName == nameof(request.DepositAmount)),
                Is.True);
        }

        [TestCase(0)]
        [TestCase(-1000)]
        public void Given_Invalid_Property_Value_Then_Validation_Fails(
            int propertyValue)
        {
            var request = ValidRequest();
            request.PropertyValue = propertyValue;

            var result = _validator.Validate(request);

            Assert.That(
                result.Errors.Any(x =>
                    x.PropertyName == nameof(request.PropertyValue)),
                Is.True);
        }

        [TestCase(0)]
        [TestCase(4)]
        [TestCase(41)]
        public void Given_Invalid_Term_Then_Validation_Fails(int term)
        {
            var request = ValidRequest();
            request.TermYears = term;

            var result = _validator.Validate(request);

            Assert.That(
                result.Errors.Any(x =>
                    x.PropertyName == nameof(request.TermYears)),
                Is.True);
        }

        [TestCase(5)]
        [TestCase(40)]
        public void Given_Valid_Term_Boundaries_Then_Validation_Succeeds(
            int term)
        {
            var request = ValidRequest();
            request.TermYears = term;

            var result = _validator.Validate(request);

            Assert.That(result.IsValid, Is.True);
        }

        [Test]
        public void Given_Zero_Expenses_And_Deposit_Then_Validation_Succeeds()
        {
            var request = ValidRequest();
            request.MonthlyExpenses = 0;
            request.DepositAmount = 0;

            var result = _validator.Validate(request);

            Assert.That(result.IsValid, Is.True);
        }

        [Test]
        public void Given_Multiple_Invalid_Properties_Then_All_Errors_Are_Returned()
        {
            var request = ValidRequest();
            request.AnnualIncome = 0;
            request.MonthlyExpenses = -1;
            request.TermYears = 50;

            var result = _validator.Validate(request);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Count, Is.EqualTo(3));
        }
    }
}