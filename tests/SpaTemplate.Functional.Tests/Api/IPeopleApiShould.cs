// -----------------------------------------------------------------------
// <copyright file="IPeopleApiShould.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Functional.Tests.Api
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Autofac;
    using Microsoft.AspNetCore.JsonPatch;
    using SpaTemplate.Application.Setup.ContainerTasks;
    using SpaTemplate.Contracts.Api;
    using SpaTemplate.Contracts.Models;
    using SpaTemplate.Contracts.Parameters;
    using SpaTemplate.Core.SharedKernel;
    using SpaTemplate.Functional.Tests.Helpers;
    using SpaTemplate.Infrastructure.Api;
    using SpaTemplate.Tests.Helpers;
    using Xeinaemm.Hateoas;
    using Xeinaemm.Tests;
    using Xunit;

    public class IPeopleApiShould : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly IPeopleApi api;

        public IPeopleApiShould(CustomWebApplicationFactory<Startup> factory)
        {
            using var scope = InitializeContainer.InitializeTests(factory.CreateClientWithDefaultRequestHeaders()).BeginLifetimeScope();
            this.api = scope.Resolve<IPeopleApi>();
        }

        [Fact]
        public void ReturnsBadRequestOrderByNotExists() =>
            RefitExceptions.Verify(async () => await this.api.GetPeople(new StudentParameters { OrderBy = "unknown" }), HttpStatusCode.BadRequest);

        [Fact]
        public void ReturnsBadRequestFieldsMappingNotExists() =>
            RefitExceptions.Verify(async () => await this.api.GetPeople(new StudentParameters { Fields = "dummy" }), HttpStatusCode.BadRequest);

        [Fact]
        public async Task ReturnsCollection()
        {
            var get = await this.api.GetPeople(new StudentParameters());
            Assert.Equal(10, get.Count());
        }

        [Theory]
        [InlineData("GET", Rel.Self, 0)]
        [InlineData("POST", SpaTemplateRel.CreateStudent, 1)]
        [InlineData("PATCH", SpaTemplateRel.PatchStudent, 2)]
        [InlineData("DELETE", SpaTemplateRel.DeleteStudent, 3)]
        public async Task ReturnsCollectionValidMethodAndRelInsideLinks(string method, string rel, int number)
        {
            var get = await this.api.GetPeopleHateoas(new StudentParameters());

            Assert.Equal(10, get.Values.Count);
            Assert.Equal(4, get.Values[0].Links.Count);

            var linkDto = get.Values[0].Links[number];
            Assert.Equal(method, linkDto.Method);
            Assert.Equal(rel, linkDto.Rel);
        }

        [Theory]
        [InlineData("GET", Rel.Self, 0)]
        [InlineData("GET", Rel.NextPage, 1)]
        public async Task ReturnsCollectionValidMethodAndRelOutsideLinks(string method, string rel, int number)
        {
            var get = await this.api.GetPeopleHateoas(new StudentParameters());

            Assert.Equal(2, get.Links.Count);
            Assert.Equal(method, get.Links[number].Method);
            Assert.Equal(rel, get.Links[number].Rel);
        }

        [Fact]
        public void ReturnsNotFoundEntityNotExists() =>
            RefitExceptions.Verify(async () => await this.api.GetStudent(Guid.NewGuid(), new StudentParameters()), HttpStatusCode.NotFound);

        [Fact]
        public async Task ReturnsNotNullHref()
        {
            var get = await this.api.GetPeopleHateoas(new StudentParameters());
            Assert.All(get.Values[0].Links, x => Assert.NotNull(x.Href));
            Assert.All(get.Links, x => Assert.NotNull(x.Href));
            Assert.Contains(get.Links, x => x.Rel == Rel.NextPage);

            var getSecond = await this.api.GetPeopleHateoas(new StudentParameters { PageNumber = 2 });
            Assert.All(getSecond.Values[0].Links, x => Assert.NotNull(x.Href));
            Assert.All(getSecond.Links, x => Assert.NotNull(x.Href));
            Assert.Contains(getSecond.Links, x => x.Rel == Rel.PreviousPage);
        }

        [Theory]
        [AutoMoqData]
        public async Task ReturnsCreated(StudentForCreationDto studentForCreationDto)
        {
            var post = await this.api.CreateStudent(studentForCreationDto);
            Assert.NotNull(post);
        }

        [Fact]
        public void ReturnsNotFoundEntityForPatchNotExists() =>
            RefitExceptions.Verify(async () => await this.api.PartiallyUpdateStudent(Guid.NewGuid(), new JsonPatchDocument<StudentForUpdateDto>()), HttpStatusCode.NotFound);

        [Fact]
        public void ReturnsNotFoundEntity() =>
            RefitExceptions.Verify(async () => await this.api.DeleteStudent(Guid.NewGuid()), HttpStatusCode.NotFound);

        [Theory]
        [AutoMoqData]
        public async Task ReturnsBadRequestFieldNotExists(StudentForCreationDto studentForCreationDto)
        {
            var post = await this.api.CreateStudent(studentForCreationDto);
            RefitExceptions.Verify(async () => await this.api.GetStudent(post.Id, new StudentParameters { Fields = "dummy" }), HttpStatusCode.BadRequest);
        }

        [Theory]
        [AutoMoqData]
        public async Task ReturnsNoContentValidPatch(StudentForCreationDto studentForCreationDto)
        {
            var post = await this.api.CreateStudent(studentForCreationDto);

            var patchDoc = new JsonPatchDocument<StudentForUpdateDto>();
            patchDoc.Replace(x => x.Surname, "Dummy");

            var patch = await this.api.PartiallyUpdateStudent(post.Id, patchDoc);
            Assert.Null(patch);
        }

        [Theory]
        [AutoMoqData]
        public async Task ReturnsUnprocessableEntityEqualFields(StudentForCreationDto studentForCreationDto)
        {
            var post = await this.api.CreateStudent(studentForCreationDto);

            var patchDoc = new JsonPatchDocument<StudentForUpdateDto>();
            patchDoc.Replace(x => x.Name, post.Surname);
            RefitExceptions.Verify(async () => await this.api.PartiallyUpdateStudent(post.Id, patchDoc), HttpStatusCode.UnprocessableEntity);
        }

        [Theory]
        [AutoMoqData]
        public async Task RemoveStudent(StudentForCreationDto studentForCreationDto)
        {
            var post = await this.api.CreateStudent(studentForCreationDto);
            await this.api.DeleteStudent(post.Id);
            RefitExceptions.Verify(async () => await this.api.GetStudent(post.Id, new StudentParameters()), HttpStatusCode.NotFound);
        }
    }
}
