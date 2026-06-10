using MassTransit;
using Pcf.Contracts.Events;
using Pcf.GivingToCustomer.Core.Messages;
using Pcf.GivingToCustomer.Core.Services;
using System.Threading.Tasks;

namespace Pcf.GivingToCustomer.WebHost.Consumers
{
    public class PromoCodeIssuedConsumer : IConsumer<IPromoCodeIssuedEvent>
    {
        private readonly IPromoCodeService _promoCodeService;

        public PromoCodeIssuedConsumer(IPromoCodeService promoCodeService)
        {
            _promoCodeService = promoCodeService;
        }

        public async Task Consume(ConsumeContext<IPromoCodeIssuedEvent> context)
        {
            var message = new GivePromoCodeMessage
            {
                ServiceInfo = context.Message.ServiceInfo,
                PartnerId = context.Message.PartnerId,
                PromoCodeId = context.Message.PromoCodeId,
                PromoCode = context.Message.PromoCode,
                PreferenceId = context.Message.PreferenceId,
                BeginDate = context.Message.BeginDate.ToString("yyyy-MM-dd"),
                EndDate = context.Message.EndDate.ToString("yyyy-MM-dd")
            };

            await _promoCodeService.GivePromoCodesToCustomersWithPreferenceAsync(message);
        }
    }
}