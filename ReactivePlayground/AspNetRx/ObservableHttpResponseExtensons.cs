using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ReactivePlayground.AspNetRx
{
    public static class ObservableHttpResponseExtensons
    {
        public static IObserver<ArraySegment<byte>> Body(this HttpResponse httpResponse)
        {
            var feature = httpResponse.HttpContext.Features.Get<HttpReactiveResponseBodyFeature>();
            if (feature == null)
            {
                feature = new HttpReactiveResponseBodyFeature();
                var task = Task.CompletedTask;
                feature.ResponseBody = Observer.Create<ArraySegment<byte>>(async buffer =>
                {
                    // Implicit task queue to make sure we don't do overlapping operations
                    await task;
                    task = httpResponse.Body.WriteAsync(buffer.Array, buffer.Offset, buffer.Count);
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
            httpResponse.OnCompleted(async s => await callback(s), state);
        }
    }
}
