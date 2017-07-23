using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReactivePlayground.AspNetRx
{
    public class HttpReactiveRequestBodyFeature
    {
        public IObservable<ArraySegment<byte>> RequestBody { get; set; }
    }

    public class HttpReactiveResponseBodyFeature
    {
        public IObserver<object> ResponseBody { get; set; }
    }
}
