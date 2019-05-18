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
	using Autofac;
	using AutoMapper;
	using Microsoft.Extensions.DependencyInjection;
	using SpaTemplate.Core;
	using Xeinaemm.Common;

	public static class StartupExtensions
	{
		public static IServiceCollection AddCustomAutoMapper(this IServiceCollection services) =>
			services.AddAutoMapper(Assembly.GetExecutingAssembly(), Assembly.GetAssembly(typeof(EmptyClassSpaTemplateCore)), Assembly.GetAssembly(typeof(EmptyClassSpaTemplateInfrastructure)));

		public static IServiceProvider AddCustomDependencyInjectionProvider(this IServiceCollection services, Action<ContainerBuilder> builder = null) =>
			services.AddCustomAutofacServiceProvider(setupAction => builder?.Invoke(setupAction), Assembly.GetExecutingAssembly(), Assembly.GetAssembly(typeof(EmptyClassSpaTemplateCore)), Assembly.GetAssembly(typeof(EmptyClassSpaTemplateInfrastructure)));
	}
}