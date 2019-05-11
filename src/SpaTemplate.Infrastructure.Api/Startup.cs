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
	using Xeinaemm.AspNetCore.Identity;

	public class Startup
	{
		public Startup(
			IHostingEnvironment environment,
			IConfiguration configuration,
			IApiVersionDescriptionProvider apiVersionDescriptionProvider)
		{
			this.Environment = environment;
			this.Configuration = configuration;
			this.ApiVersionDescriptionProvider = apiVersionDescriptionProvider;
		}

		public IConfiguration Configuration { get; }

		public IHostingEnvironment Environment { get; }

		public IApiVersionDescriptionProvider ApiVersionDescriptionProvider { get; }

		public IServiceProvider ConfigureServices(IServiceCollection services)
		{
			if (this.Environment.IsProduction())
				services.AddCustomDbContext<ApplicationDbContext>(this.Configuration.GetConnectionString());
			else
				services.AddCustomInMemoryDbContext<ApplicationDbContext>("api");

			services.AddCustomApiMvc();
			services.AddCustomApiBehavior();
			services.AddCustomVersionedApiExplorer();
			services.AddCustomApiAuthentication(new ApiParameters(this.Configuration.GetSecurityString(), this.Configuration.GetAuthorityString()));
			services.AddCustomApiVersioning();
			services.AddCustomSwagger(nameof(Api), new OpenApiInfo());
			services.AddCustomHttpCacheHeaders();
			services.AddMemoryCache();
			services.AddCustomIpRateLimitOptions(new List<RateLimitRule>());
			services.AddCustomLogging(this.Configuration);
#pragma warning disable IDISP005 // Return type should indicate that the value should be disposed.
			services.AddCustomAutoMapper();
			return services.AddCustomDependencyInjectionProvider(this.Configuration);
#pragma warning restore IDISP005 // Return type should indicate that the value should be disposed.
		}

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