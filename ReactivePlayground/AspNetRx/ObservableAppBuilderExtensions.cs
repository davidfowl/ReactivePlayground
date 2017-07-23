using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace ReactivePlayground.AspNetRx
{
    public static class ObservableAppBuilderExtensions
    {
        public static void Run<T>(this IApplicationBuilder app, Func<HttpContext, IObserver<T>> requestDelegate)
        {
            app.Run(context =>
            {
                requestDelegate(context).OnCompleted();
                return Task.CompletedTask;
            });
        }

        public static void Run(this IApplicationBuilder app, ObservableRequestDelegate requestDelegate)
        {
            app.Run(async context =>
            {
                await requestDelegate(context);
            });
        }

        public static IApplicationBuilder Use(this IApplicationBuilder app, Func<ObservableRequestDelegate, ObservableRequestDelegate> middleware)
        {
            return app.Use((RequestDelegate next) =>
            {
                var del = middleware(context =>
                {
                    return Observable.FromAsync(() => next(context));
                });

                return async context =>
                {
                    await del(context);
                };
            });
        }
    }
}
