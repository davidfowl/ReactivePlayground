using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ReactivePlayground.AspNetRx
{
    public static class ObservableHttpRequestExtensons
    {
        public static IObservable<ArraySegment<byte>> Body(this HttpRequest httpRequest)
        {
            var feature = httpRequest.HttpContext.Features.Get<HttpReactiveRequestBodyFeature>();
            if (feature == null)
            {
                feature = new HttpReactiveRequestBodyFeature();
                feature.RequestBody = Observable.Create<ArraySegment<byte>>(async (observer, token) =>
                {
                    try
                    {
                        await httpRequest.Body.CopyToAsync(new ObserverStream(observer), 4096, token);
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                    }
                    finally
                    {
                        observer.OnCompleted();
                    }
                });

                httpRequest.HttpContext.Features.Set(feature);
            }

            return feature.RequestBody;
        }

        // Stream that pushes via the observer
        private class ObserverStream : Stream
        {
            private readonly IObserver<ArraySegment<byte>> _observer;

            public ObserverStream(IObserver<ArraySegment<byte>> observer)
            {
                _observer = observer;
            }

            public override bool CanRead => false;

            public override bool CanSeek => false;

            public override bool CanWrite => true;

            public override long Length => throw new NotSupportedException();

            public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

            public override void Flush()
            {

            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                _observer.OnNext(new ArraySegment<byte>(buffer, offset, count));
            }

            public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                _observer.OnNext(new ArraySegment<byte>(buffer, offset, count));
                return Task.CompletedTask;
            }
        }
    }
}
