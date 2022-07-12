namespace API
{
    public class Log4NetProvider: ILoggerProvider
    {
        private IDictionary<string, ILogger> loggers = new Dictionary<string, ILogger>();

        public void Dispose()
        {
            loggers.Clear();
            loggers = null!;
        }

        public ILogger CreateLogger(string name)
        {
            if (!loggers.ContainsKey(name))
            {
                lock (loggers)
                {
                    // Have to check again since another thread may have gotten the lock first
                    if (!loggers.ContainsKey(name))
                    {
                        loggers[name] = new Log4NetAdapter(name);
                    }
                }
            }
            return loggers[name];
        }
    }
}
