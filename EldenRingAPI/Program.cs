using NLog;
using NLog.Web;

namespace EldenRingAPI
{
    public static class Program
    {
        public static void Main(string[] args)
        {

            var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

            try
            {
                var builder = WebApplication.CreateBuilder(args);
                var startup = new Startup(builder.Configuration);
                startup.ConfigureServices(builder.Services);
                var app = builder.Build();
                startup.Configure(app, builder.Environment);
            }
            catch (Exception e)
            {
                logger.Error(e, "Stopped program because of exception");
            }
            finally
            {
                LogManager.Shutdown();
            }

        }
    }
}