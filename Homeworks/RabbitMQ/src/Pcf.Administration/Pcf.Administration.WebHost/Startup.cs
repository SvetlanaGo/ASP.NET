using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pcf.Administration.Core.Abstractions.Repositories;
using Pcf.Administration.Core.Services;
using Pcf.Administration.DataAccess;
using Pcf.Administration.DataAccess.Data;
using Pcf.Administration.DataAccess.Repositories;
using Pcf.Administration.WebHost.Consumers;
using System;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace Pcf.Administration.WebHost
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddMvcOptions(x =>
                x.SuppressAsyncSuffixInActionNames = false);
            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
            services.AddScoped<IDbInitializer, EfDbInitializer>();
            services.AddDbContext<DataContext>(x =>
            {
                //x.UseSqlite("Filename=PromocodeFactoryAdministrationDb.sqlite");
                x.UseNpgsql(Configuration.GetConnectionString("PromocodeFactoryAdministrationDb"));
                x.UseSnakeCaseNamingConvention();
                x.UseLazyLoadingProxies();
            });

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            services.AddOpenApiDocument(options =>
            {
                options.Title = "PromoCode Factory Administration API Doc";
                options.Version = "1.0";
            });

            services.AddScoped<IEmployeeService, EmployeeService>();

            services.AddMassTransit(x =>
            {
                x.AddConsumer<AdminPromoCodeIssuedConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(Configuration.GetConnectionString("RabbitMq"));

                    cfg.ReceiveEndpoint("administration-queue", e =>
                    {
                        e.UseMessageRetry(r =>
                        {
                            r.Interval(3, TimeSpan.FromSeconds(5));

                            r.Ignore<EmployeeNotFoundException>();
                            r.Ignore<ArgumentException>();
                            r.Ignore<InvalidOperationException>();
                        });

                        e.ConfigureConsumer<AdminPromoCodeIssuedConsumer>(context);
                    });
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IDbInitializer dbInitializer)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseOpenApi();
            app.UseSwaggerUi(x =>
            {
                x.DocExpansion = "list";
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            dbInitializer.InitializeDb();
        }
    }
}