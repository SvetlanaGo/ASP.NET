using Pcf.Administration.Core.Messages;
using System.Threading.Tasks;

namespace Pcf.Administration.Core.Services
{
    public interface IEmployeeService
    {
        Task UpdateAppliedPromocodesAsync(UpdateEmployeePromocodesMessage message);
    }
}