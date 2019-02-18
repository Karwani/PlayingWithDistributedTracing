using System;
using System.Net.Http;
using System.Threading.Tasks;
using Common;
using OpenTracing;
using OpenTracing.Tag;

namespace ClientApp
{
  public interface IHelloClient
  {
    Task SayHello(string helloTo);
  }

  public class HelloClient : IHelloClient
  {
    private readonly ITracer _tracer;
    private readonly HttpClient _httpClient;

    public HelloClient(ITracer tracer, HttpClient httpClient)
    {
      _tracer     = tracer;
      _httpClient = httpClient;
    }

    public async Task SayHello(string helloTo)
    {
      using (IScope scope = _tracer.BuildActiveSpan("Call-web-app")) // BuildActiveSpan: From common extensions.
      {
        ISpan span = scope.Span;

        span.SetBaggageItem("baggage", "baggage-value");

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"Hello/{helloTo}");

        request.Headers.InjectTracing( // InjectTracing: From common extensions.
          request.Method.Method, $"{_httpClient.BaseAddress}{request.RequestUri}");

        HttpResponseMessage response = null;

        try
        {
          response = await _httpClient.SendAsync(request);
        }
        catch (Exception ex)
        {
          span.LogError(ex, "Failed to call say hello."); // LogError: From common extensions.

          Console.WriteLine(ex.Message);

          return;
        }

        span.SetTag(Tags.HttpStatus, (int)response.StatusCode);

        string responseString = await response.Content.ReadAsStringAsync();

        span.LogMessage("response", responseString); // LogMessage: From common extensions.

        Console.WriteLine(responseString);
      }
    }
  }
}
