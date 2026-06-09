using Pcf.GivingToCustomer.Core.Abstractions.Repositories;
using Pcf.GivingToCustomer.Core.Domain;
using Pcf.GivingToCustomer.Core.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pcf.GivingToCustomer.Core.Services
{
    public class PromoCodeService : IPromoCodeService
    {
        private readonly IRepository<PromoCode> _promoCodesRepository;
        private readonly IRepository<Preference> _preferencesRepository;
        private readonly IRepository<Customer> _customersRepository;

        public PromoCodeService(
            IRepository<PromoCode> promoCodesRepository,
            IRepository<Preference> preferencesRepository,
            IRepository<Customer> customersRepository)
        {
            _promoCodesRepository = promoCodesRepository;
            _preferencesRepository = preferencesRepository;
            _customersRepository = customersRepository;
        }

        public async Task GivePromoCodesToCustomersWithPreferenceAsync(GivePromoCodeMessage message)
        {
            var preference = await _preferencesRepository.GetByIdAsync(message.PreferenceId);

            if (preference == null)
            {
                throw new PreferenceNotFoundException(message.PreferenceId);
            }

            var customers = await _customersRepository
                .GetWhere(d => d.Preferences.Any(x => x.Preference.Id == preference.Id));

            var promoCode = new PromoCode
            {
                Id = message.PromoCodeId,
                PartnerId = message.PartnerId,
                Code = message.PromoCode,
                ServiceInfo = message.ServiceInfo,
                BeginDate = DateTime.Parse(message.BeginDate),
                EndDate = DateTime.Parse(message.EndDate),
                Preference = preference,
                PreferenceId = preference.Id,
                Customers = new List<PromoCodeCustomer>()
            };

            foreach (var customer in customers)
            {
                promoCode.Customers.Add(new PromoCodeCustomer
                {
                    CustomerId = customer.Id,
                    Customer = customer,
                    PromoCodeId = promoCode.Id,
                    PromoCode = promoCode
                });
            }

            await _promoCodesRepository.AddAsync(promoCode);
        }
    }

    public class PreferenceNotFoundException : Exception
    {
        public Guid PreferenceId { get; }

        public PreferenceNotFoundException(Guid preferenceId)
            : base($"Preference with id {preferenceId} not found")
        {
            PreferenceId = preferenceId;
        }
    }
}