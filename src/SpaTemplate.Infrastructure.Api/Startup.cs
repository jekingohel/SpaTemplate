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
	using Xeinaemm.AspNetCore;
	using Xeinaemm.AspNetCore.Api;
	using Xeinaemm.AspNetCore.Data;
	using Xeinaemm.AspNetCore.Identity;
	using Xeinaemm.AspNetCore.Swagger;

	public class Startup
	{
		public Startup(
			IHostingEnvironment environment,
			IConfiguration configuration)
		{
			this.Environment = environment;
			this.Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public IHostingEnvironment Environment { get; }

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
			services.AddSwaggerGen(setupAction =>
			{
				setupAction.CustomSwaggerDoc(services, nameof(Api), OpenApiInfoCommon.Create("Api", " "));
				setupAction.CustomSecurityDefinition();
				setupAction.CustomSecurityRequirement();
				setupAction.CustomDocInclusionPredicate(nameof(Api));
				setupAction.CustomXmlComments<EmptyClassSpaTemplateInfrastructureApi>();
			});
			services.AddCustomHttpCacheHeaders();
			services.AddMemoryCache();
			services.AddCustomIpRateLimitOptions(new List<RateLimitRule>());
			services.AddCustomLogging(this.Configuration);
#pragma warning disable IDISP005 // Return type should indicate that the value should be disposed.
			services.AddCustomAutoMapper();
			return services.AddCustomDependencyInjectionProvider(this.Configuration);
#pragma warning restore IDISP005 // Return type should indicate that the value should be disposed.
		}

		public void Configure(IApplicationBuilder app, IApiVersionDescriptionProvider apiVersionDescriptionProvider)
		{
			app.UseIpRateLimiting();
			app.UseCustomHostingEnvironment(this.Environment);
			app.UseHttpsRedirection();
			app.UseSwagger();
			app.UseCustomSwaggerUI(apiVersionDescriptionProvider, nameof(Api));
			app.UseAuthentication();
			app.UseStaticFiles();
			app.UseHttpCacheHeaders();
			app.UseCookiePolicy();
			app.UseMvcWithDefaultRoute();
		}
	}
}