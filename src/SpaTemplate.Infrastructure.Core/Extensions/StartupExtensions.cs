using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspNetCoreRateLimit;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using SpaTemplate.Core.Hateoas;
using SpaTemplate.Core.SharedKernel;
using Swashbuckle.AspNetCore.Swagger;
using MediaType = SpaTemplate.Core.Hateoas.MediaType;

namespace SpaTemplate.Infrastructure.Core
{
    public static class StartupExtensions
    {
        public static IServiceProvider BuildDependencyInjectionProvider(this IServiceCollection services)
        {
            var builder = new ContainerBuilder();

            builder.Populate(services);

            var webAssembly = Assembly.GetExecutingAssembly();
            var coreAssembly = Assembly.GetAssembly(typeof(BaseEntity));
            var infrastructureAssembly =
                Assembly.GetAssembly(typeof(Repository));
            builder.RegisterAssemblyTypes(webAssembly, coreAssembly, infrastructureAssembly).AsImplementedInterfaces();

            //Custom DI
            builder.RegisterType<MemoryCacheRateLimitCounterStore>().As<IRateLimitCounterStore>();
            builder.RegisterType<MemoryCacheIpPolicyStore>().As<IIpPolicyStore>();
            builder.RegisterType<RateLimitConfiguration>().As<IRateLimitConfiguration>();
            builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>();
            builder.RegisterType<ActionContextAccessor>().As<IActionContextAccessor>();
            builder.Register(x =>
            {
                var actionContext = x.Resolve<IActionContextAccessor>().ActionContext;
                var factory = x.Resolve<IUrlHelperFactory>();
                return factory.GetUrlHelper(actionContext);
            }).As<IUrlHelper>();

            var applicationContainer = builder.Build();
            return new AutofacServiceProvider(applicationContainer);
        }

        public static void ConfigureIpRateLimitOptions(this IServiceCollection services)
        {
            services.Configure<IpRateLimitOptions>(options =>
            {
                options.IpWhitelist = new List<string>
                {
                    Constants.LocalhostIp
                };
                options.GeneralRules = new List<RateLimitRule>
                {
                    new RateLimitRule
                    {
                        Endpoint = "/api",
                        Limit = 50,
                        Period = "5m"
                    },
                    new RateLimitRule
                    {
                        Endpoint = "*",
                        Limit = 100,
                        Period = "60s"
                    }
                };
            });
        }

        public static void AddCustomHttpCacheHeaders(this IServiceCollection services)
        {
            services.AddHttpCacheHeaders(
                expirationModelOptions
                    =>
                {
                    expirationModelOptions.MaxAge = 600;
                },
                validationModelOptions
                    =>
                {
                    validationModelOptions.MustRevalidate = true;
                });
        }

        public static void AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new Info {Title = "My API", Version = "v1"}); });
        }

        public static void AddCustomMvc(this IServiceCollection services)
        {
            services.AddMvc(setupAction =>
                {
                    setupAction.InputFormatters
                        .OfType<JsonInputFormatter>()
                        .FirstOrDefault()?
                        .SupportedMediaTypes
                        .Add(MediaType.InputFormatterJson);

                    setupAction.OutputFormatters
                        .OfType<JsonOutputFormatter>()
                        .FirstOrDefault()?
                        .SupportedMediaTypes
                        .Add(MediaType.OutputFormatterJson);
                })
                .AddControllersAsServices()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver =
                        new CamelCasePropertyNamesContractResolver();
                });
        }

        public static void SetupDbContext(this IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("rest"));
        }

        public static void SetupLogging(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConfiguration(configuration.GetSection("Logging"));
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
            });
        }
    }
}