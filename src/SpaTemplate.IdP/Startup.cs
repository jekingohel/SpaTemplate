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
	using System.Text;
	using Autofac;
	using Microsoft.AspNetCore.Builder;
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.AspNetCore.Identity;
	using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.IdentityModel.Tokens;
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

			if (this.environment.IsProduction())
			{
				services.AddCustomIdentityServer<IdentityUser, CustomIdentityDbContext>(this.configuration.GetConnectionString(), Assembly.GetExecutingAssembly().GetName().Name)
					.AddSigningCredential(new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.configuration.GetSecurityString())), SecurityAlgorithms.HmacSha256Signature));
			}
			else
			{
				services.AddCustomInMemoryIdentityServer<IdentityUser, CustomIdentityDbContext>("idp")
					.AddDeveloperSigningCredential();
			}

			return services.AddCustomDependencyInjectionProvider(setupAction =>
			{
				setupAction.RegisterType<CustomIdentityDbContext>().As<IdentityDbContext<IdentityUser>>();
				setupAction.RegisterType<IdentityServerService>().As<IIdentityServerService>();
			});
		}

		public void Configure(IApplicationBuilder app)
		{
			app.UseCustomHostingEnvironment(this.environment);
			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseCookiePolicy();
			app.UseAuthentication();
			app.UseIdentityServer();
			app.UseMvcWithDefaultRoute();
		}
	}
}
