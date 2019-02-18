using Jaeger;
using Jaeger.Reporters;
using Jaeger.Samplers;
using Jaeger.Senders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OpenTracing;

namespace Common
{
  public static class Tracing
  {
    public static Tracer Init(string serviceName, ILoggerFactory loggerFactory = null)
    {
      RemoteReporter reporter = new RemoteReporter
        .Builder()
        .WithSender(new UdpSender())
        //.WithSender(new UdpSender(UdpSender.DefaultAgentUdpHost, UdpSender.DefaultAgentUdpCompactPort, 0))
        .WithLoggerFactory(loggerFactory ?? NullLoggerFactory.Instance)
        .Build();

      ISampler sampler = getSampler(ConstSampler.Type);

      Tracer tracer = new Tracer
        .Builder(serviceName)
        .WithReporter(reporter)
        .WithSampler(sampler)
        .Build();

      return tracer;
    }

    public static Tracer Dafault(string serviceName)
      => new Tracer.Builder(serviceName)
          .WithReporter(new NoopReporter())
          .WithSampler(new ConstSampler(false))
          .Build();

    public static Tracer InitOther(string serviceName, ILoggerFactory loggerFactory)
    {
      var samplerConfiguration = new Configuration.SamplerConfiguration(loggerFactory)
        .WithType(ConstSampler.Type)
        .WithParam(1);

      var reporterConfiguration = new Configuration.ReporterConfiguration(loggerFactory)
        .WithLogSpans(true);

      ITracer tracer = new Configuration(serviceName, loggerFactory)
        .WithSampler(samplerConfiguration)
        .WithReporter(reporterConfiguration)
        .GetTracer();

      return tracer as Tracer;
    }

    private static ISampler getSampler(string type)
    {
      switch (type)
      {
        case "const": return new ConstSampler(true);
        case "rate": return new RateLimitingSampler(5);
        case "probabilistic": return new ProbabilisticSampler(0.2);
        default: return new ConstSampler(true);
      }
    }
  }
}
