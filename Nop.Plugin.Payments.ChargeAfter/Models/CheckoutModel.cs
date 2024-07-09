using Nop.Plugin.Payments.ChargeAfter.Domain;
using Nop.Web.Framework.Models;
using System.Collections.Generic;

namespace Nop.Plugin.Payments.ChargeAfter.Models
{
    public record CheckoutModel : BaseNopModel
    {
        public CheckoutModel()
        {
            Items = new List<CheckoutItemModel>();
            DiscountItems = new List<CheckoutDiscountItemModel>();
        }

        public ChargeAfterCheckoutUI ChargeAfterCheckoutUI { get; set; }

        public string CaPublicKey { get; set; }

        public string CaHost { get; set; }

        public IList<CheckoutItemModel> Items { get; set; }

        public IList<CheckoutDiscountItemModel> DiscountItems { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal TotalTaxAmount { get; set; }

        public decimal TotalShippingAmount { get; set; }

        public string OrderId { get; set; }

        public string OrderGuild { get; set; }

        #region Nested Classes

        public partial record CheckoutItemModel : BaseNopEntityModel
        {
            public string Sku { get; set; }

            public int ProductId { get; set; }

            public string Name { get; set; }

            public decimal UnitPrice { get; set; }

            public int Quantity { get; set; }

            public bool Leasable { get; set; }

            public WarrantyItemModel Warranty { get; set; }

            public partial record WarrantyItemModel
            {
                public string Name { get; set; }

                public decimal Price { get; set; }

                public string Sku { get; set; }
            }
        }

        public partial record CheckoutDiscountItemModel : BaseNopEntityModel
        {

            public string Name { get; set; }

            public string Code { get; set; }

            public int DiscountId { get; set; }

            public decimal Amount { get; set; }
        }

        #endregion

    }
}
