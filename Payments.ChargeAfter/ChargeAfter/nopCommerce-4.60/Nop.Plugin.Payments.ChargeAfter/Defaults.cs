using Nop.Core;

namespace Nop.Plugin.Payments.ChargeAfter
{
    public class Defaults
    {
        public static string SystemName => "Payments.ChargeAfter";

        public static string ConfigurationRouteName => "Admin/PaymentChargeAfter/Configure";

        public static string ConfirmationTokenRouteName => "Plugin.Payments.ChargeAfter.ConfirmationToken";

        public static string CheckoutRouteName => "Plugin.Payments.ChargeAfter.CheckoutFlow";

        public static string CheckoutRoute => "Plugins/ChargeAfter/CheckoutFlow";

        public static string CheckoutPlaceRouteName => "Plugin.Payments.ChargeAfter.CheckoutFlowPlace";

        public static string CheckoutPlaceRoute => "Plugins/ChargeAfter/CheckoutPlace";

        public static string CheckoutSuccessRouteName => "Plugin.Payments.ChargeAfter.CheckoutFlowSuccess";

        public static string CheckoutSuccessRoute => "Plugins/ChargeAfter/CheckoutFlowSuccess";

        public static string CheckoutFailedRouteName => "Plugin.Payments.ChargeAfter.CheckoutFlowFailed";

        public static string CheckoutFailedRoute => "Plugins/ChargeAfter/CheckoutFlowFailed";

        public static string UserAgent => $"nopCommerce-{NopVersion.CURRENT_VERSION}";

        public static string NonLeasableAttribute = "ChargeAfterNonLeasable";

        public static string WarrantyAttribute = "ChargeAfterWarranty";
    }
}
