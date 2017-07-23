using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ReactivePlayground.AspNetRx
{
    public static class ObservableHttpResponseExtensons
    {
        public static IObserver<object> Body(this HttpResponse httpResponse)
        {
            // TODO: Store in a feature
            var task = Task.CompletedTask;
            return Observer.Create<object>(async data =>
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
