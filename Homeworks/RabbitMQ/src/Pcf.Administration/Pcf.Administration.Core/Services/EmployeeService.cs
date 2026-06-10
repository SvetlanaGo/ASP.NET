using Pcf.Administration.Core.Abstractions.Repositories;
using Pcf.Administration.Core.Domain.Administration;
using Pcf.Administration.Core.Messages;
using System;
using System.Threading.Tasks;

namespace Pcf.Administration.Core.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IRepository<Employee> _employeesRepository;

        public EmployeeService(IRepository<Employee> employeesRepository)
        {
            _employeesRepository = employeesRepository;
        }

        public async Task UpdateAppliedPromocodesAsync(UpdateEmployeePromocodesMessage message)
        {
            var employee = await _employeesRepository.GetByIdAsync(message.EmployeeId);

            if (employee == null)
            {
                throw new EmployeeNotFoundException(message.EmployeeId);
            }

            employee.AppliedPromocodesCount++;
            await _employeesRepository.UpdateAsync(employee);
        }
    }

    public class EmployeeNotFoundException : Exception
    {
        public Guid EmployeeId { get; }

        public EmployeeNotFoundException(Guid employeeId)
            : base($"Employee with id {employeeId} not found")
        {
            EmployeeId = employeeId;
        }
    }
}