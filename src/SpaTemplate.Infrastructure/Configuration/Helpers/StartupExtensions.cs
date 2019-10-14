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
    using Autofac.Extensions.DependencyInjection;
    using AutoMapper;
    using Microsoft.Extensions.DependencyInjection;
    using SpaTemplate.Core;
    using Xeinaemm.Configuration.Autofac;

    public static class StartupExtensions
    {
        public static ContainerConfiguration Configuration { get; } = new ContainerConfiguration
        {
            Assemblies = new[] { Assembly.GetExecutingAssembly(), Assembly.GetAssembly(typeof(EmptyClassSpaTemplateCore)), Assembly.GetAssembly(typeof(EmptyClassSpaTemplateInfrastructure)) },
        };

        public static IServiceCollection AddCustomAutoMapper(this IServiceCollection services) =>
            services.AddAutoMapper(Configuration.Assemblies);

        public static IServiceProvider InitializeWeb(this IServiceCollection services, Action<ContainerBuilder> builder = null) =>
            new AutofacServiceProvider(services.InitializeWeb(Configuration, extendedSetupAction => builder.Invoke(extendedSetupAction)));
    }
}