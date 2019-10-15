// -----------------------------------------------------------------------
// <copyright file="ApiCoursesControllerShould.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Tests.FunctionalTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.JsonPatch;
    using Newtonsoft.Json;
    using SpaTemplate.Core.FacultyContext;
    using SpaTemplate.Core.SharedKernel;
    using SpaTemplate.Infrastructure.Api;
    using SpaTemplate.Tests.Helpers;
    using Xeinaemm.Hateoas;
    using Xeinaemm.Tests;
    using Xunit;

    /// <inheritdoc />
    /// <summary>
    /// <para>
    ///     A - Standard calls to API without combinations(POST + GET, POST + DELETE etc.)
    ///     B - Extended calls that test API in isolation without prepared data in the database to remove hardcoded values.
    ///         Firstly we call POST to prepare object and then exercise another action(ie. DELETE).
    ///     C - If you exercise more actions (ie. 3) then use next letter to distinct number of calls, ie. C = 3 D = 4, E = 5 etc.
    ///     Number defines tests for particular action(ie. GET)
    ///     1 - GET,
    ///     2 - POST,
    ///     3 - PATCH,
    ///     4 - PUT,
    ///     5 - DELETE.
    /// </para>
    /// <para>    The functional test needs to be tested in particular order due to domino's effect of API, ie. if POST will fail, others too.</para>
    /// </summary>
    public sealed partial class ApiCoursesControllerShould : IClassFixture<CustomWebApplicationFactory<Startup>>, IDisposable
    {
        private readonly HttpClient httpClient;
        private readonly string baseAddress = "http://localhost:64412/";

        public ApiCoursesControllerShould(CustomWebApplicationFactory<Startup> factory)
        {
            //this.httpClient = factory.CreateClientWithDefaultRequestHeaders();
            this.baseAddress = this.httpClient.BaseAddress.ToString();
        }

        [Theory]
        [InlineData("?orderBy=unknown")]
        [InlineData("?fields=dummy")]
        public async Task A1GetReturnsBadRequestFieldAndMappingNotExistAsync(string field)
        {
            var id = await this.GetFirstIEnumerableStudentIdAsync();
            var get = await this.courseEndpoint.GetCoursesForStudentAsync(id, new CourseParameters { Fields = field });
            Assert.Equal(HttpStatusCode.BadRequest, get.StatusCode);
        }

        [Theory]
        [AutoMoqData]
        public async Task A2PostReturnsNotFoundPostCourseToStudentThatNotExistsAsync(CourseForCreationDto dto)
        {
            var post = await this.PostAsync(Guid.NewGuid(), dto).ConfigureAwait(false);
            Assert.Equal(HttpStatusCode.NotFound, post.StatusCode);
        }

        [Theory]
        [AutoMoqData]
        public async Task A4PutReturnsCreatedNewCourseAsync(CourseForUpdateDto dto)
        {
            var put = await this.PutAsync(await this.GetFirstIEnumerableStudentIdAsync().ConfigureAwait(false), Guid.NewGuid(), dto)
                .ConfigureAwait(false);
            Assert.Equal(HttpStatusCode.Created, put.StatusCode);
        }

        [Theory]
        [AutoMoqData]
        public async Task A4PutReturnsNotFoundStudentNotExistsAsync(CourseForUpdateDto dto)
        {
            var put = await this.PutAsync(Guid.NewGuid(), Guid.NewGuid(), dto).ConfigureAwait(false);
            Assert.Equal(HttpStatusCode.NotFound, put.StatusCode);
        }

        [Theory]
        [InlineData(Method.Get, Rel.Self, 0)]
        [InlineData(Method.Post, SpaTemplateRel.CreateCourseForStudent, 1)]
        [InlineData(Method.Patch, SpaTemplateRel.PartiallyUpdateCourse, 2)]
        [InlineData(Method.Put, SpaTemplateRel.UpdateCourse, 3)]
        [InlineData(Method.Delete, SpaTemplateRel.DeleteCourse, 4)]
        public async Task B1GetReturnsCollectionWithHateoasInsideLinksAsync(string method, string rel, int number)
        {
            var (get, _) = await this.GetHateoasCoursesAndStudentIdAsync().ConfigureAwait(false);

            Assert.Equal(10, get.Values.Count);
            Assert.Equal(5, get.Values[0].Links.Count);

            Assert.Equal(method, get.Values[0].Links[number].Method);
            Assert.Equal(rel, get.Values[0].Links[number].Rel);
        }

        [Theory]
        [InlineData(Method.Get, Rel.Self, 0)]
        [InlineData(Method.Get, Rel.NextPage, 1)]
        public async Task B1GetReturnsCollectionWithHateoasOutsideLinksAsync(string method, string rel, int number)
        {
            var (get, _) = await this.GetHateoasCoursesAndStudentIdAsync().ConfigureAwait(false);

            Assert.Equal(2, get.Links.Count);
            Assert.Equal(method, get.Links[number].Method);
            Assert.Equal(rel, get.Links[number].Rel);
        }

        [Theory]
        [AutoMoqData]
        public async Task B2PostReturnsBadRequestUnprocessableEntityAsync(CourseForCreationDto courseForCreationDto)
        {
            courseForCreationDto.Description = "Dummy";
            courseForCreationDto.Title = "Dummy";
            var (post, _) = await this.PostCourseAsync(courseForCreationDto).ConfigureAwait(false);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, post.StatusCode);
        }

        [Theory]
        [AutoMoqData]
        public async Task B2PostReturnsCreatedAsync(CourseForCreationDto dto)
        {
            var (post, _) = await this.PostCourseAsync(dto).ConfigureAwait(false);
            Assert.Equal(HttpStatusCode.Created, post.StatusCode);
        }

        [Theory]
        [AutoMoqData]
        public async Task C1GetPaginationNextPageNotNullAsync(CourseForCreationDto dto)
        {
            var (_, studentId) = await this.PostCourseAsync(dto).ConfigureAwait(false);
            var get = await this.GetCoursesAsync(studentId).ConfigureAwait(false);
            var header = get.Headers.GetValues(Header.XPagination);
            var pagination = JsonConvert.DeserializeObject<HateoasPagination>(header.First());
            Assert.NotNull(pagination.NextPage);
        }

        [Theory]
        [AutoMoqData]
        public async Task C2PostReturnsSameEntityCourseCreatedAsync(CourseForCreationDto courseForCreationDto)
        {
            var (post, studentId) = await this.PostCourseAsync<CourseDto>(courseForCreationDto).ConfigureAwait(false);
            var get = JsonConvert.DeserializeObject<CourseDto>(
                await (await this.GetCourseAsync(studentId, post.Id).ConfigureAwait(false))
                    .Content.ReadAsStringAsync().ConfigureAwait(false));

            Assert.Equal(post.Id, get.Id);
        }

        [Theory]
        [AutoMoqData]
        public async Task C3PatchReturnsBadRequestPatchDocIsNullAsync(CourseForCreationDto courseForCreationDto)
        {
            var (_, studentId) = await this.PostCourseAsync(courseForCreationDto).ConfigureAwait(false);
            var patch = await this.PatchAsync(studentId, Guid.NewGuid(), null).ConfigureAwait(false);
            Assert.Equal(HttpStatusCode.BadRequest, patch.StatusCode);
        }

        [Theory]
        [AutoMoqData]
        public async Task C3PatchReturnsCreatedNewCourseAsync(CourseForCreationDto courseForCreationDto)
        {
            var (_, studentId) = await this.PostCourseAsync(courseForCreationDto).ConfigureAwait(false);
            var patchDoc = new JsonPatchDocument<CourseForUpdateDto>();
            patchDoc.Replace(x => x.Description, "Dummy");

            var patch = await this.PatchAsync(studentId, Guid.NewGuid(), patchDoc).ConfigureAwait(false);
            Assert.Equal(HttpStatusCode.Created, patch.StatusCode);
        }

        [Theory]
        [AutoMoqData]
        public async Task C3PatchReturnsNoContentValidPatchAsync(CourseForCreationDto courseForCreationDto)
        {
            var (post, studentId) = await this.PostCourseAsync<CourseDto>(courseForCreationDto).ConfigureAwait(false);
            var patchDoc = new JsonPatchDocument<CourseForUpdateDto>();
            patchDoc.Replace(x => x.Description, "Dummy");

            var patch = await this.PatchAsync(studentId, post.Id, patchDoc).ConfigureAwait(false);
            Assert.Equal(HttpStatusCode.NoContent, patch.StatusCode);
        }

        [Theory]
        [AutoMoqData]
        public async Task C3PatchReturnsUnprocessableEntityPropertiesEqualExistingCourseAsync(
            CourseForCreationDto courseForCreationDto)
        {
            var (post, studentId) = await this.PostCourseAsync<CourseDto>(courseForCreationDto).ConfigureAwait(false);

            var patchDoc = new JsonPatchDocument<CourseForUpdateDto>();
            patchDoc.Replace(x => x.Description, "Dummy");
            patchDoc.Replace(x => x.Title, "Dummy");

            var patch = await this.PatchAsync(studentId, post.Id, patchDoc).ConfigureAwait(false);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, patch.StatusCode);
        }

        [Theory]
        [AutoMoqData]
        public async Task C3PatchReturnsUnprocessableEntityPropertiesEqualsNewCourseAsync(
            CourseForCreationDto courseForCreationDto)
        {
            var (_, studentId) = await this.PostCourseAsync(courseForCreationDto).ConfigureAwait(false);
            var patchDoc = new JsonPatchDocument<CourseForUpdateDto>();
            patchDoc.Replace(x => x.Description, "Dummy");
            patchDoc.Replace(x => x.Title, "Dummy");

            var patch = await this.PatchAsync(studentId, Guid.NewGuid(), patchDoc).ConfigureAwait(false);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, patch.StatusCode);
        }

        [Theory]
        [AutoMoqData]
        public async Task C4PutReturnsNoContentExistingCourseAsync(CourseForUpdateDto courseForCreationDto)
        {
            var (post, studentId) = await this.PostCourseAsync<CourseDto>(courseForCreationDto).ConfigureAwait(false);
            var put = await this.PutAsync(studentId, post.Id, courseForCreationDto).ConfigureAwait(false);
            Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);
        }

        [Theory]
        [AutoMoqData]
        public async Task C4PutReturnsUnprocessableEntityPropertiesEqualAsync(CourseForUpdateDto courseForCreationDto)
        {
            courseForCreationDto.Description = "Dummy";
            courseForCreationDto.Title = "Dummy";
            var (post, studentId) = await this.PostCourseAsync<CourseDto>(courseForCreationDto).ConfigureAwait(false);
            var put = await this.PutAsync(studentId, post.Id, courseForCreationDto).ConfigureAwait(false);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, put.StatusCode);
        }

        [Theory]
        [AutoMoqData]
        public async Task C5DeleteReturnsNoContentCourseDeletedAsync(CourseForCreationDto courseForCreationDto)
        {
            var (post, studentId) = await this.PostCourseAsync<CourseDto>(courseForCreationDto).ConfigureAwait(false);
            var delete = await this.DeleteAsync(studentId, post.Id).ConfigureAwait(false);
            Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        }

        [Fact]
        public async Task A1GetReturnsNotFoundStudentNotExistsAsync()
        {
            var get = await this.GetCoursesAsync(Guid.NewGuid()).ConfigureAwait(false);
            Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
        }

        [Fact]
        public async Task A2PostReturnsBadRequestCourseIsNullAsync()
        {
            var (post, _) = await this.PostCourseAsync(null).ConfigureAwait(false);
            Assert.Equal(HttpStatusCode.BadRequest, post.StatusCode);
        }

        [Fact]
        public async Task A3PatchReturnsNotFoundStudentNotExistsAsync()
        {
            var patch = await this.PatchAsync(Guid.NewGuid(), Guid.NewGuid(), new JsonPatchDocument<CourseForUpdateDto>())
                .ConfigureAwait(false);
            Assert.Equal(HttpStatusCode.NotFound, patch.StatusCode);
        }

        [Fact]
        public async Task A5DeleteReturnsNotFoundStudentNotExistsAsync()
        {
            var delete = await this.DeleteAsync(Guid.NewGuid(), Guid.NewGuid()).ConfigureAwait(false);
            Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);
        }

        [Fact]
        public async Task B1GetReturnsCollectionWithoutHateoasAsync()
        {
            var (courses, _) = await this.GetIEnumerableCoursesAndStudentIdAsync().ConfigureAwait(false);
            Assert.Equal(10, courses.Count());
        }

        [Fact]
        public async Task B1GetReturnsNotFoundCourseNotExistAsync()
        {
            var get = await this.GetCourseAsync(
                await this.GetFirstIEnumerableStudentIdAsync().ConfigureAwait(false),
                Guid.NewGuid())
                .ConfigureAwait(false);

            Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
        }

        [Fact]
        public async Task B1GetReturnsNotNullHrefCoursesWithHateoasAsync()
        {
            var (get, _) = await this.GetHateoasCoursesAndStudentIdAsync().ConfigureAwait(false);

            Assert.All(get.Values[0].Links, x => Assert.NotNull(x.Href));
            Assert.All(get.Links, x => Assert.NotNull(x.Href));
        }

        [Fact]
        public async Task B1GetReturnsOKGetCoursesAsync()
        {
            var get = await this.GetCoursesAsync(await this.GetFirstIEnumerableStudentIdAsync().ConfigureAwait(false))
                .ConfigureAwait(false);
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        }

        [Fact]
        public async Task B1GetReturnsOKGetCoursesWithHateoasAsync()
        {
            var getCourse = await this.GetCoursesAsync(await this.GetFirstHateoasStudentIdAsync().ConfigureAwait(false))
                .ConfigureAwait(false);
            Assert.Equal(HttpStatusCode.OK, getCourse.StatusCode);
        }

        [Fact]
        public async Task B4PutReturnsBadRequestCourseIsNullAsync()
        {
            var put = await this.PutAsync(await this.GetFirstIEnumerableStudentIdAsync().ConfigureAwait(false), Guid.NewGuid(), null)
                .ConfigureAwait(false);
            Assert.Equal(HttpStatusCode.BadRequest, put.StatusCode);
        }

        [Fact]
        public async Task B5DeleteReturnsNotFoundCourseNotExistsAsync()
        {
            var delete = await this.DeleteAsync(
                await this.GetFirstIEnumerableStudentIdAsync().ConfigureAwait(false), Guid.NewGuid())
                .ConfigureAwait(false);
            Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);
        }

        [Fact]
        public async Task C1GetReturnsOKCourseExistsInStudentAsync()
        {
            var (get, studentId) = await this.GetIEnumerableCoursesAndStudentIdAsync().ConfigureAwait(false);

            var getCourse = await this.GetCourseAsync(studentId, get.First().Id).ConfigureAwait(false);
            Assert.Equal(HttpStatusCode.OK, getCourse.StatusCode);
        }

        [Fact]
        public async Task C1GetReturnsOKCourseShapedWithHateoasAsync()
        {
            var (get, studentId) = await this.GetHateoasCoursesAndStudentIdAsync().ConfigureAwait(false);

            var getCourse = await this.GetCourseAsync(studentId, get.Values[0].Id).ConfigureAwait(false);
            Assert.Equal(HttpStatusCode.OK, getCourse.StatusCode);
        }

        public void Dispose()
        {
            this.httpClient.Dispose();
        }
    }

    public partial class ApiCoursesControllerShould
    {
        private async Task<Guid> GetFirstHateoasStudentIdAsync()
        {
            this.client.DefaultRequestHeaders.TryAddWithoutValidation(Header.Accept, MediaType.OutputFormatterJson);
            return (await this.GetPeopleAsync<HateoasDto<StudentDto>>().ConfigureAwait(false)).Values[0].Id;
        }

        private async Task<Guid> GetFirstIEnumerableStudentIdAsync() =>
            (await this.GetPeopleAsync<List<StudentDto>>().ConfigureAwait(false))[0].Id;

        private async Task<(HateoasCollectionDto<CourseDto> Courses, Guid StudentId)> GetHateoasCoursesAndStudentIdAsync()
        {
            var studentId = await this.GetFirstHateoasStudentIdAsync().ConfigureAwait(false);
            return (await this.courseEndpoint.GetCoursesForStudentAsync(studentId), studentId);
        }

        private async Task<(IEnumerable<CourseDto> Courses, Guid StudentId)> GetIEnumerableCoursesAndStudentIdAsync()
        {
            var studentId = await this.GetFirstIEnumerableStudentIdAsync().ConfigureAwait(false);
            return (JsonConvert.DeserializeObject<IEnumerable<CourseDto>>(
                await (await this.GetCoursesAsync(studentId).ConfigureAwait(false))
                    .Content
                    .ReadAsStringAsync().ConfigureAwait(false)), studentId);
        }

        private Task<TCollection> GetPeopleAsync<TCollection>() => $"{this.baseAddress}{Api.People}".GetJsonAsync<TCollection>();

        private Task<HttpResponseMessage> PatchAsync(
            Guid studentId,
            Guid courseId,
            JsonPatchDocument<CourseForUpdateDto> patchDoc) => this.client != null
            ? this.client.PatchAsync(
                new Uri(Api.Course, UriKind.Relative).ToApiUrl(studentId, courseId),
                patchDoc.Content(MediaType.PatchFormatterJson))
            : Task.FromResult<HttpResponseMessage>(null);

        private Task<HttpResponseMessage> PostAsync(Guid studentId, CourseForManipulationDto dto)
        {

            return this.client != null
? this.client.PostAsync(new Uri(Api.Courses, UriKind.Relative).ToApiUrl(studentId), dto.Content(MediaType.InputFormatterJson))
: Task.FromResult<HttpResponseMessage>(null);
        }

        private async Task<(HttpResponseMessage Response, Guid StudentId)> PostCourseAsync(
            CourseForManipulationDto courseDto)
        {
            var studentId = await this.GetFirstIEnumerableStudentIdAsync().ConfigureAwait(false);
            return (await this.PostAsync(studentId, courseDto).ConfigureAwait(false), studentId);
        }

        private async Task<(T course, Guid StudentId)> PostCourseAsync<T>(
            CourseForManipulationDto courseDto)
        {
            var studentId = await this.GetFirstIEnumerableStudentIdAsync().ConfigureAwait(false);
            return (JsonConvert.DeserializeObject<T>(await (await this.PostAsync(studentId, courseDto).ConfigureAwait(false))
                    .Content
                    .ReadAsStringAsync().ConfigureAwait(false)),
                studentId);
        }

        private Task<HttpResponseMessage> PutAsync(Guid studentId, Guid courseId, CourseForManipulationDto dto) =>
            this.client.PutAsync(new Uri(Api.Course, UriKind.Relative).ToApiUrl(studentId, courseId), dto.Content(MediaType.InputFormatterJson));
    }
}