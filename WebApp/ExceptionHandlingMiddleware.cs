using System;
using System.Threading.Tasks;
using Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using OpenTracing;
using OpenTracing.Util;

namespace WebApp
{
  public static class ExceptionMiddlewareExtensions
  {
    public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder app)
    {
      return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
  }

  public class ExceptionHandlingMiddleware
  {
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
      _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
      try
      {
        await _next(httpContext);
      }
      catch (Exception ex)
      {
        await handleExceptionAsync(httpContext, ex);
      }
    }

    private static async Task handleExceptionAsync(HttpContext httpContext, Exception ex)
    {
      ISpan span = GlobalTracer.Instance.ActiveSpan;

      span.LogError(ex, "Internal Server Error from the ExceptionHandlingMiddleware.");

      httpContext.Response.StatusCode = 500;

      await httpContext.Response.WriteAsync($"StatusCode = 500, Error = '{ex.Message}'");
    }
  }
}
