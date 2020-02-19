﻿// -----------------------------------------------------------------------
// <copyright file="ICoursesApiShould.cs" company="Piotr Xeinaemm Czech">
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

    public class ICoursesApiShould : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly ICoursesApi api;
        private readonly IPeopleApi peopleApi;

        public ICoursesApiShould(CustomWebApplicationFactory<Startup> factory)
        {
            using var scope = InitializeContainer.InitializeTests(factory.CreateClientWithDefaultRequestHeaders()).BeginLifetimeScope();
            this.api = scope.Resolve<ICoursesApi>();
            this.peopleApi = scope.Resolve<IPeopleApi>();
        }

        [Theory]
        [AutoMoqData]
        public void ReturnsNotFoundPostCourseToStudentThatNotExists(CourseForCreationDto dto) =>
            RefitExceptions.Verify(async () => await this.api.CreateCourseForStudent(Guid.NewGuid(), dto), HttpStatusCode.NotFound);

        [Theory]
        [AutoMoqData]
        public async Task ReturnsCreated(CourseForCreationDto dto)
        {
            var post = await this.api.CreateCourseForStudent(Guid.NewGuid(), dto);
            Assert.NotNull(post);
        }

        [Fact]
        public void ReturnsBadRequestCourseIsNull() =>
            RefitExceptions.Verify(async () => await this.api.CreateCourseForStudent(Guid.NewGuid(), null), HttpStatusCode.BadRequest);

        [Fact]
        public async Task ReturnsBadRequestFieldAndMappingNotExists()
        {
            var person = (await this.peopleApi.GetPeople(new StudentParameters())).First();
            RefitExceptions.Verify(async () => await this.api.GetCoursesForStudent(person.Id, new CourseParameters { Fields = "dummy" }), HttpStatusCode.BadRequest);
        }

        [Theory]
        [AutoMoqData]
        public async Task ReturnsCreatedNewCourse(CourseForUpdateDto dto)
        {
            var person = (await this.peopleApi.GetPeople(new StudentParameters())).First();
            var put = await this.api.UpdateCourseForStudent(person.Id, Guid.NewGuid(), dto);
            Assert.NotNull(put);
        }

        [Theory]
        [AutoMoqData]
        public void ReturnsNotFoundStudentNotExists(CourseForUpdateDto dto) =>
            RefitExceptions.Verify(async () => await this.api.UpdateCourseForStudent(Guid.NewGuid(), Guid.NewGuid(), dto), HttpStatusCode.NotFound);

        [Theory]
        [InlineData("GET", Rel.Self, 0)]
        [InlineData("POST", SpaTemplateRel.CreateCourseForStudent, 1)]
        [InlineData("PATCH", SpaTemplateRel.PartiallyUpdateCourse, 2)]
        [InlineData("PUT", SpaTemplateRel.UpdateCourse, 3)]
        [InlineData("DELETE", SpaTemplateRel.DeleteCourse, 4)]
        public async Task ReturnsCollectionWithHateoasInsideLinks(string method, string rel, int number)
        {
            var person = (await this.peopleApi.GetPeople(new StudentParameters())).First();
            var get = await this.api.GetCoursesForStudentHateoas(person.Id, new CourseParameters());

            Assert.Equal(10, get.Values.Count);
            Assert.Equal(5, get.Values[0].Links.Count);

            Assert.Equal(method, get.Values[0].Links[number].Method);
            Assert.Equal(rel, get.Values[0].Links[number].Rel);
        }

        [Theory]
        [InlineData("GET", Rel.Self, 0)]
        [InlineData("GET", Rel.NextPage, 1)]
        public async Task ReturnsCollectionWithHateoasOutsideLinks(string method, string rel, int number)
        {
            var person = (await this.peopleApi.GetPeople(new StudentParameters())).First();
            var get = await this.api.GetCoursesForStudentHateoas(person.Id, new CourseParameters());

            Assert.Equal(2, get.Links.Count);
            Assert.Equal(method, get.Links[number].Method);
            Assert.Equal(rel, get.Links[number].Rel);
        }

        [Theory]
        [AutoMoqData]
        public async Task ReturnsSameEntityCourseCreated(CourseForCreationDto courseForCreationDto)
        {
            var person = (await this.peopleApi.GetPeople(new StudentParameters())).First();
            var post = await this.api.CreateCourseForStudent(person.Id, courseForCreationDto);
            var get = await this.api.GetCourseForStudent(person.Id, post.Id);
            Assert.Equal(post.Id, get.Id);
        }

        [Fact]
        public async Task ReturnsBadRequestPatchDocIsNull()
        {
            var person = (await this.peopleApi.GetPeople(new StudentParameters())).First();
            RefitExceptions.Verify(async () => await this.api.PartiallyUpdateCourseForStudent(person.Id, Guid.NewGuid(), null), HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task ReturnsCreatedNewCoursePartiallyUpdate()
        {
            var person = (await this.peopleApi.GetPeople(new StudentParameters())).First();
            var patchDoc = new JsonPatchDocument<CourseForUpdateDto>();
            patchDoc.Replace(x => x.Description, "Dummy");

            var patch = await this.api.PartiallyUpdateCourseForStudent(person.Id, Guid.NewGuid(), patchDoc);
            Assert.NotNull(patch);
        }

        [Theory]
        [AutoMoqData]
        public async Task ReturnsNoContentValidPatch(CourseForCreationDto courseForCreationDto)
        {
            var person = (await this.peopleApi.GetPeople(new StudentParameters())).First();
            var post = await this.api.CreateCourseForStudent(person.Id, courseForCreationDto);
            var patchDoc = new JsonPatchDocument<CourseForUpdateDto>();
            patchDoc.Replace(x => x.Description, "Dummy");

            var patch = await this.api.PartiallyUpdateCourseForStudent(person.Id, post.Id, patchDoc);
            Assert.Null(patch);
        }

        [Theory]
        [AutoMoqData]
        public async Task ReturnsUnprocessableEntityPropertiesEqualExistingCourse(
            CourseForCreationDto courseForCreationDto)
        {
            var person = (await this.peopleApi.GetPeople(new StudentParameters())).First();
            var post = await this.api.CreateCourseForStudent(person.Id, courseForCreationDto);

            var patchDoc = new JsonPatchDocument<CourseForUpdateDto>();
            patchDoc.Replace(x => x.Description, "Dummy");
            patchDoc.Replace(x => x.Title, "Dummy");
            RefitExceptions.Verify(async () => await this.api.PartiallyUpdateCourseForStudent(person.Id, post.Id, patchDoc), HttpStatusCode.UnprocessableEntity);
        }

        [Theory]
        [AutoMoqData]
        public async Task ReturnsUnprocessableEntityPropertiesEqualsNewCourse(CourseForCreationDto courseForCreationDto)
        {
            var person = (await this.peopleApi.GetPeople(new StudentParameters())).First();
            var post = await this.api.CreateCourseForStudent(person.Id, courseForCreationDto);
            var patchDoc = new JsonPatchDocument<CourseForUpdateDto>();
            patchDoc.Replace(x => x.Description, "Dummy");
            patchDoc.Replace(x => x.Title, "Dummy");
            RefitExceptions.Verify(async () => await this.api.PartiallyUpdateCourseForStudent(person.Id, post.Id, patchDoc), HttpStatusCode.UnprocessableEntity);
        }

        [Theory]
        [AutoMoqData]
        public async Task ReturnsNoContentExistingCourse(CourseForCreationDto courseForCreationDto, CourseForUpdateDto courseForUpdateDto)
        {
            var person = (await this.peopleApi.GetPeople(new StudentParameters())).First();
            var post = await this.api.CreateCourseForStudent(person.Id, courseForCreationDto);
            var put = await this.api.UpdateCourseForStudent(person.Id, post.Id, courseForUpdateDto);
            Assert.Null(put);
        }

        [Theory]
        [AutoMoqData]
        public async Task ReturnsUnprocessableEntityPropertiesEqual(CourseForCreationDto courseForCreationDto, CourseForUpdateDto courseForUpdateDto)
        {
            courseForUpdateDto.Description = "Dummy";
            courseForUpdateDto.Title = "Dummy";
            var person = (await this.peopleApi.GetPeople(new StudentParameters())).First();
            var post = await this.api.CreateCourseForStudent(person.Id, courseForCreationDto);
            RefitExceptions.Verify(async () => await this.api.UpdateCourseForStudent(person.Id, post.Id, courseForUpdateDto), HttpStatusCode.UnprocessableEntity);
        }

        [Theory]
        [AutoMoqData]
        public async Task ReturnsNoContentCourseDeleted(CourseForCreationDto courseForCreationDto)
        {
            var person = (await this.peopleApi.GetPeople(new StudentParameters())).First();
            var post = await this.api.CreateCourseForStudent(person.Id, courseForCreationDto);
            await this.api.DeleteCourseForStudent(person.Id, post.Id);
            RefitExceptions.Verify(async () => await this.api.GetCourseForStudent(person.Id, post.Id), HttpStatusCode.NotFound);
        }

        [Fact]
        public void ReturnsNotFoundStudentNotExistsAsync() =>
            RefitExceptions.Verify(async () => await this.api.GetCoursesForStudent(Guid.NewGuid(), new CourseParameters()), HttpStatusCode.NotFound);

        [Fact]
        public void PatchReturnsNotFoundStudentNotExists() =>
            RefitExceptions.Verify(async () => await this.api.PartiallyUpdateCourseForStudent(Guid.NewGuid(), Guid.NewGuid(), new JsonPatchDocument<CourseForUpdateDto>()), HttpStatusCode.NotFound);

        [Fact]
        public void ReturnsNotFoundStudentNotExistsDelete() =>
            RefitExceptions.Verify(async () => await this.api.DeleteCourseForStudent(Guid.NewGuid(), Guid.NewGuid()), HttpStatusCode.NotFound);

        [Fact]
        public async Task ReturnsCollectionWithoutHateoas()
        {
            var person = (await this.peopleApi.GetPeople(new StudentParameters())).First();
            var get = await this.api.GetCoursesForStudent(person.Id, new CourseParameters());
            Assert.Equal(10, get.Count());
        }

        [Fact]
        public async Task ReturnsNotFoundCourseNotExists()
        {
            var person = (await this.peopleApi.GetPeople(new StudentParameters())).First();
            RefitExceptions.Verify(async () => await this.api.GetCourseForStudent(person.Id, Guid.NewGuid()), HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task ReturnsNotNullHrefCoursesWithHateoas()
        {
            var person = (await this.peopleApi.GetPeople(new StudentParameters())).First();
            var get = await this.api.GetCoursesForStudentHateoas(person.Id, new CourseParameters());

            Assert.All(get.Values[0].Links, x => Assert.NotNull(x.Href));
            Assert.All(get.Links, x => Assert.NotNull(x.Href));
        }

        [Fact]
        public async Task ReturnsOKGetCourses()
        {
            var person = (await this.peopleApi.GetPeople(new StudentParameters())).First();
            var get = await this.api.GetCoursesForStudent(person.Id, new CourseParameters());
            Assert.NotNull(get);
        }

        [Fact]
        public async Task ReturnsOKGetCoursesWithHateoas()
        {
            var person = (await this.peopleApi.GetPeople(new StudentParameters())).First();
            var get = await this.api.GetCoursesForStudent(person.Id, new CourseParameters(), MediaType.OutputFormatterJson);
            Assert.NotNull(get);
        }

        [Fact]
        public async Task ReturnsBadRequestCourseIsNullUpdate()
        {
            var person = (await this.peopleApi.GetPeople(new StudentParameters())).First();
            RefitExceptions.Verify(async () => await this.api.UpdateCourseForStudent(person.Id, Guid.NewGuid(), null), HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task ReturnsNotFoundCourseNotExistsDelete()
        {
            var person = (await this.peopleApi.GetPeople(new StudentParameters())).First();
            RefitExceptions.Verify(async () => await this.api.UpdateCourseForStudent(person.Id, Guid.NewGuid(), null), HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task ReturnsOKCourseExistsInStudent()
        {
            var person = (await this.peopleApi.GetPeople(new StudentParameters())).First();
            var get = await this.api.GetCoursesForStudent(person.Id, new CourseParameters());
            var getCourse = await this.api.GetCourseForStudent(person.Id, get.First().Id);
            Assert.NotNull(getCourse);
        }

        [Fact]
        public async Task ReturnsOKCourseShapedWithHateoas()
        {
            var person = (await this.peopleApi.GetPeople(new StudentParameters())).First();
            var get = await this.api.GetCoursesForStudentHateoas(person.Id, new CourseParameters());
            var getCourse = await this.api.GetCourseForStudent(person.Id, get.Values[0].Values[0].Id);
            Assert.NotNull(getCourse);
        }
    }
}
