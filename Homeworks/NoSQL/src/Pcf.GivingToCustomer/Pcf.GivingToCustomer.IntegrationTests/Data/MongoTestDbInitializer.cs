using MongoDB.Driver;
using Pcf.GivingToCustomer.Core.Domain;
using Pcf.GivingToCustomer.DataAccess;
using Pcf.GivingToCustomer.DataAccess.Data;
using System.Linq;

namespace Pcf.GivingToCustomer.IntegrationTests.Data
{
    public class MongoTestDbInitializer : IDbInitializer
    {
        private readonly MongoDbContext _context;

        public MongoTestDbInitializer(MongoDbContext context)
        {
            _context = context;
        }

        public void InitializeDb()
        {
            _context.Preferences.Database.DropCollection("Preferences");
            _context.Customers.Database.DropCollection("Customers");
            _context.PromoCodes.Database.DropCollection("PromoCodes");

            _context.Preferences.Indexes.CreateOne(new CreateIndexModel<Preference>(
                Builders<Preference>.IndexKeys.Ascending(x => x.Name)));

            _context.Customers.Indexes.CreateOne(new CreateIndexModel<Customer>(
                Builders<Customer>.IndexKeys.Ascending(x => x.Email)));

            var preferencesList = TestDataFactory.Preferences.ToList<Preference>();
            _context.Preferences.InsertMany(preferencesList);

            var customersList = TestDataFactory.Customers.ToList<Customer>();

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

        public void CleanDb()
        {
            _context.Preferences.DeleteMany(Builders<Preference>.Filter.Empty);
            _context.Customers.DeleteMany(Builders<Customer>.Filter.Empty);
            _context.PromoCodes.DeleteMany(Builders<PromoCode>.Filter.Empty);
        }
    }
}