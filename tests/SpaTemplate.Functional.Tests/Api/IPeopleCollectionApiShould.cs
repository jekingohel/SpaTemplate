// -----------------------------------------------------------------------
// <copyright file="IPeopleCollectionApiShould.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Functional.Tests.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Autofac;
    using FluentAssertions;
    using SpaTemplate.Application.Setup.ContainerTasks;
    using SpaTemplate.Contracts.Api;
    using SpaTemplate.Contracts.Models;
    using SpaTemplate.Functional.Tests.Helpers;
    using SpaTemplate.Infrastructure.Api;
    using SpaTemplate.Tests.Helpers;
    using Xeinaemm.Tests;
    using Xunit;

    public class IPeopleCollectionApiShould : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly IPeopleCollectionApi api;

        public IPeopleCollectionApiShould(CustomWebApplicationFactory<Startup> factory)
        {
            using var scope = InitializeContainer.InitializeTests(factory.CreateClientWithDefaultRequestHeaders()).BeginLifetimeScope();
            this.api = scope.Resolve<IPeopleCollectionApi>();
        }

        [Theory]
        [AutoMoqData]
        public async Task CreateCollectionAfterValidPost(IEnumerable<StudentForCreationDto> dtos)
        {
            var post = await this.api.CreateStudentCollection(dtos);
            Assert.Equal(3, post.Count());
        }

        [Fact]
        public void ReturnsBadRequestNoIds() =>
            RefitExceptions.Verify(async () => await this.api.GetStudentCollection(null), HttpStatusCode.BadRequest);

        [Theory]
        [AutoMoqData]
        public async Task ReturnsCollectionAfterValidGet(IEnumerable<StudentForCreationDto> dtos)
        {
            var post = await this.api.CreateStudentCollection(dtos);
            var get = await this.api.GetStudentCollection(post.Select(x => x.Id).Take(3));
            post.Should().AllBeEquivalentTo(get);
        }

        [Fact]
        public void ReturnsNotFoundIdsNotExist()
        {
            var ids = new List<Guid>
            {
                Guid.NewGuid(),
                Guid.NewGuid(),
            };
            RefitExceptions.Verify(async () => await this.api.GetStudentCollection(ids), HttpStatusCode.NotFound);
        }
    }
}
