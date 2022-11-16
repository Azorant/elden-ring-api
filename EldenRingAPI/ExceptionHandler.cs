using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using Newtonsoft.Json;


namespace EldenRingAPI
{
    public static class UncaughtExceptionHandler
    {
        public static void ConfigureExceptionHandler(this IApplicationBuilder app, ILogger logger)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";
                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        logger.LogError($"Something went wrong: {contextFeature.Error}");
                    }
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new Response(HttpStatusCode.InternalServerError, "An error has occurred")));
                });
            });
        }
    }
}
