namespace Mortage_Application_API.Domain.Affordability
{
    public interface IAffordabilityCalculator
    {
        AffordabilityResult Assess(MortgageApplication application);
    }
}
