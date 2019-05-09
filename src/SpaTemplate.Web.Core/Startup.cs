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
	using SpaTemplate.Infrastructure;
	using Xeinaemm.AspNetCore;

	public class Startup
	{
		public Startup(IConfiguration configuration, IHostingEnvironment environment)
		{
			this.Configuration = configuration;
			this.Environment = environment;
		}

		public IConfiguration Configuration { get; }

		public IHostingEnvironment Environment { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddCustomClientAuthentication(new SpaTemplateWebCoreParameters(this.Configuration.GetSecurityString(), this.Configuration.GetAuthorityString()));
			services.AddSpaStaticFiles(configuration => configuration.RootPath = "ClientApp/dist");
		}

		public void Configure(IApplicationBuilder app)
		{
			app.UseCustomHostingEnvironment(this.Environment);
			app.UseAuthentication();

			app.UseStaticFiles();
			app.UseSpaStaticFiles();

			app.UseMvcWithDefaultRoute();

			app.UseSpa(spa =>
			{
				spa.Options.SourcePath = "ClientApp";

				if (this.Environment.IsDevelopment()) spa.UseAngularCliServer("start");
			});
		}
	}
}