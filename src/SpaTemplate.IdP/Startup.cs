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
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using SpaTemplate.Application.Setup.ContainerTasks;
    using Xeinaemm.AspNetCore;
    using Xeinaemm.AspNetCore.Identity.IdentityServer;

    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(
            IConfiguration configuration) => this.configuration = configuration;

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddCustomCookiePolicy();
            services.AddRazorPages();
            services.AddControllersWithViews();
            services.AddCustomIISOptions();

            services.AddCustomIdentityServer<IdentityUser, CustomIdentityDbContext>(this.configuration.GetConnectionString(), Assembly.GetExecutingAssembly().GetName().Name)
                .AddDeveloperSigningCredential();

            return services.InitializeWeb(setupAction => setupAction.RegisterType<IdentityServerService>().As<IIdentityServerService>());
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.EnvironmentName == "Development")
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseIdentityServer();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapRazorPages();
            });
        }
    }
}
