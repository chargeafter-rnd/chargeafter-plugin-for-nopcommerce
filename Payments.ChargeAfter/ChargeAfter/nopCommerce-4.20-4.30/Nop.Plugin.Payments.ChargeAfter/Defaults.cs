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

        public static string UserAgent => $"nopCommerce-{NopVersion.CurrentVersion}";

        public static string NonLeasableAttribute = "ChargeAfterNonLeasable";

        public static string WarrantyAttribute = "ChargeAfterWarranty";

        public const string PAYMENT_INFO_VIEW_COMPONENT_NAME = "ChargeAfterPaymentInfo";

        public const string CHECKOUT_SCRIPT_VIEW_COMPONENT_NAME = "ChargeAfterCheckoutScript";

        public const string PROMO_SCRIPT_VIEW_COMPONENT_NAME = "ChargeAfterPromoScript";

        public const string PROMO_LINE_OF_CREDIT_VIEW_COMPONENT_NAME = "ChargeAfterPromoLineOfCredit";

        public const string PROMO_SIMPLE_GLOBAL_VIEW_COMPONENT_NAME = "ChargeAfterPromoSimpleGlobal";

        public const string PROMO_SIMPLE_PRODUCT_VIEW_COMPONENT_NAME = "ChargeAfterPromoSimpleProduct";

        public const string ADMIN_ORDER_VIEW_COMPONENT_NAME = "ChargeAfterAdminOrder";

        public const string ADMIN_PRODUCT_VIEW_COMPONENT_NAME = "ChargeAfterAdminProduct";
    }
}
