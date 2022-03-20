﻿using Common;
using Microsoft.Extensions.Logging.Abstractions;
using OpenTracing;
using OpenTracing.Util;

namespace WebApp
{
  public class Startup
  {
    private static readonly ITracer _tracer = Tracing.Init("Web-app", NullLoggerFactory.Instance);

    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
      services.AddControllers();

      // Register tracer globally.
      GlobalTracer.Register(_tracer);

      // Install-Package OpenTracing.Contrib.NetCore
      // Add tracer to the DI container.
      services.AddOpenTracing();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      // Use custom middleware to handle exceptions.
      app.UseExceptionHandlingMiddleware();

      app.UseRouting();

      app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
  }
}
