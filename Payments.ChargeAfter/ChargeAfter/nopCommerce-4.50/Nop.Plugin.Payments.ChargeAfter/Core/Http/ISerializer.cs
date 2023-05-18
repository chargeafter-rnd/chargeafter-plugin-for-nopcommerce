using System;
using System.Net.Http;

namespace Nop.Plugin.Payments.ChargeAfter.Core.Http
{
    public interface ISerializer
    {
        string GetContentTypeRegexPattern();
        HttpContent Encode(HttpRequest request);
        object Decode(HttpContent content, Type responseType);
    }
}
