using MongoDB.Driver;
using Pcf.GivingToCustomer.Core.Domain;
using System.Linq;

namespace Pcf.GivingToCustomer.DataAccess.Data
{
    public class MongoDbInitializer : IDbInitializer
    {
        private readonly MongoDbContext _context;

        public MongoDbInitializer(MongoDbContext context)
        {
            _context = context;
        }

        public void InitializeDb()
        {
            _context.Preferences.DeleteMany(Builders<Preference>.Filter.Empty);
            _context.Customers.DeleteMany(Builders<Customer>.Filter.Empty);
            _context.PromoCodes.DeleteMany(Builders<PromoCode>.Filter.Empty);

            _context.Preferences.Indexes.CreateOne(new CreateIndexModel<Preference>(
                Builders<Preference>.IndexKeys.Ascending(x => x.Name)));

            _context.Customers.Indexes.CreateOne(new CreateIndexModel<Customer>(
                Builders<Customer>.IndexKeys.Ascending(x => x.Email)));

            var preferencesList = FakeDataFactory.Preferences.ToList<Preference>();
            _context.Preferences.InsertMany(preferencesList);

            var customersList = FakeDataFactory.Customers.ToList<Customer>();

            foreach (var customer in customersList)
            {
                if (customer.Preferences != null)
                {
                    foreach (var cp in customer.Preferences)
                    {
                        cp.Preference = preferencesList.FirstOrDefault(p => p.Id == cp.PreferenceId);
                        cp.Customer = customer;
                    }
                }
            }

            _context.Customers.InsertMany(customersList);
        }
    }
}