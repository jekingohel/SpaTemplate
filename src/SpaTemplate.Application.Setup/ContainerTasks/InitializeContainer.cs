﻿// -----------------------------------------------------------------------
// <copyright file="InitializeContainer.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Application.Setup.ContainerTasks
{
    using System;
    using System.Net.Http;
    using System.Reflection;
    using Autofac;
    using AutoMapper;
    using Microsoft.Extensions.DependencyInjection;
    using Refit;
    using SpaTemplate.Contracts.Api;
    using SpaTemplate.Core;
    using SpaTemplate.Infrastructure;
    using Xeinaemm.Configuration.Autofac;

    public static class InitializeContainer
    {
        public static ContainerConfiguration Container { get; } = new ContainerConfiguration
        {
            RegisterAssemblies = new[]
            {
                Assembly.GetExecutingAssembly(),
                Assembly.GetAssembly(typeof(EmptyClassSpaTemplateCore)),
                Assembly.GetAssembly(typeof(EmptyClassSpaTemplateInfrastructure)),
            },
        };

        public static IServiceCollection AddCustomAutoMapper(this IServiceCollection services) =>
            services.AddAutoMapper(Container.RegisterAssemblies);

        public static IServiceProvider InitializeWeb(this IServiceCollection services, Action<ContainerBuilder> extendedSetupAction = null) =>
            services.InitializeWeb(Container, setupAction =>
            {
                setupAction.CommonSetup();
                extendedSetupAction?.Invoke(setupAction);
            });

        public static IServiceProvider InitializeApi(this IServiceCollection services, Action<ContainerBuilder> extendedSetupAction = null) =>
            services.InitializeApi(Container, setupAction =>
            {
                setupAction.CommonSetup();
                extendedSetupAction?.Invoke(setupAction);
            });

        public static IServiceProvider InitializeReadApi(this IServiceCollection services, Action<ContainerBuilder> extendedSetupAction = null) =>
            services.InitializeReadApi(Container, setupAction =>
            {
                setupAction.CommonSetup();
                extendedSetupAction?.Invoke(setupAction);
            });

        public static IContainer InitializeTests(HttpClient client) =>
            AutofacConfiguration.InitializeTests(Container, setupAction =>
            {
                setupAction.Register(c => RestService.For<IApi>(client)).As<IApi>();
                setupAction.Register(c => RestService.For<ICoursesApi>(client)).As<ICoursesApi>();
                setupAction.Register(c => RestService.For<IPeopleApi>(client)).As<IPeopleApi>();
                setupAction.Register(c => RestService.For<IPeopleCollectionApi>(client)).As<IPeopleCollectionApi>();
                setupAction.CommonSetup();
            });

        private static void CommonSetup(this ContainerBuilder builder)
        {
        }
    }
}
