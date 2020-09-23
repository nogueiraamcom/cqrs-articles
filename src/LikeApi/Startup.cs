using Core;
using IoC;
using IoC.Extensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO.Compression;

namespace LikeApi
{
    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddResponseCompression();
            services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Fastest;
            });

            var assemblyApplication = AppDomain.CurrentDomain.Load("Core");
            services.AddMediatR(assemblyApplication);

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = true;
                options.Filters.Add(new GlobalExceptionFilter());
            })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            services.RegisterDependencies();

            var messagingConfiguration = configuration.GetSection(nameof(Messaging)).Get<Messaging>();

            services.Configure<Messaging>(c =>
            {
                c.Exchanges = messagingConfiguration.Exchanges;
                c.Queues = messagingConfiguration.Queues;
            });

            var rabbitMQConfiguration = configuration.GetSection(nameof(RabbitMQConfiguration)).Get<RabbitMQConfiguration>();
            _ = services.Configure<RabbitMQConfiguration>(c =>
              {
                  c.Hostname = rabbitMQConfiguration.Hostname;
                  c.Username = rabbitMQConfiguration.Username;
                  c.Password = rabbitMQConfiguration.Password;
              });

            services.AddRabbit(messagingConfiguration, rabbitMQConfiguration);
        }
    }
}