using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ReactivePlayground.AspNetRx
{
    public static class ObservableHttpResponseExtensons
    {
        public static IObserver<object> Body(this HttpResponse httpResponse)
        {
            var feature = httpResponse.HttpContext.Features.Get<HttpReactiveResponseBodyFeature>();
            if (feature == null)
            {
                feature = new HttpReactiveResponseBodyFeature();
                var task = Task.CompletedTask;
                feature.ResponseBody = Observer.Create<object>(async data =>
                {
                    // Implicit task queue to make sure we don't do overlappning operations
                    await task;

                    switch (data)
                    {
                        case string value:
                            task = httpResponse.WriteAsync(value);
                            break;
                        case byte[] buffer:
                            task = httpResponse.Body.WriteAsync(buffer, 0, buffer.Length);
                            break;
                        case ArraySegment<byte> buffer:
                            task = httpResponse.Body.WriteAsync(buffer.Array, buffer.Offset, buffer.Count);
                            break;
                        default:
                            break;
                    }
                });

                httpResponse.HttpContext.Features.Set(feature);
            }

            return feature.ResponseBody;
        }

        public static void OnCompleted(this HttpResponse httpResponse, Func<IObservable<Unit>> callback)
        {
            httpResponse.OnCompleted(async () => await callback());
        }

        public static void OnCompleted(this HttpResponse httpResponse, Func<object, IObservable<Unit>> callback, object state)
        {
            httpResponse.OnCompleted(async (s) => await callback(s), state);
        }
    }
}
