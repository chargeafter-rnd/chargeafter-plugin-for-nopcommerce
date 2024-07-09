using FluentValidation;
using Nop.Plugin.Payments.ChargeAfter.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Payments.ChargeAfter.Validators
{
    public class ConfigurationValidator : BaseNopValidator<ConfigurationModel>
    {
        #region Ctor

        public ConfigurationValidator(ILocalizationService localizationService)
        {
            RuleFor(model => model.SandboxPublicKey)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Plugins.Payments.ChargeAfter.Fields.SandboxPublicKey.Required").Result)
                .When(model => !model.UseProduction);

            RuleFor(model => model.SandboxPrivateKey)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Plugins.Payments.ChargeAfter.Fields.SandboxPrivateKey.Required").Result)
                .When(model => !model.UseProduction);

            RuleFor(model => model.ProductionPublicKey)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Plugins.Payments.ChargeAfter.Fields.ProductionPublicKey.Required").Result)
                .When(model => model.UseProduction);

            RuleFor(model => model.ProductionPrivateKey)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Plugins.Payments.ChargeAfter.Fields.ProductionPrivateKey.Required").Result)
                .When(model => model.UseProduction);

            RuleFor(model => model.FinancingPageUrlLineOfCreditPromo)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Plugins.Payments.ChargeAfter.Fields.ProductionPublicKey.Required").Result)
                .When(model => model.EnableLineOfCreditPromo);
        }

        #endregion
    }
}
