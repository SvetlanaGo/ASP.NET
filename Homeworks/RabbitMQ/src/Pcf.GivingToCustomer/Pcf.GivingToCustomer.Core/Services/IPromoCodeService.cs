using Pcf.GivingToCustomer.Core.Messages;
using System.Threading.Tasks;

namespace Pcf.GivingToCustomer.Core.Services
{
    public interface IPromoCodeService
    {
        Task GivePromoCodesToCustomersWithPreferenceAsync(GivePromoCodeMessage message);
    }
}