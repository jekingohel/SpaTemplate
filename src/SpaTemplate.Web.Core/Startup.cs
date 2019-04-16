// -----------------------------------------------------------------------
// <copyright file="Startup.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Web.Core
{
	using Microsoft.AspNetCore.Builder;
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.AspNetCore.SpaServices.AngularCli;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.DependencyInjection;

	public class Startup
	{
		public Startup(IConfiguration configuration) => this.Configuration = configuration;

		public IConfiguration Configuration { get; }

		public static void ConfigureServices(IServiceCollection services) => services.AddSpaStaticFiles(configuration => configuration.RootPath = "ClientApp/dist");

		public static void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				_ = app.UseDeveloperExceptionPage();
			}
			else
			{
				_ = app.UseExceptionHandler("/Error");
				_ = app.UseHsts();
			}

			_ = app.UseStaticFiles();
			app.UseSpaStaticFiles();

			_ = app.UseMvcWithDefaultRoute();

			app.UseSpa(spa =>
			{
				spa.Options.SourcePath = "ClientApp";

				if (env.IsDevelopment()) spa.UseAngularCliServer("start");
			});
		}
	}
}