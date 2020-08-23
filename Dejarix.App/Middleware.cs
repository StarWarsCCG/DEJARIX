using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Dejarix.App.Entities;
using Microsoft.Extensions.Logging;

namespace Dejarix.App
{
    public static class Middleware
    {
        public static IApplicationBuilder UseExceptionLogger(this IApplicationBuilder builder)
        {
            return builder.Use(next =>
            {
                return async httpContext =>
                {
                    try
                    {
                        await next(httpContext);
                    }
                    catch (Exception ex)
                    {
                        var serviceProvider = httpContext.RequestServices;
                        
                        try
                        {
                            var factory = serviceProvider.GetRequiredService<ConnectionFactory>();
                            
                            await using (var context = factory.CreateContext())
                                await context.LogAsync(ex);
                        }
                        catch (Exception dbException)
                        {
                            var logger = serviceProvider.GetRequiredService<ILogger<ConnectionFactory>>();
                            logger.LogError(dbException, "Unable to write exception to database.");
                            logger.LogError(ex, "(This is the exception that was originally thrown.)");
                        }

                        throw;
                    }
                };
            });
        }
    }
}