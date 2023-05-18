using Nop.Plugin.Payments.ChargeAfter.Core.Http;
using System.Net.Http.Headers;

namespace Nop.Plugin.Payments.ChargeAfter.Core
{
    public class ChargeAfterHttpClient : HttpClient
    {
        private IInjector gzipInjector;
        private IInjector authorizationInjector;

        public ChargeAfterHttpClient(ChargeAfterEnvironment environment) : base(environment)
        {
            gzipInjector = new GzipInjector();
            authorizationInjector = new AuthorizationInjector(this, environment);

            AddInjector(this.gzipInjector);
            AddInjector(this.authorizationInjector);
        }

        class AuthorizationInjector : IInjector
        {
            private HttpClient client;
            private ChargeAfterEnvironment environment;

            public AuthorizationInjector(HttpClient client, ChargeAfterEnvironment environment)
            {
                this.client = client;
                this.environment = environment;
            }

            public void Inject(HttpRequest request)
            {
                if (!request.Headers.Contains("Authorization") && !string.IsNullOrEmpty(this.environment.ClientPrivate()))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.environment.ClientPrivate());
                }
            }
        }

        private class GzipInjector : IInjector
        {
            public void Inject(HttpRequest request)
            {
                request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            }
        }
    }
}
