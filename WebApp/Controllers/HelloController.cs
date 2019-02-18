using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Microsoft.AspNetCore.Mvc;
using OpenTracing;

namespace WebApp.Controllers
{
  [Route("[controller]")]
  [ApiController]
  public class HelloController : ControllerBase
  {
    private readonly Random _random = new Random();

    private readonly HttpStatusCode[] _httpStatusCodes = new HttpStatusCode[]
    {
      HttpStatusCode.BadRequest,  // Polly won't retry for this.
      HttpStatusCode.NotFound,    // Polly won't retry for this.
      HttpStatusCode.RequestTimeout,
      HttpStatusCode.RequestTimeout,
      HttpStatusCode.InternalServerError,
      HttpStatusCode.InternalServerError,
      HttpStatusCode.InternalServerError,
      HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.OK,
      HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.OK,
    };

    private readonly ITracer _tracer;

    public HelloController(ITracer tracer)
    {
      _tracer = tracer;
    }

    // This method is called by the RefitTestController.
    [HttpGet("{name}")]
    public async Task<ActionResult<string>> Get(string name, CancellationToken ct)
    {
      using (IScope scope = _tracer.BuildActiveSpan("Hello-get"))
      {
        ISpan span = scope.Span;

        span.LogMessage("baggage", span.GetBaggageItem("baggage"));

        // Here you can have TaskCanceledException, which will be handled by ExceptionHandlingMiddleware.
        await Task.Delay(_random.Next(100, 250), ct);

        HttpStatusCode selectedStatusCode = _httpStatusCodes[_random.Next(_httpStatusCodes.Length)];

        span.LogMessage("name", name);
        span.Log($"Selected status code: {selectedStatusCode}");

        // --> Return OK.
        if (selectedStatusCode == HttpStatusCode.OK)
          return Ok($"Hello {name}.");

        // --> Delay.
        if (selectedStatusCode == HttpStatusCode.RequestTimeout)
        {
          try
          {
            // If your method do not accept token in the argument, you can check it here beforehand.
            ct.ThrowIfCancellationRequested();

            await Task.Delay(5000, ct);
          }
          catch (OperationCanceledException)
          {
            span.Log("The operation was canceled.");

            return NoContent();
          }

          // The timeout policy cancel this call earlier, so you won't see this line.
          span.Log($"After the delay.");
        }

        if (selectedStatusCode == HttpStatusCode.InternalServerError)
          throw new Exception("Throw exception for test purpose.");

        // --> Other returns.
        return new ContentResult
        {
          StatusCode = (int)selectedStatusCode,
          Content    = $"Selected status code: {selectedStatusCode}"
        };
      }
    }
  }
}
