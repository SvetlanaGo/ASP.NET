using MassTransit;
using Pcf.Administration.Core.Messages;
using Pcf.Administration.Core.Services;
using Pcf.Contracts.Events;
using System.Threading.Tasks;

namespace Pcf.Administration.WebHost.Consumers
{
    public class AdminPromoCodeIssuedConsumer : IConsumer<IPromoCodeIssuedEvent>
    {
        private readonly IEmployeeService _employeeService;

        public AdminPromoCodeIssuedConsumer(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        public async Task Consume(ConsumeContext<IPromoCodeIssuedEvent> context)
        {
            if (context.Message.PartnerManagerId.HasValue)
            {
                var message = new UpdateEmployeePromocodesMessage
                {
                    EmployeeId = context.Message.PartnerManagerId.Value
                };

                await _employeeService.UpdateAppliedPromocodesAsync(message);
            }
        }
    }
}