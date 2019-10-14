// -----------------------------------------------------------------------
// <copyright file="Startup.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.IdP
{
    using System;
    using System.Reflection;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using SpaTemplate.Infrastructure;
    using Xeinaemm.AspNetCore;
    using Xeinaemm.AspNetCore.Identity.IdentityServer;

    public class Startup
    {
        private readonly IConfiguration configuration;
        private readonly IHostingEnvironment environment;

        public Startup(
            IConfiguration configuration,
            IHostingEnvironment environment)
        {
            this.environment = environment;
            this.configuration = configuration;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddCustomCookiePolicy();
            services.AddMvc().SetCompatibilityVersion();
            services.AddCustomIISOptions();

            services.AddCustomIdentityServer<IdentityUser, CustomIdentityDbContext>(this.configuration.GetConnectionString(), Assembly.GetExecutingAssembly().GetName().Name)
                .AddDeveloperSigningCredential();

            return services.InitializeWeb(setupAction => setupAction.RegisterType<IdentityServerService>().As<IIdentityServerService>());
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseCustomHostingEnvironment(this.environment);
            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseIdentityServer();
            app.UseMvcWithDefaultRoute();
        }
    }
}
