using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Pcf.GivingToCustomer.Core.Abstractions.Gateways;
using Pcf.GivingToCustomer.Core.Abstractions.Repositories;
using Pcf.GivingToCustomer.Core.Domain;
using Pcf.GivingToCustomer.DataAccess;
using Pcf.GivingToCustomer.DataAccess.Data;
using Pcf.GivingToCustomer.DataAccess.Repositories;
using Pcf.GivingToCustomer.Integration;
using Pcf.GivingToCustomer.IntegrationTests.Data;

namespace Pcf.GivingToCustomer.IntegrationTests
{
    public class TestWebApplicationFactory<TStartup>
        : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.Sources.Clear();
                config.AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "ConnectionStrings:PromocodeFactoryGivingToCustomerDb",
                      "mongodb://localhost:27018/promocode_factory_api_test_db" },
                    { "Logging:LogLevel:Default", "Information" }
                });
            });

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(MongoDbContext));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                var mongoConnectionString = "mongodb://localhost:27018/promocode_factory_api_test_db";
                var mongoContext = new MongoDbContext(mongoConnectionString, "promocode_factory_api_test_db");
                services.AddSingleton(mongoContext);

                services.AddScoped(typeof(IRepository<>), typeof(MongoRepository<>));

                services.AddScoped<INotificationGateway, NotificationGateway>();

                var initializerDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IDbInitializer));
                if (initializerDescriptor != null)
                {
                    services.Remove(initializerDescriptor);
                }

                services.AddScoped<IDbInitializer, MongoTestDbInitializer>();
            });
        }
    }
}