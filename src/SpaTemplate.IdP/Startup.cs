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
	using Autofac.Extensions.DependencyInjection;
	using Microsoft.AspNetCore.Builder;
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.AspNetCore.Identity;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.IdentityModel.Tokens;
	using SpaTemplate.Infrastructure;
	using Xeinaemm.AspNetCore;
	using Xeinaemm.AspNetCore.Data;
	using Xeinaemm.AspNetCore.Identity.Extensions;
	using Xeinaemm.AspNetCore.Identity.IdentityServer;

	public class Startup
	{
		public Startup(IConfiguration configuration, IHostingEnvironment environment, IServiceProvider serviceProvider)
		{
			this.Configuration = configuration;
			this.Environment = environment;
			this.ServiceProvider = serviceProvider;
		}

		public IConfiguration Configuration { get; }

		public IHostingEnvironment Environment { get; }

		public IServiceProvider ServiceProvider { get; }

		public IServiceProvider ConfigureServices(IServiceCollection services)
		{
			services.AddCustomCookiePolicy();
			services.AddCustomIdentity<IdentityUser, IdentityDbContext>();

			if (this.Environment.IsProduction())
			{
				var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.Configuration.GetSecurityString())), SecurityAlgorithms.HmacSha256Signature);
				var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
				var connectionString = this.Configuration.GetConnectionString();
				services.AddCustomDbContext<IdentityDbContext>(connectionString);
				services.AddCustomIdentityServer<IdentityUser>(signingCredentials, connectionString, migrationsAssembly);
				this.ServiceProvider.EnsureIdentitySeedDataAsync<IdentityDbContext>(new IdentitySeedData(this.Configuration)).ConfigureAwait(false);
			}
			else
			{
				services.AddCustomInMemoryDbContext<IdentityDbContext>("idp");
				services.AddCustomInMemoryIdentityServer<IdentityUser>(new IdentitySeedData(this.Configuration));
			}

			services.AddCustomIISOptions();
			services.AddMvc().SetCompatibilityVersion();

			var builder = new ContainerBuilder();
			builder.Populate(services);
			builder.RegisterType<IdentityServerService>().As<IIdentityServerService>();
#pragma warning disable IDISP005 // Return type should indicate that the value should be disposed.
			return new AutofacServiceProvider(builder.Build());
#pragma warning restore IDISP005 // Return type should indicate that the value should be disposed.
		}

		public void Configure(IApplicationBuilder app)
		{
			app.UseCustomHostingEnvironment(this.Environment);
			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseCookiePolicy();
			app.UseAuthentication();
			app.UseIdentityServer();
			app.UseMvcWithDefaultRoute();
		}
	}
}
