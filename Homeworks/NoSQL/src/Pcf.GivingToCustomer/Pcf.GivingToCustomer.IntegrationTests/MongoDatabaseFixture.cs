using MongoDB.Driver;
using Pcf.GivingToCustomer.Core.Domain;
using Pcf.GivingToCustomer.DataAccess;
using Pcf.GivingToCustomer.IntegrationTests.Data;
using System;
using Xunit;

namespace Pcf.GivingToCustomer.IntegrationTests
{
    public class MongoDatabaseFixture : IDisposable
    {
        public MongoDbContext DbContext { get; private set; }
        public MongoTestDbInitializer DbInitializer { get; private set; }

        public MongoDatabaseFixture()
        {
            var connectionString = "mongodb://localhost:27018/promocode_factory_test_db";
            DbContext = new MongoDbContext(connectionString, "promocode_factory_test_db");
            DbInitializer = new MongoTestDbInitializer(DbContext);

            DbInitializer.InitializeDb();
        }

        public void Dispose()
        {
            DbContext.Customers.DeleteMany(Builders<Customer>.Filter.Empty);
            DbContext.Preferences.DeleteMany(Builders<Preference>.Filter.Empty);
            DbContext.PromoCodes.DeleteMany(Builders<PromoCode>.Filter.Empty);
        }
    }

    [CollectionDefinition(DbCollection)]
    public class MongoDatabaseCollection : ICollectionFixture<MongoDatabaseFixture>
    {
        public const string DbCollection = "MongoDb collection";
    }
}