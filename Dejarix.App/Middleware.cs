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
                            {
                                var id = Guid.NewGuid();
                                int ordinal = 0;

                                for (Exception? e = ex; e != null; e = e.InnerException)
                                {
                                    var log = ExceptionLog.FromException(e, ordinal++);
                                    await context.AddAsync(log);
                                }

                                await context.SaveChangesAsync();
                            }
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