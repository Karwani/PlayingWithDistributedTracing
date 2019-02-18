using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Jaeger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OpenTracing;
using OpenTracing.Util;

namespace ClientApp
{
  public class Program
  {
    public static async Task Main(string[] args)
    {
      //using (ILoggerFactory loggerFactory = new LoggerFactory().AddConsole())
      using (ILoggerFactory loggerFactory = NullLoggerFactory.Instance)
      using (Tracer tracer = Tracing.Init("Client-app", loggerFactory))
      {
        // Register tracer globally.
        GlobalTracer.Register(tracer);

        IServiceCollection services = new ServiceCollection();

        services
          .AddHelloClient()
          .AddSingleton<ITracer>(tracer);

        using (ServiceProvider serviceProvider = services.BuildServiceProvider())
        {
          IHelloClient helloClient = serviceProvider.GetRequiredService<IHelloClient>();

          using (IScope scope = tracer.BuildActiveSpan("Parallel-tasks"))
          {
            IEnumerable<Task> tasks = Enumerable.Range(1, 5).Select(n => helloClient.SayHello($"Balazs #{n}"));

            await Task.WhenAll(tasks);
          }
        }
      }
    }
  }
}
