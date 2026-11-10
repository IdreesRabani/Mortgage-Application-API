namespace Mortage_Application_API.Logging
{
    public class InMemoryLoggerProvider : ILoggerProvider
    {
        private readonly InMemoryLogStore _store;

        public InMemoryLoggerProvider(InMemoryLogStore store)
        {
            _store = store;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new InMemoryLogger(categoryName, _store);
        }

        public void Dispose()
        {
        }
    }
}
