﻿// -----------------------------------------------------------------------
// <copyright file="Startup.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Web.Quartz
{
	using CrystalQuartz.AspNetCore;
	using Microsoft.AspNetCore.Builder;
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.DependencyInjection;
	using Xeinaemm.AspNetCore;
	using Xeinaemm.Quartz;

	public class Startup
	{
		public Startup(IConfiguration configuration, IHostingEnvironment environment)
		{
			this.Configuration = configuration;
			this.Environment = environment;
		}

		public IConfiguration Configuration { get; }

		public IHostingEnvironment Environment { get; }

		public static void ConfigureServices(IServiceCollection services)
		{
			services.AddCustomCookiePolicy();
			services.AddMvc().SetCompatibilityVersion();
		}

		public async void Configure(IApplicationBuilder app, IQuartzService quartzService)
		{
			app.UseCustomHostingEnvironment(this.Environment);
			app.UseHttpsRedirection();
			app.UseCrystalQuartz(() => quartzService.ServerInstance);
			await new QuartzScheduler(quartzService).RegisterScheduledJobsAsync().ConfigureAwait(false);
			app.UseAuthentication();
			app.UseStaticFiles();
			app.UseCookiePolicy();

			app.UseMvcWithDefaultRoute();
		}
	}
}
