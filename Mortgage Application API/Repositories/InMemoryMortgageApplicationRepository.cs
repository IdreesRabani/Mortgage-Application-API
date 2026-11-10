using Mortage_Application_API.Domain;
using System.Collections.Concurrent;

namespace Mortage_Application_API.Repositories
{
    public class InMemoryMortgageApplicationRepository : IMortgageApplicationRepository
    {
        private readonly ConcurrentDictionary<Guid, MortgageApplication> _applications = new();
        private ILogger<InMemoryMortgageApplicationRepository> _logger;

        public InMemoryMortgageApplicationRepository(ILogger<InMemoryMortgageApplicationRepository> logger)
        {
            _logger = logger;
        }

        public Task<MortgageApplication?> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Retrieving mortgage application {ApplicationId}", id);
            _applications.TryGetValue(id, out var application);

            return Task.FromResult(application);
        }

        public Task AddAsync(MortgageApplication application)
        {
            _logger.LogInformation("Creating mortgage application {ApplicationId}", application.Id);
            if (!_applications.TryAdd(application.Id, application))
            {
                throw new InvalidOperationException(
                    "An application with this ID already exists.");
            }

            return Task.CompletedTask;
        }
    }
}
