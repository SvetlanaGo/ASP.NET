using HotChocolate;
using Pcf.GivingToCustomer.Core.Abstractions.Repositories;
using Pcf.GivingToCustomer.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pcf.GivingToCustomer.WebHost.GraphQL
{
    public class CustomersQuery
    {
        public async Task<List<Customer>> GetCustomers(
            [Service] IRepository<Customer> repository)
        {
            return (await repository.GetAllAsync()).ToList();
        }

        public async Task<Customer> GetCustomer(
            Guid id,
            [Service] IRepository<Customer> repository)
        {
            return await repository.GetByIdAsync(id);
        }
    }
}