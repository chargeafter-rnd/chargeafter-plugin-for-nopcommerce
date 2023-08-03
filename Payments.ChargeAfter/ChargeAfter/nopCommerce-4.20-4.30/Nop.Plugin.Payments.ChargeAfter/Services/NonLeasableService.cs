using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Common;

namespace Nop.Plugin.Payments.ChargeAfter.Services
{
    public interface INonLeasableService
    {
        public bool GetAttributeValue(Product product);

        public void SetAttributeValue(Product product, bool nonLeasableValue);
    }

    public class NonLeasableService : INonLeasableService
    {
        #region Fields

        private readonly IGenericAttributeService _genericAttributeService;
        
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public NonLeasableService(
            IGenericAttributeService genericAttributeService,
            IStoreContext storeContext
        )
        {
            _genericAttributeService = genericAttributeService;
            _storeContext = storeContext;
        }

        #endregion

        #region Methods

        public bool GetAttributeValue(Product product) => _genericAttributeService.GetAttribute<bool>(
                product,
                Defaults.NonLeasableAttribute,
                _storeContext.CurrentStore?.Id ?? 0,
                false
            );

        public void SetAttributeValue(Product product, bool nonLeasableValue) => _genericAttributeService.SaveAttribute<bool>(
                product, 
                Defaults.NonLeasableAttribute,
                nonLeasableValue,
                _storeContext.CurrentStore?.Id ?? 0
            );

        #endregion
    }
}
