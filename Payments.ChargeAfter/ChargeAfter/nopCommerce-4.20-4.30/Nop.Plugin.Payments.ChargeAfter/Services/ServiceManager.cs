using Microsoft.Net.Http.Headers;
using Nop.Core;
using Nop.Plugin.Payments.ChargeAfter.Core;
using Nop.Plugin.Payments.ChargeAfter.Core.Http;
using Nop.Plugin.Payments.ChargeAfter.Charges;
using Nop.Plugin.Payments.ChargeAfter.Payments;
using System;
using System.Linq;

namespace Nop.Plugin.Payments.ChargeAfter.Services
{
    public class ServiceManager
    {
        #region Methods

        public (Charge Charge, string ErrorMessage) Authorization(ChargeAfterPaymentSettings chargeAfterPaymentSettings, string confirmationToken) {
            return HandleFunction(chargeAfterPaymentSettings, () =>
            {
                var authorizeRequest = new AuthorizeRequest();

                authorizeRequest.ConfirmationToken = confirmationToken;

                var request = new ChargeAuthorizeRequest().RequestBody(authorizeRequest);
                return HandleCheckoutRequest<ChargeAuthorizeRequest, Charge>(chargeAfterPaymentSettings, request);
            });
        }

        public (Charge Charge, string ErrorMessage) SetMerchantOrderId(ChargeAfterPaymentSettings chargeAfterPaymentSettings, string chargeId, string orderId) {
            return HandleFunction(chargeAfterPaymentSettings, () =>
            {
                var merchantOrderIdRequest = new MerchantOrderIdRequest();

                merchantOrderIdRequest.MerchantOrderId = orderId;

                var request = new ChargeMerchantOrderIdRequest(chargeId).RequestBody(merchantOrderIdRequest);
                return HandleCheckoutRequest<ChargeMerchantOrderIdRequest, Charge>(chargeAfterPaymentSettings, request);
            });
        }

        public (Charge Charge, string ErrorMessage) GetChargeById(ChargeAfterPaymentSettings chargeAfterPaymentSettings, string chargeId) {
            return HandleFunction(chargeAfterPaymentSettings, () =>
            {
                var request = new ChargeGetRequest(chargeId);
                return HandleCheckoutRequest<ChargeGetRequest, Charge>(chargeAfterPaymentSettings, request);
            });
        }

        public (ChargeCapture ChargeCapture, string ErrorMessage) Capture(ChargeAfterPaymentSettings chargeAfterPaymentSettings, string chargeId, decimal amount) {
            return HandleFunction(chargeAfterPaymentSettings, () =>
            {
                var captureRequest = new CaptureRequest();

                captureRequest.Amount = amount;

                var request = new ChargeCaptureRequest(chargeId).RequestBody(captureRequest);
                return HandleCheckoutRequest<ChargeCaptureRequest, ChargeCapture>(chargeAfterPaymentSettings, request);
            });
        }
        
        public (ChargeVoid ChargeVoid, string ErrorMessage) Void(ChargeAfterPaymentSettings chargeAfterPaymentSettings, string chargeId) {
            return HandleFunction(chargeAfterPaymentSettings, () =>
            {
                var request = new ChargeVoidRequest(chargeId);
                return HandleCheckoutRequest<ChargeVoidRequest, ChargeVoid>(chargeAfterPaymentSettings, request);
            });
        }

        public (ChargeRefund ChargeRefund, string ErrorMessage) Refund(ChargeAfterPaymentSettings chargeAfterPaymentSettings, string chargeId, decimal amount)
        {
            return HandleFunction(chargeAfterPaymentSettings, () =>
            {
                var refundRequest = new RefundRequest();

                refundRequest.Amount = amount;

                var request = new ChargeRefundRequest(chargeId).RequestBody(refundRequest);
                return HandleCheckoutRequest<ChargeRefundRequest, ChargeRefund>(chargeAfterPaymentSettings, request);
            });
        }

        private (TResult Result, string ErrorMessage) HandleFunction<TResult>(ChargeAfterPaymentSettings chargeAfterPaymentSettings, Func<TResult> function)
        {
            try
            {
                //ensure that plugin is configured
                if (!IsConfigured(chargeAfterPaymentSettings))
                    throw new NopException("Plugin not configured");

                //invoke function
                return (function(), default);
            }
            catch (Exception exception)
            {
                if (exception.InnerException != null && exception.InnerException is HttpException httpException)
                {
                    if (httpException?.Body?.Errors != null)
                    {
                        var error = httpException.Body.Errors.FirstOrDefault(r => r.Description != null);
                        if (error != null)
                        {
                            return (default, error.Description);
                        }
                    }
                }

                return (default, "Request error occurred");
            }
        }

        private TResult HandleCheckoutRequest<TRequest, TResult>(ChargeAfterPaymentSettings settings, TRequest request, bool isExternal = false)
            where TRequest : HttpRequest where TResult : class
        {
            //prepare common request params
            request.Headers.Add(HeaderNames.UserAgent, Defaults.UserAgent);

            //execute request
            var clientPublic = ChargeAfterHelper.GetPublicKeyFromSettings(settings);
            var clientPrivate = ChargeAfterHelper.GetPrivateKeyFromSettings(settings);

            if (isExternal)
                clientPrivate = clientPublic;

            var environment = settings.UseProduction 
                ? new LiveEnvironment(clientPublic, clientPrivate, isExternal) as ChargeAfterEnvironment 
                : new SandboxEnvironment(clientPublic, clientPrivate, isExternal) as ChargeAfterEnvironment;

            var client = new ChargeAfterHttpClient(environment);
            var response = client.Execute(request)
                ?? throw new NopException("No response from the service.");

            //return the results if necessary
            if (typeof(TResult) == typeof(object))
                return default;

            var result = response.Result?.Result<TResult>()
                ?? throw new NopException("No response from the service.");

            return result;
        }

        private bool IsConfigured(ChargeAfterPaymentSettings settings)
        {
            //client id and secret are required to request services
            return !string.IsNullOrEmpty(ChargeAfterHelper.GetPublicKeyFromSettings(settings)) && !string.IsNullOrEmpty(ChargeAfterHelper.GetPrivateKeyFromSettings(settings));
        }

        #endregion
    }
}
