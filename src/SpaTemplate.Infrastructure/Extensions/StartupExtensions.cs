// -----------------------------------------------------------------------
// <copyright file="StartupExtensions.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Infrastructure
{
	using System;
	using System.Reflection;
	using AspNetCoreRateLimit;
	using Autofac;
	using Autofac.Extensions.DependencyInjection;
	using AutoMapper;
	using Microsoft.AspNetCore.Http;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.AspNetCore.Mvc.Infrastructure;
	using Microsoft.AspNetCore.Mvc.Routing;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.DependencyInjection;
	using SpaTemplate.Core.SharedKernel;
	using Xeinaemm.Hateoas;
	using Xeinaemm.Quartz;

	public static class StartupExtensions
	{
		public static (Assembly webAssembly, Assembly coreAssembly, Assembly infrastructureAssembly) Assemblies() =>
			(Assembly.GetExecutingAssembly(), Assembly.GetAssembly(typeof(Person)), Assembly.GetAssembly(typeof(Repository)));

		public static IServiceCollection AddCustomAutoMapper(this IServiceCollection services)
		{
			var (webAssembly, coreAssembly, infrastructureAssembly) = Assemblies();
			return services.AddAutoMapper(webAssembly, coreAssembly, infrastructureAssembly);
		}

		public static IServiceProvider AddCustomDependencyInjectionProvider(this IServiceCollection services, IConfiguration config)
		{
			var builder = new ContainerBuilder();

			builder.Populate(services);

			var (webAssembly, coreAssembly, infrastructureAssembly) = Assemblies();
			builder.RegisterAssemblyTypes(webAssembly, coreAssembly, infrastructureAssembly)
				.AsImplementedInterfaces();

			// Custom DI
			builder.Register(_ => new QuartzService(config.GetConnectionString())).As<IQuartzService>().SingleInstance();
			builder.RegisterType<MemoryCacheRateLimitCounterStore>().As<IRateLimitCounterStore>();
			builder.RegisterType<TypeHelperService>().As<ITypeHelperService>();
			builder.RegisterType<MemoryCacheIpPolicyStore>().As<IIpPolicyStore>();
			builder.RegisterType<RateLimitConfiguration>().As<IRateLimitConfiguration>();
			builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>();
			builder.RegisterType<ActionContextAccessor>().As<IActionContextAccessor>();
			builder.Register(x =>
			  {
				  var actionContext = x.Resolve<IActionContextAccessor>().ActionContext;
				  var factory = x.Resolve<IUrlHelperFactory>();
				  return factory.GetUrlHelper(actionContext);
			  }).As<IUrlHelper>();

#pragma warning disable IDISP005 // Return type should indicate that the value should be disposed.
			return new AutofacServiceProvider(builder.Build());
#pragma warning restore IDISP005 // Return type should indicate that the value should be disposed.
		}
	}
}