using System;

namespace Nop.Plugin.Payments.ChargeAfter.Payments
{
    public class ChargeState
    {
        #region Fields

        public static string AUTHORIZED = "AUTHORIZED";

        public static string SETTLED = "SETTLED";

        public static string PARTIALLY_SETTLED = "PARTIALLY_SETTLED";

        public static string EXPIRED = "EXPIRED";

        public static string VOIDED = "VOIDED";

        public static string REFUNDED = "REFUNDED";

        public static string FULLY_REFUNDED = "FULLY_REFUNDED";

        public static string PARTIALLY_REFUNDED = "PARTIALLY_REFUNDED";

        public static string CHARGEBACK = "CHARGEBACK";

        #endregion
    }
}
