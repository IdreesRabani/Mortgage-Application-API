using System.Collections.Concurrent;

namespace Mortage_Application_API.Logging
{
    public class InMemoryLogStore
    {
        private readonly ConcurrentQueue<LogEntry> _logs = new();

        private const int MaxEntries = 1000;

        public void Add(LogEntry entry)
        {
            _logs.Enqueue(entry);

            while (_logs.Count > MaxEntries)
            {
                _logs.TryDequeue(out _);
            }
        }

        public IReadOnlyList<LogEntry> GetAll()
        {
            return _logs.ToArray();
        }
    }
}
