using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace Nop.Plugin.Payments.ChargeAfter.Core.Http
{
    public class HttpException : IOException
    {
        public HttpStatusCode StatusCode { get; }
        public HttpHeaders Headers { get; }

        public HttpException(HttpStatusCode statusCode, HttpHeaders headers, string message) : base(message)
        {
            StatusCode = statusCode;
            Headers = headers;
        }
    }
}
