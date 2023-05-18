using Nop.Plugin.Payments.ChargeAfter.Domain;

namespace Nop.Plugin.Payments.ChargeAfter
{
    public class ChargeAfterHelper
    {
        #region Methods

        public static string GetPublicKeyFromSettings(ChargeAfterPaymentSettings chargeAfterPaymentSettings)
        {
            if (chargeAfterPaymentSettings.UseProduction && !string.IsNullOrEmpty(chargeAfterPaymentSettings.ProductionPublicKey)) {
                return chargeAfterPaymentSettings.ProductionPublicKey;
            }

            if (!chargeAfterPaymentSettings.UseProduction && !string.IsNullOrEmpty(chargeAfterPaymentSettings.SandboxPublicKey))
            {
                return chargeAfterPaymentSettings.SandboxPublicKey;
            }

            return string.Empty;
        }

        public static string GetPrivateKeyFromSettings(ChargeAfterPaymentSettings chargeAfterPaymentSettings)
        {
            if (chargeAfterPaymentSettings.UseProduction && !string.IsNullOrEmpty(chargeAfterPaymentSettings.ProductionPrivateKey)) {
                return chargeAfterPaymentSettings.ProductionPrivateKey;
            }

            if (!chargeAfterPaymentSettings.UseProduction && !string.IsNullOrEmpty(chargeAfterPaymentSettings.SandboxPrivateKey))
            {
                return chargeAfterPaymentSettings.SandboxPrivateKey;
            }

            return string.Empty;
        }

        public static string GetCaHostByUseProduction(bool useProduction)
        {
            return useProduction ? ".chargeafter.com" : "-sandbox.ca-dev.co";
        }

        public static string GetPromoWidgetType(PromoWidgetType selectedType)
        {
            var type = "default";
            switch(selectedType)
            {
                case PromoWidgetType.BannerHorizontal:
                    type = "banner-horizontal";
                    break;

                case PromoWidgetType.BannerVertical:
                    type = "banner-vertical";
                    break;
            }

            return type;
        }

        public static string GetBrandUrlFromSettings(ChargeAfterPaymentSettings chargeAfterPaymentSettings)
        {
            if(string.IsNullOrEmpty(chargeAfterPaymentSettings.BrandId)) {
                return string.Empty;
            }

            return GetBrandUrlFromCdn(chargeAfterPaymentSettings.UseProduction, chargeAfterPaymentSettings.BrandId);
        }

        public static string GetCdnUrl(bool useProduction)
        {
            return useProduction ? "https://storage.googleapis.com/cdn-production-bucket/" : "https://cdn-sandbox.ca-dev.co/";
        }

        public static string GetBrandUrlFromCdn(bool useProduction, string brandId)
        {
            return string.Concat(GetCdnUrl(useProduction), "assets/brands/", brandId, "/button.svg");
        }

        public static string GetSettingsUrlByMerchantFromCdn(bool useProduction, string merchantId)
        {
            return string.Concat(GetCdnUrl(useProduction), "assets/merchants/", merchantId, "/settings.json");
        }

        #endregion
    }
}
