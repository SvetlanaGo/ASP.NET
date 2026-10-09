using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Pcf.GivingToCustomer.Core.Abstractions.Repositories;
using Pcf.GivingToCustomer.Core.Domain;
using Pcf.GivingToCustomer.WebHost.Grpc;

namespace Pcf.GivingToCustomer.WebHost.GrpcServices
{
    public class CustomersGrpcService : CustomersGrpc.CustomersGrpcBase
    {
        private readonly IRepository<Customer> _customerRepository;

        public CustomersGrpcService(IRepository<Customer> customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public override async Task<GetCustomersResponse> GetCustomers(
            GetCustomersRequest request,
            ServerCallContext context)
        {
            var allCustomers = await _customerRepository.GetAllAsync();

            var page = request.Page > 0 ? request.Page : 1;
            var pageSize = request.PageSize > 0 ? request.PageSize : 10;

            var pagedCustomers = allCustomers
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var response = new GetCustomersResponse
            {
                TotalCount = allCustomers.Count()
            };

            response.Customers.AddRange(pagedCustomers.Select(customer => new CustomerShortGrpcResponse
            {
                Id = customer.Id.ToString(),
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email
            }));

            return response;
        }

        public override async Task<CustomerGrpcResponse> GetCustomerById(
            GetCustomerByIdRequest request,
            ServerCallContext context)
        {
            if (!Guid.TryParse(request.Id, out var customerId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Неверный формат Id"));
            }

            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Клиент не найден"));
            }

            var response = new CustomerGrpcResponse
            {
                Id = customer.Id.ToString(),
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email
            };

            if (customer.Preferences != null)
            {
                response.Preferences.AddRange(customer.Preferences.Select(cp =>
                    new PreferenceGrpcResponse
                    {
                        Id = cp.Preference.Id.ToString(),
                        Name = cp.Preference.Name
                    }));
            }

            if (customer.PromoCodes != null)
            {
                response.PromoCodes.AddRange(customer.PromoCodes.Select(pc =>
                    new PromoCodeGrpcResponse
                    {
                        Id = pc.PromoCode.Id.ToString(),
                        Code = pc.PromoCode.Code,
                        ServiceInfo = pc.PromoCode.ServiceInfo ?? string.Empty,
                        BeginDate = pc.PromoCode.BeginDate.ToString("yyyy-MM-dd"),
                        EndDate = pc.PromoCode.EndDate.ToString("yyyy-MM-dd"),
                        PartnerName = pc.PromoCode.PartnerId.ToString()
                    }));
            }

            return response;
        }
    }
}