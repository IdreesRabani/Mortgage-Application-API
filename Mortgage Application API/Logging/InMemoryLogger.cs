namespace Mortage_Application_API.Logging
{
    public class InMemoryLogger : ILogger
    {
        private readonly string _category;
        private readonly InMemoryLogStore _store;

        public InMemoryLogger(
            string category,
            InMemoryLogStore store)
        {
            _category = category;
            _store = store;
        }

        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull
        {
            return NoopScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var entry = new LogEntry(
                DateTime.UtcNow,
                logLevel,
                _category,
                formatter(state, exception),
                exception?.ToString()
            );

            _store.Add(entry);
        }

        private sealed class NoopScope : IDisposable
        {
            public static readonly NoopScope Instance = new();

            public void Dispose() { }
        }
    }
}
