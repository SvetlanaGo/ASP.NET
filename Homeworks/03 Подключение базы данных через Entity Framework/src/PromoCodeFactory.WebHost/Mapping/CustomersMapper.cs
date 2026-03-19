using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Models.Customers;

namespace PromoCodeFactory.WebHost.Mapping;

public static class CustomersMapper
{
    public static CustomerShortResponse ToCustomerShortResponse(Customer customer) =>
        new CustomerShortResponse(
            customer.Id,
            customer.FirstName,
            customer.LastName,
            customer.Email,
            customer.Preferences.Select(PreferencesMapper.ToPreferenceShortResponse).ToArray());

    public static CustomerResponse ToCustomerResponse(
    Customer customer,
    IEnumerable<PromoCode> promoCodes)
    {
        var promoCodeDictionary = promoCodes.ToDictionary(p => p.Id);

        return new CustomerResponse(
            customer.Id,
            customer.FirstName,
            customer.LastName,
            customer.Email,
            customer.Preferences
                .Select(PreferencesMapper.ToPreferenceShortResponse)
                .ToArray(),
            customer.CustomerPromoCodes
                .Select(c => PromoCodesMapper.ToCustomerPromoCodeResponse(c, promoCodeDictionary[c.PromoCodeId]))
                .ToArray());
    }

    public static Customer ToCustomer(CustomerCreateRequest request, IEnumerable<Preference> preferences) =>
        new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Preferences = preferences.ToArray()
        };

    public static Customer ToCustomer(CustomerUpdateRequest request, IEnumerable<Preference> preferences, Customer customer)
    {
        customer.FirstName = request.FirstName;
        customer.LastName = request.LastName;
        customer.Email = request.Email;
        customer.Preferences = preferences.ToArray();

        return customer;
    }
}
