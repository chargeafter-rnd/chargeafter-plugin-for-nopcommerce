using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Common;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.ChargeAfter.Services
{
    public interface INonLeasableService
    {
        public Task<bool> GetAttributeValueAsync(Product product);
        public Task SetAttributeValueAsync(Product product, bool nonLeasableValue);
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

        public async Task<bool> GetAttributeValueAsync(Product product) => await _genericAttributeService.GetAttributeAsync<bool>(
                product,
                Defaults.NonLeasableAttribute,
                _storeContext.GetCurrentStore()?.Id ?? 0,
                false
            );

        public async Task SetAttributeValueAsync(Product product, bool nonLeasableValue) => await _genericAttributeService.SaveAttributeAsync<bool>(
                product,
                Defaults.NonLeasableAttribute,
                nonLeasableValue,
                _storeContext.GetCurrentStore()?.Id ?? 0
            );

        #endregion
    }
}