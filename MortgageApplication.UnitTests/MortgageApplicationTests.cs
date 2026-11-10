using Mortage_Application_API.Domain;

namespace MortgageApplications.UnitTests
{
    [TestFixture]
    public class MortgageApplicationTests
    {
        [Test]
        public void Given_Valid_Values_When_Creating_Application_Then_Properties_Are_Set()
        {
            // Arrange
            const decimal income = 60_000m;
            const decimal expenses = 500m;
            const decimal deposit = 50_000m;
            const decimal propertyValue = 300_000m;
            const int term = 25;

            // Act
            var application = new MortgageApplication(
                income,
                expenses,
                deposit,
                propertyValue,
                term);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(application.Id, Is.Not.EqualTo(Guid.Empty));
                Assert.That(application.AnnualIncome, Is.EqualTo(income));
                Assert.That(application.MonthlyExpenses, Is.EqualTo(expenses));
                Assert.That(application.DepositAmount, Is.EqualTo(deposit));
                Assert.That(application.PropertyValue, Is.EqualTo(propertyValue));
                Assert.That(application.TermYears, Is.EqualTo(term));
            });
        }

        [Test]
        public void Given_Two_Applications_When_Created_Then_Ids_Are_Unique()
        {
            // Act
            var first = new MortgageApplication(
                60_000m, 500m, 50_000m, 300_000m, 25);

            var second = new MortgageApplication(
                60_000m, 500m, 50_000m, 300_000m, 25);

            // Assert
            Assert.That(first.Id, Is.Not.EqualTo(second.Id));
        }
    }

}
