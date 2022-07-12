using log4net;

namespace API
{
    public class Log4NetAdapter : ILogger
    {
        private readonly ILog logger;

        public Log4NetAdapter(string loggerName)
        {
            logger = LogManager.GetLogger(typeof(Log4NetAdapter));
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null!;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Debug:
                    return logger.IsDebugEnabled;
                case LogLevel.Information:
                    return logger.IsInfoEnabled;
                case LogLevel.Warning:
                    return logger.IsWarnEnabled;
                case LogLevel.Error:
                    return logger.IsErrorEnabled;
                case LogLevel.Critical:
                    return logger.IsFatalEnabled;
                default:
                    throw new ArgumentException($"Unknown log level {logLevel}.", nameof(logLevel));
            }
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            string? message;

            if (formatter != null)
            {
                message = formatter(state, exception);
            }
            else
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            switch (logLevel)
            {
                case LogLevel.Debug:
                    logger.Debug(message, exception);
                    break;
                case LogLevel.Information:
                    logger.Info(message, exception);
                    break;
                case LogLevel.Warning:
                    logger.Warn(message, exception);
                    break;
                case LogLevel.Error:
                    logger.Error(message, exception);
                    break;
                case LogLevel.Critical:
                    logger.Fatal(message, exception);
                    break;
                default:
                    logger.Warn($"Encountered unknown log level {logLevel}, writing out as Info.");
                    logger.Info(message, exception);
                    break;
            }
        }
    }
}
