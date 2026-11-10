using Mortage_Application_API.Domain;

namespace Mortage_Application_API.Repositories
{
    public interface IMortgageApplicationRepository
    {
        Task<MortgageApplication?> GetByIdAsync(Guid id);

        Task AddAsync(MortgageApplication mortgageApplication);
    }
}
