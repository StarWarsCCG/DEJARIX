using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Dejarix.App.Entities;

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
                        try
                        {
                            var factory = httpContext.RequestServices.GetService<ConnectionFactory>();
                            
                            await using (var context = factory.CreateContext())
                                await context.LogAsync(ex);
                        }
                        catch
                        {

                        }

                        throw;
                    }
                };
            });
        }
    }
}