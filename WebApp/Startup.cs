using Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
      services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

      // Register tracer globally.
      GlobalTracer.Register(_tracer);

      // Install-Package OpenTracing.Contrib.NetCore
      // Add tracer to the DI container.
      services.AddOpenTracing();
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
      // Use custom middleware to handle exceptions.
      app.UseExceptionHandlingMiddleware();

      app.UseMvc();
    }
  }
}
