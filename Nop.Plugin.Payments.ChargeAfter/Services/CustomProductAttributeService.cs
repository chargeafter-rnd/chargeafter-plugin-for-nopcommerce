using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Common;

namespace Nop.Plugin.Payments.ChargeAfter.Services
{
    public interface ICustomProductAttributeService
    {
        public bool GetNonLeasableAttributeValue(Product product);

        public void SetNonLeasableAttributeValue(Product product, bool nonLeasableValue);

        public bool GetWarrantyAttributeValue(Product product);

        public void SetWarrantyAttributeValue(Product product, bool warrantyValue);
    }

    public class CustomProductAttributeService : ICustomProductAttributeService
    {
        #region Fields

        private readonly IGenericAttributeService _genericAttributeService;
        
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public CustomProductAttributeService(
            IGenericAttributeService genericAttributeService,
            IStoreContext storeContext
        )
        {
            _genericAttributeService = genericAttributeService;
            _storeContext = storeContext;
        }

        #endregion

        #region Methods

        private bool GetAttributeValue(Product product, string attributeName) 
            => _genericAttributeService.GetAttribute<bool>(
                product,
                attributeName,
                _storeContext.CurrentStore?.Id ?? 0,
                false
            );

        private void SetAttributeValue(Product product, string attributeName, bool attributeValue) 
            => _genericAttributeService.SaveAttribute<bool>(
                product, 
                attributeName,
                attributeValue,
                _storeContext.CurrentStore?.Id ?? 0
            );

        public bool GetNonLeasableAttributeValue(Product product)
            => GetAttributeValue(product, Defaults.NonLeasableAttribute);

        public void SetNonLeasableAttributeValue(Product product, bool nonLeasableValue)
            => SetAttributeValue(product, Defaults.NonLeasableAttribute, nonLeasableValue);

        public bool GetWarrantyAttributeValue(Product product)
            => GetAttributeValue(product, Defaults.WarrantyAttribute);

        public void SetWarrantyAttributeValue(Product product, bool warrantyValue) 
            => SetAttributeValue(product, Defaults.WarrantyAttribute, warrantyValue);

        #endregion
    }
}
