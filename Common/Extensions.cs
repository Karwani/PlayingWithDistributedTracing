using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using OpenTracing;
using OpenTracing.Propagation;
using OpenTracing.Tag;
using OpenTracing.Util;

namespace Common
{
  public static class Extensions
  {
    #region Extension methods for ITracer.

    // StartActive() automatically creates a ChildOf reference to the previously active span.
    public static IScope BuildActiveSpan(this ITracer tracer, string operationName)
      => tracer.BuildSpan(operationName).StartActive(true);

    public static Dictionary<string, string> InjectWithTags(this ITracer tracer, string httpMethod, string httpUrl)
    {
      ISpan activeSpan = tracer.ActiveSpan;

      activeSpan
        .SetTag(Tags.SpanKind, Tags.SpanKindClient)
        .SetTag(Tags.HttpMethod, httpMethod)
        .SetTag(Tags.HttpUrl, httpUrl);

      var dictionary = new Dictionary<string, string>();

      tracer.Inject(activeSpan.Context, BuiltinFormats.HttpHeaders, new TextMapInjectAdapter(dictionary));

      return dictionary;
    }

    #endregion

    #region Extension methods for ISpan.

    public static ISpan LogMessage(this ISpan span, string message)
      => span.LogMessage(LogFields.Message, message);

    public static ISpan LogMessage(this ISpan span, string key, string message)
      => span.Log(new Dictionary<string, object> { [key] = message });

    public static ISpan LogError(this ISpan span, Exception ex, string message)
    {
      span.SetTag(Tags.Error, true);

      var fields = new Dictionary<string, object>
      {
        [LogFields.Message]     = message,
        [LogFields.ErrorObject] = ex?.GetType().Name,
        ["Exception"]           = ex
      };

      return span.Log(fields);
    }

    #endregion

    #region Extension methods for HttpHeaders.

    public static void InjectTracing(this HttpHeaders httpHeaders, string httpMethod, string httpUrl)
      => httpHeaders.InjectTracing(GlobalTracer.Instance, httpMethod, httpUrl);

    public static void InjectTracing(this HttpHeaders httpHeaders, ITracer tracer, string httpMethod, string httpUrl)
    {
      if (tracer is null)
        throw new ArgumentNullException(nameof(tracer));

      Dictionary<string, string> dictionary = tracer.InjectWithTags(httpMethod, httpUrl);

      foreach (var entry in dictionary)
        httpHeaders.Add(entry.Key, entry.Value);
    }

    #endregion

    // This method is use for Instrumenting the Server manually.
    // https://github.com/yurishkuro/opentracing-tutorial/tree/master/csharp/src/lesson03#instrumenting-the-server-manually
    //public static IScope StartServerSpan(this ITracer tracer, IDictionary<string, string> headers, string operationName)
    //{
    //  ISpanBuilder spanBuilder;

    //  try
    //  {
    //    ISpanContext parentSpanContext = tracer.Extract(BuiltinFormats.HttpHeaders, new TextMapExtractAdapter(headers));

    //    spanBuilder = tracer.BuildSpan(operationName);

    //    if (parentSpanContext != null)
    //      spanBuilder = spanBuilder.AsChildOf(parentSpanContext);
    //  }
    //  catch (Exception)
    //  {
    //    spanBuilder = tracer.BuildSpan(operationName);
    //  }

    //  return spanBuilder.WithTag(Tags.SpanKind, Tags.SpanKindServer).StartActive(true);
    //}
  }
}
