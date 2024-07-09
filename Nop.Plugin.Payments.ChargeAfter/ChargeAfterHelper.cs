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

        #endregion
    }
}
