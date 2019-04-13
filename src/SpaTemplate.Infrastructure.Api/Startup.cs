// -----------------------------------------------------------------------
// <copyright file="Startup.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Infrastructure.Api
{
	using System;
	using AspNetCoreRateLimit;
	using AutoMapper;
	using Microsoft.AspNetCore.Builder;
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.DependencyInjection;
	using SpaTemplate.Core.FacultyContext;

	public class Startup
	{
		public Startup(IConfiguration configuration) => this.Configuration = configuration;

		public IConfiguration Configuration { get; }

		public static void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			app.UseIpRateLimiting();

			if (env.IsDevelopment())
				app.UseDeveloperExceptionPage();

			// TODO: Find fix for tests(downgraded to 6.1.0)
			Mapper.Initialize(cfg =>
			{
				cfg.CreateMap<Student, StudentDto>();
				cfg.CreateMap<StudentForCreationDto, Student>();
				cfg.CreateMap<StudentForUpdateDto, Student>();
				cfg.CreateMap<Student, StudentForUpdateDto>();
				cfg.CreateMap<Course, CourseDto>();
				cfg.CreateMap<CourseDto, Course>();
				cfg.CreateMap<CourseForCreationDto, Course>();
				cfg.CreateMap<CourseForUpdateDto, Course>();
				cfg.CreateMap<Course, CourseForUpdateDto>();
			});

			app.UseSwagger();
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
				c.RoutePrefix = string.Empty;
			});

			app.UseHttpCacheHeaders();

			app.UseMvcWithDefaultRoute();
		}

		public IServiceProvider ConfigureServices(IServiceCollection services)
		{
			services.SetupDbContext();
			services.AddCustomMvc();
			services.AddSwagger();
			services.AddCustomHttpCacheHeaders();
			services.AddMemoryCache();
			services.ConfigureIpRateLimitOptions();
			services.SetupLogging(this.Configuration);
			return services.BuildDependencyInjectionProvider();
		}
	}
}