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
	using Microsoft.AspNetCore.Builder;
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.AspNetCore.Mvc.ApiExplorer;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.OpenApi.Models;
	using Xeinaemm.AspNetCore;

	/// <summary>
	///
	/// </summary>
	public class Startup
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Startup"/> class.
		/// </summary>
		/// <param name="environment"></param>
		/// <param name="configuration"></param>
		/// <param name="apiVersionDescriptionProvider"></param>
		public Startup(
			IHostingEnvironment environment,
			IConfiguration configuration,
			IApiVersionDescriptionProvider apiVersionDescriptionProvider)
		{
			this.Environment = environment;
			this.Configuration = configuration;
			this.ApiVersionDescriptionProvider = apiVersionDescriptionProvider;
		}

		/// <summary>
		///
		/// </summary>
		public IConfiguration Configuration { get; }

		/// <summary>
		///
		/// </summary>
		public IHostingEnvironment Environment { get; }

		/// <summary>
		///
		/// </summary>
		public IApiVersionDescriptionProvider ApiVersionDescriptionProvider { get; }

		/// <summary>
		///
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public IServiceProvider ConfigureServices(IServiceCollection services)
		{
			if (this.Environment.IsProduction())
			{
				var connectionString = this.Configuration.GetConnectionString("DefaultConnection");
				services.AddCustomDbContext<ApplicationDbContext>(connectionString);
			}
			else
			{
				services.AddCustomInMemoryDbContext<ApplicationDbContext>("api");
			}

			services.AddCustomApiMvc();
			services.AddCustomApiBehavior();
			services.AddCustomVersionedApiExplorer();
			services.AddCustomApiAuthentication();
			services.AddCustomApiVersioning();
			services.AddCustomSwagger(nameof(Api), new OpenApiInfo());
			services.AddCustomHttpCacheHeaders();
			services.AddMemoryCache();
			services.AddCustomIpRateLimitOptions(new List<RateLimitRule>());
			services.AddCustomLogging(this.Configuration);
#pragma warning disable IDISP005 // Return type should indicate that the value should be disposed.
			services.AddCustomAutoMapper();
			return services.AddCustomDependencyInjectionProvider();
#pragma warning restore IDISP005 // Return type should indicate that the value should be disposed.
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="app"></param>
		public void Configure(IApplicationBuilder app)
		{
			app.UseIpRateLimiting();
			app.UseCustomHostingEnvironment(this.Environment);
			app.UseHttpsRedirection();
			app.UseSwagger();
			app.UseCustomSwaggerUI(this.ApiVersionDescriptionProvider, nameof(Api));
			app.UseAuthentication();
			app.UseStaticFiles();
			app.UseHttpCacheHeaders();
			app.UseCookiePolicy();
			app.UseMvcWithDefaultRoute();
		}
	}
}