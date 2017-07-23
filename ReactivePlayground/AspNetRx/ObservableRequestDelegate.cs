using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ReactivePlayground.AspNetRx
{
    public delegate IObservable<Unit> ObservableRequestDelegate(HttpContext httpContext);
}
