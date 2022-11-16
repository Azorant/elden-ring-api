using EldenRingAPI.Interfaces;
using EldenRingAPI.Services;
using Microsoft.OpenApi.Models;

namespace EldenRingAPI
{
    public class Startup
    {
        public IConfiguration config { get; }

        public Startup(IConfiguration configuration)
        {
            config = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            #region Register services
            services.AddTransient<IWikiService, WikiService>();
            services.AddTransient<IAPIService, APIService>();
            #endregion

            #region Database
            services.AddSingleton<Database>();
            #endregion

            services.AddControllers();
            services.AddEndpointsApiExplorer();

            #region Swagger
            services.AddSwaggerGen(options =>
            {
                options.EnableAnnotations();
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Elden Ring API", Version = "v1" });
            });
            #endregion
        }
        public void Configure(WebApplication app, IWebHostEnvironment env)
        {
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Startup>>();
                app.ConfigureExceptionHandler(logger);
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().Build());
            app.MapControllers();
            app.Run();
        }
    }

}
