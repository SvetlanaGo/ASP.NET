using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pcf.GivingToCustomer.Core.Abstractions.Gateways;
using Pcf.GivingToCustomer.Core.Abstractions.Repositories;
using Pcf.GivingToCustomer.DataAccess;
using Pcf.GivingToCustomer.DataAccess.Data;
using Pcf.GivingToCustomer.DataAccess.Repositories;
using Pcf.GivingToCustomer.Integration;

namespace Pcf.GivingToCustomer.WebHost
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddMvcOptions(x =>
                x.SuppressAsyncSuffixInActionNames = false);

            var mongoConnectionString = Configuration.GetConnectionString("PromocodeFactoryGivingToCustomerDb");
            var databaseName = "promocode_factory_giving_to_customer_db";

            var mongoUrl = new MongoDB.Driver.MongoUrl(mongoConnectionString);
            if (!string.IsNullOrEmpty(mongoUrl.DatabaseName))
            {
                databaseName = mongoUrl.DatabaseName;
            }

            services.AddSingleton(new MongoDbContext(mongoConnectionString, databaseName));

            services.AddScoped(typeof(IRepository<>), typeof(MongoRepository<>));

            services.AddScoped<INotificationGateway, NotificationGateway>();

            services.AddScoped<IDbInitializer, MongoDbInitializer>();

            services.AddOpenApiDocument(options =>
            {
                options.Title = "PromoCode Factory Giving To Customer API Doc";
                options.Version = "1.0";
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseOpenApi();
            app.UseSwaggerUi(x =>
            {
                x.DocExpansion = "list";
            });

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            using var scope = app.ApplicationServices.CreateScope();
            var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
            initializer.InitializeDb();
        }
    }
}