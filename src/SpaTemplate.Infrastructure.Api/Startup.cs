// -----------------------------------------------------------------------
// <copyright file="Startup.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Infrastructure.Api
{
    using System;
    using System.Collections.Generic;
    using AspNetCoreRateLimit;
    using Autofac;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Xeinaemm.AspNetCore;
    using Xeinaemm.AspNetCore.Api;
    using Xeinaemm.AspNetCore.Data;
    using Xeinaemm.AspNetCore.Identity.IdentityServer;
    using Xeinaemm.AspNetCore.Swagger;
    using Xeinaemm.Hateoas;
    using Xeinaemm.Quartz;

    public class Startup
    {
        public Startup(
            IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public IServiceProvider ConfigureServices(IServiceCollection services, IWebHostEnvironment env)
        {
            if (env.EnvironmentName == "Production")
                services.AddCustomDbContext<ApplicationDbContext>(this.Configuration.GetConnectionString());
            else
                services.AddCustomInMemoryDbContext<ApplicationDbContext>("api");

            services.AddCustomApiMvc();
            services.AddCustomApiBehavior();
            services.AddCustomVersionedApiExplorer();
            services.AddCustomApiAuthentication(new ApiParameters(this.Configuration.GetSecurityString(), this.Configuration.GetAuthorityString()));
            services.AddCustomApiVersioning();
            services.AddSwaggerGen(setupAction =>
            {
                setupAction.CustomSwaggerDoc(services, nameof(Api), OpenApiInfoCommon.Create(nameof(Api), " "));
                setupAction.CustomSecurityDefinition();
                setupAction.CustomSecurityRequirement();
                setupAction.CustomDocInclusionPredicate(nameof(Api));
                setupAction.CustomXmlComments<EmptyClassSpaTemplateInfrastructureApi>();
            });
            //services.AddOpenApiDocument();
            services.AddCustomHttpCacheHeaders();
            services.AddMemoryCache();
            services.AddCustomIpRateLimitOptions(new List<RateLimitRule>());
            services.AddCustomAutoMapper();

            return services.InitializeWeb(builder =>
            {
                builder.Register(_ => new QuartzService(this.Configuration.GetConnectionString())).As<IQuartzService>().SingleInstance();
                builder.RegisterType<MemoryCacheRateLimitCounterStore>().As<IRateLimitCounterStore>();
                builder.RegisterType<TypeHelperService>().As<ITypeHelperService>();
                builder.RegisterType<MemoryCacheIpPolicyStore>().As<IIpPolicyStore>();
                builder.RegisterType<RateLimitConfiguration>().As<IRateLimitConfiguration>();
                builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>();
                builder.RegisterType<ActionContextAccessor>().As<IActionContextAccessor>();
                builder.Register(x => x.Resolve<IUrlHelperFactory>().GetUrlHelper(x.Resolve<IActionContextAccessor>().ActionContext)).As<IUrlHelper>();
            });
        }

        public void Configure(IApplicationBuilder app, IApiVersionDescriptionProvider apiVersionDescriptionProvider, IWebHostEnvironment env)
        {
            app.UseIpRateLimiting();
            if (env.EnvironmentName == "Development")
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseHttpsRedirection();
            SwaggerBuilderExtensions.UseSwagger(app);
            app.UseCustomSwaggerUI(apiVersionDescriptionProvider, nameof(Api));
            app.UseAuthentication();
            app.UseStaticFiles();
            app.UseHttpCacheHeaders();
            app.UseCookiePolicy();
            app.UseMvcWithDefaultRoute();
        }
    }
}