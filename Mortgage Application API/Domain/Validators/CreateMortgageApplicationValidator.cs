using FluentValidation;
using Mortage_Application_API.Domain;
using Mortgage_Application_API.Domain.Requests;

namespace Mortgage_Application_API.Domain.Validators
{
    public class CreateMortgageApplicationValidator : AbstractValidator<CreateMortgageApplicationRequest>
    {
        public CreateMortgageApplicationValidator()
        {
            RuleFor(x => x.AnnualIncome)
                .GreaterThan(0)
                .WithMessage("Annual income must be positive.");

            RuleFor(x => x.MonthlyExpenses)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Monthly expenses cannot be negative.");

            RuleFor(x => x.PropertyValue)
                .GreaterThan(0)
                .WithMessage("Property value must be positive.");

            RuleFor(x => x.DepositAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Deposit cannot be negative.");

            RuleFor(x => x.DepositAmount)
                .LessThan(x => x.PropertyValue)
                .When(x => x.PropertyValue > 0)
                .WithMessage(
                    "Deposit must be less than the property value.");

            RuleFor(x => x.TermYears)
                .InclusiveBetween(
                    MortgageApplication.MinTermYears,
                    MortgageApplication.MaxTermYears)
                .WithMessage(
                    "Term must be between 5 and 40 years.");
        }
    }
}
