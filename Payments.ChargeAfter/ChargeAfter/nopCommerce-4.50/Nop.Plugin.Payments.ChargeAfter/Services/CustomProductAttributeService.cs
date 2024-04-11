using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Common;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.ChargeAfter.Services
{
    public interface ICustomProductAttributeService
    {
        public Task<bool> GetNonLeasableAttributeValueAsync(Product product);

        public Task SetNonLeasableAttributeValueAsync(Product product, bool nonLeasableValue);

        public Task<bool> GetWarrantyAttributeValueAsync(Product product);

        public Task SetWarrantyAttributeValueAsync(Product product, bool warrantyValue);
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

        private Task<bool> GetAttributeValue(Product product, string attributeName)
            => _genericAttributeService.GetAttributeAsync<bool>(
                product,
                attributeName,
                _storeContext.GetCurrentStore()?.Id ?? 0,
                false
            );

        private Task SetAttributeValue(Product product, string attributeName, bool attributeValue)
            => _genericAttributeService.SaveAttributeAsync<bool>(
                product,
                attributeName,
                attributeValue,
                _storeContext.GetCurrentStore()?.Id ?? 0
            );

        public Task<bool> GetNonLeasableAttributeValueAsync(Product product)
            => GetAttributeValue(product, Defaults.NonLeasableAttribute);

        public Task SetNonLeasableAttributeValueAsync(Product product, bool nonLeasableValue)
            => SetAttributeValue(product, Defaults.NonLeasableAttribute, nonLeasableValue);

        public Task<bool> GetWarrantyAttributeValueAsync(Product product)
            => GetAttributeValue(product, Defaults.WarrantyAttribute);

        public Task SetWarrantyAttributeValueAsync(Product product, bool warrantyValue)
          => SetAttributeValue(product, Defaults.WarrantyAttribute, warrantyValue);

        #endregion
    }
}