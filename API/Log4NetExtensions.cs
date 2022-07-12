using log4net.Config;

namespace API
{
    public static class Log4NetExtensions
    {
        public static void ConfigureLog4Net(this IWebHostEnvironment appEnv, IConfiguration configuration)
        {
            XmlConfigurator.Configure(new FileInfo("log4net.config"));
        }
        public static ILoggingBuilder AddLog4Net(this ILoggingBuilder loggingBuilder)
        {
            return loggingBuilder.AddProvider(new Log4NetProvider());
        }
    }
}
