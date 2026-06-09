using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pcf.GivingToCustomer.Core.Abstractions.Gateways;
using Pcf.GivingToCustomer.DataAccess;
using Pcf.GivingToCustomer.DataAccess.Data;
using Pcf.GivingToCustomer.Integration;
using Pcf.GivingToCustomer.IntegrationTests.Data;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pcf.GivingToCustomer.IntegrationTests
{
    public class TestWebApplicationFactory<TStartup>
        : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "ConnectionStrings:RabbitMq", "amqp://guest:guest@localhost:5672" }
                });
            });

            builder.ConfigureServices(services =>
            {
                var descriptorsToRemove = services
                    .Where(d =>
                        d.ServiceType == typeof(DbContextOptions) ||
                        d.ServiceType == typeof(DbContextOptions<DataContext>) ||
                        d.ServiceType == typeof(DataContext))
                    .ToList();

                foreach (var descriptor in descriptorsToRemove)
                {
                    services.Remove(descriptor);
                }

                var dbInitializerDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IDbInitializer));

                if (dbInitializerDescriptor != null)
                {
                    services.Remove(dbInitializerDescriptor);
                }

                services.AddScoped<IDbInitializer, EfTestDbInitializer>();
                services.AddScoped<INotificationGateway, NotificationGateway>();

                services.AddDbContext<DataContext>(x =>
                {
                    x.UseSqlite("Filename=PromoCodeFactoryDb.sqlite");
                    x.UseSnakeCaseNamingConvention();
                    x.UseLazyLoadingProxies();
                });

                var massTransitDescriptors = services
                    .Where(d => d.ServiceType.FullName?.Contains("MassTransit") == true)
                    .ToList();

                foreach (var descriptor in massTransitDescriptors)
                {
                    services.Remove(descriptor);
                }

                services.AddMassTransit(x =>
                {
                    x.UsingInMemory((context, cfg) =>
                    {
                        cfg.UseDelayedMessageScheduler();
                    });
                });

                var sp = services.BuildServiceProvider();

                using var scope = sp.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var dbContext = scopedServices.GetRequiredService<DataContext>();
                var logger = scopedServices
                    .GetRequiredService<ILogger<TestWebApplicationFactory<TStartup>>>();

                try
                {
                    new EfTestDbInitializer(dbContext).InitializeDb();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Проблема во время заполнения тестовой базы. " +
                        "Ошибка: {Message}", ex.Message);
                }
            });
        }
    }
}