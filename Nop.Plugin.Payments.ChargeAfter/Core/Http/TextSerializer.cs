﻿using System;
using System.Net.Http;

namespace Nop.Plugin.Payments.ChargeAfter.Core.Http
{
    public class TextSerializer : ISerializer
    {

        public object Decode(HttpContent content, Type responseType)
        {
            return content.ReadAsStringAsync().Result;
        }

        public string GetContentTypeRegexPattern()
        {
            return "^text/.*$";
        }

        public HttpContent Encode(HttpRequest request)
        {
            return new StringContent(request.Body.ToString());
        }
    }
}
