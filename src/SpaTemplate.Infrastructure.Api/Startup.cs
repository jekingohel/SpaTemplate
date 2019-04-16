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
			_ = app.UseIpRateLimiting();

			if (env.IsDevelopment())
				_ = app.UseDeveloperExceptionPage();

			// TODO: Find fix for tests(downgraded to 6.1.0)
			Mapper.Initialize(cfg =>
			{
				_ = cfg.CreateMap<Student, StudentDto>();
				_ = cfg.CreateMap<StudentForCreationDto, Student>();
				_ = cfg.CreateMap<StudentForUpdateDto, Student>();
				_ = cfg.CreateMap<Student, StudentForUpdateDto>();
				_ = cfg.CreateMap<Course, CourseDto>();
				_ = cfg.CreateMap<CourseDto, Course>();
				_ = cfg.CreateMap<CourseForCreationDto, Course>();
				_ = cfg.CreateMap<CourseForUpdateDto, Course>();
				_ = cfg.CreateMap<Course, CourseForUpdateDto>();
			});

			_ = app.UseSwagger();
			_ = app.UseSwaggerUI(c =>
			  {
				  c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
				  c.RoutePrefix = string.Empty;
			  });

			_ = app.UseHttpCacheHeaders();

			_ = app.UseMvcWithDefaultRoute();
		}

		public IServiceProvider ConfigureServices(IServiceCollection services)
		{
			services.SetupDbContext();
			services.AddCustomMvc();
			services.AddSwagger();
			services.AddCustomHttpCacheHeaders();
			_ = services.AddMemoryCache();
			services.ConfigureIpRateLimitOptions();
			services.SetupLogging(this.Configuration);
			return services.BuildDependencyInjectionProvider();
		}
	}
}