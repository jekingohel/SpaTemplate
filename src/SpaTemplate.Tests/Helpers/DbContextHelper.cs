// -----------------------------------------------------------------------
// <copyright file="DbContextHelper.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Tests.Helpers
{
    using System;
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using SpaTemplate.Core.FacultyContext;
    using SpaTemplate.Infrastructure;
    using Xeinaemm.Domain;
    using Xeinaemm.Hateoas;

    public static class DbContextHelper
    {
        public static List<Guid> DummyGuidsArray() => new List<Guid>
        {
            new Guid("cf23dadb-8997-41b2-8851-d429be769391"),
            new Guid("a2398e93-cceb-4027-8e3c-ba56b9bd6e10"),
            new Guid("015ed34c-f678-48ae-abbe-1be8a79d8f0f"),
            new Guid("e0195020-ab04-4691-b6b7-a6b35fe58874"),
            new Guid("b9b39223-dc15-4dd5-bc16-e7aec7a0cdf4"),
        };

        public static Repository GetRepository(IPropertyMappingService propertyMapping = null)
        {
            var dbContext = new AppDbContext(CreateNewContextOptions(), Mock.Of<IDomainEventDispatcher>());
            return new Repository(dbContext, propertyMapping);
        }

        public static Repository SeedRepo(IPropertyMappingService property)
        {
            var repo = GetRepository(property);
            var ids = DummyGuidsArray();
            repo.AddEntity(new Student { Id = ids[0], Name = "Dummy", Surname = "FizzBuzz" });
            repo.AddEntity(new Student { Id = ids[1], Name = "Dummy", Surname = "FizzBuzz" });
            repo.AddEntity(new Student { Id = ids[2], Name = "Test", Surname = "Buzz" });
            repo.AddEntity(new Student { Id = ids[3], Name = "Test", Surname = "FizzBuzz" });
            repo.AddEntity(new Student { Id = ids[4], Name = "Test", Surname = "FizzBuzz" });
            return repo;
        }

        private static DbContextOptions<AppDbContext> CreateNewContextOptions()
        {
#pragma warning disable IDISP001 // Dispose created.
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();
#pragma warning restore IDISP001 // Dispose created.
            var builder = new DbContextOptionsBuilder<AppDbContext>();
            builder.UseInMemoryDatabase("rest")
                .UseInternalServiceProvider(serviceProvider);
            return builder.Options;
        }
    }
}