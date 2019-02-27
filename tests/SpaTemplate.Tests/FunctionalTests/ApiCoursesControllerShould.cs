using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using SpaTemplate.Core.FacultyContext;
using SpaTemplate.Core.SharedKernel;
using SpaTemplate.Infrastructure.Api;
using SpaTemplate.Tests.Helpers;
using Xunit;
using static Newtonsoft.Json.JsonConvert;

namespace SpaTemplate.Tests.FunctionalTests
{
    /// <inheritdoc />
    /// <summary>
    ///     A - Standard calls to API without combinations(POST + GET, POST + DELETE etc.)
    ///     B - Extended calls that test API in isolation without prepared data in the database to remove hardcoded values.
    ///     Firstly we call POST to prepare object and then exercise another action(ie. DELETE).
    ///     C - If you exercise more actions (ie. 3) then use next letter to distinct number of calls, ie. C = 3 D = 4, E = 5
    ///     etc.
    ///     Number defines tests for particular action(ie. GET)
    ///     1 - GET,
    ///     2 - POST,
    ///     3 - PATCH,
    ///     4 - PUT,
    ///     5 - DELETE
    ///     The functional test needs to be tested in particular order due to domino's effect of API, ie. GET will fail and
    ///     then others too.
    /// </summary>
    public partial class ApiCoursesControllerShould : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        public ApiCoursesControllerShould(CustomWebApplicationFactory<Startup> factory) =>
            _client = factory.CreateClientWithDefaultRequestHeaders();

        private readonly HttpClient _client;

        [Theory]
        [InlineData("?orderBy=unknown")]
        [InlineData("?fields=dummy")]
        public async Task A1_GetReturnsBadRequest_FieldAndMappingNotExist(string field)
        {
            var get = await GetCoursesAsync(await GetFirstIEnumerableStudentId(), field);
            Assert.Equal(HttpStatusCode.BadRequest, get.StatusCode);
        }

        [Theory]
        [InlineData(Method.Get, Rel.Self, 0)]
        [InlineData(Method.Post, Rel.CreateCourseForStudent, 1)]
        [InlineData(Method.Patch, Rel.PartiallyUpdateCourse, 2)]
        [InlineData(Method.Put, Rel.UpdateCourse, 3)]
        [InlineData(Method.Delete, Rel.DeleteCourse, 4)]
        public async Task B1_GetReturnsCollection_WithHateoasInsideLinks(string method, string rel, int number)
        {
            var (get, _) = await GetHateoasCoursesAndStudentIdAsync();

            Assert.Equal(10, get.Values.Count);
            Assert.Equal(5, get.Values.First().Links.Count);

            Assert.Equal(method, get.Values.First().Links[number].Method);
            Assert.Equal(rel, get.Values.First().Links[number].Rel);
        }


        [Theory]
        [InlineData(Method.Get, Rel.Self, 0)]
        [InlineData(Method.Get, Rel.NextPage, 1)]
        public async Task B1_GetReturnsCollection_WithHateoasOutsideLinks(string method, string rel, int number)
        {
            var (get, _) = await GetHateoasCoursesAndStudentIdAsync();

            Assert.Equal(2, get.Links.Count);
            Assert.Equal(method, get.Links[number].Method);
            Assert.Equal(rel, get.Links[number].Rel);
        }

        [Theory]
        [AutoMoqData]
        public async Task B2_PostReturnsCreated(CourseForCreationDto dto)
        {
            var (post, _) = await PostCourseAsync(dto);
            Assert.Equal(HttpStatusCode.Created, post.StatusCode);
        }

        [Theory]
        [AutoMoqData]
        public async Task A2_PostReturnsNotFound_PostCourseToStudentThatNotExists(CourseForCreationDto dto)
        {
            var post = await PostAsync(Guid.NewGuid(), dto);
            Assert.Equal(HttpStatusCode.NotFound, post.StatusCode);
        }

        [Theory]
        [AutoMoqData]
        public async Task A4_PutReturnsCreated_NewCourse(CourseForUpdateDto dto)
        {
            var put = await PutAsync(await GetFirstIEnumerableStudentId(), Guid.NewGuid(), dto);
            Assert.Equal(HttpStatusCode.Created, put.StatusCode);
        }

        [Theory]
        [AutoMoqData]
        public async Task C4_PutReturnsNoContent_ExistingCourse(CourseForUpdateDto courseForCreationDto)
        {
            var (post, studentId) = await PostCourseAsync<CourseDto>(courseForCreationDto);
            var put = await PutAsync(studentId, post.Id, courseForCreationDto);
            Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);
        }

        [Theory]
        [AutoMoqData]
        public async Task C4_PutReturnsUnprocessableEntity_PropertiesEqual(CourseForUpdateDto courseForCreationDto)
        {
            courseForCreationDto.Description = "Dummy";
            courseForCreationDto.Title = "Dummy";
            var (post, studentId) = await PostCourseAsync<CourseDto>(courseForCreationDto);
            var put = await PutAsync(studentId, post.Id, courseForCreationDto);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, put.StatusCode);
        }

        [Theory]
        [AutoMoqData]
        public async Task A4_PutReturnsNotFound_StudentNotExists(CourseForUpdateDto dto)
        {
            var put = await PutAsync(Guid.NewGuid(), Guid.NewGuid(), dto);
            Assert.Equal(HttpStatusCode.NotFound, put.StatusCode);
        }

        [Theory]
        [AutoMoqData]
        public async Task C5_DeleteReturnsNoContent_CourseDeleted(CourseForCreationDto courseForCreationDto)
        {
            var (post, studentId) = await PostCourseAsync<CourseDto>(courseForCreationDto);
            var delete = await DeleteAsync(studentId, post.Id);
            Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        }

        [Theory]
        [AutoMoqData]
        public async Task C2_PostReturnsSameEntity_CourseCreated(CourseForCreationDto courseForCreationDto)
        {
            var (post, studentId) = await PostCourseAsync<CourseDto>(courseForCreationDto);
            var get = DeserializeObject<CourseDto>(await (await GetCourseAsync(studentId, post.Id))
                .Content.ReadAsStringAsync());

            Assert.Equal(post.Id, get.Id);
        }

        [Theory]
        [AutoMoqData]
        public async Task C1_GetPaginationNextPageNotNull(CourseForCreationDto dto)
        {
            var (_, studentId) = await PostCourseAsync(dto);
            var get = await GetCoursesAsync(studentId);
            var header = get.Headers.GetValues(Header.XPagination);
            var pagination = DeserializeObject<HateoasPagination>(header.First());
            Assert.NotNull(pagination.NextPage);
        }

        [Theory]
        [AutoMoqData]
        public async Task C3_PatchReturnsBadRequest_PatchDocIsNull(CourseForCreationDto courseForCreationDto)
        {
            var (_, studentId) = await PostCourseAsync(courseForCreationDto);
            var patch = await PatchAsync(studentId, Guid.NewGuid(), null);
            Assert.Equal(HttpStatusCode.BadRequest, patch.StatusCode);
        }

        [Theory]
        [AutoMoqData]
        public async Task C3_PatchReturnsUnprocessableEntity_PropertiesEqualExistingCourse(
            CourseForCreationDto courseForCreationDto)
        {
            var (post, studentId) = await PostCourseAsync<CourseDto>(courseForCreationDto);

            var patchDoc = new JsonPatchDocument<CourseForUpdateDto>();
            patchDoc.Replace(x => x.Description, "Dummy");
            patchDoc.Replace(x => x.Title, "Dummy");

            var patch = await PatchAsync(studentId, post.Id, patchDoc);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, patch.StatusCode);
        }

        [Theory]
        [AutoMoqData]
        public async Task C3_PatchReturnsUnprocessableEntity_PropertiesEqualsNewCourse(
            CourseForCreationDto courseForCreationDto)
        {
            var (_, studentId) = await PostCourseAsync(courseForCreationDto);
            var patchDoc = new JsonPatchDocument<CourseForUpdateDto>();
            patchDoc.Replace(x => x.Description, "Dummy");
            patchDoc.Replace(x => x.Title, "Dummy");

            var patch = await PatchAsync(studentId, Guid.NewGuid(), patchDoc);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, patch.StatusCode);
        }

        [Theory]
        [AutoMoqData]
        public async Task C3_PatchReturnsCreated_NewCourse(CourseForCreationDto courseForCreationDto)
        {
            var (_, studentId) = await PostCourseAsync(courseForCreationDto);
            var patchDoc = new JsonPatchDocument<CourseForUpdateDto>();
            patchDoc.Replace(x => x.Description, "Dummy");

            var patch = await PatchAsync(studentId, Guid.NewGuid(), patchDoc);
            Assert.Equal(HttpStatusCode.Created, patch.StatusCode);
        }

        [Theory]
        [AutoMoqData]
        public async Task C3_PatchReturnsNoContent_ValidPatch(CourseForCreationDto courseForCreationDto)
        {
            var (post, studentId) = await PostCourseAsync<CourseDto>(courseForCreationDto);
            var patchDoc = new JsonPatchDocument<CourseForUpdateDto>();
            patchDoc.Replace(x => x.Description, "Dummy");

            var patch = await PatchAsync(studentId, post.Id, patchDoc);
            Assert.Equal(HttpStatusCode.NoContent, patch.StatusCode);
        }

        [Theory]
        [AutoMoqData]
        public async Task B2_PostReturnsBadRequest_UnprocessableEntity(CourseForCreationDto courseForCreationDto)
        {
            courseForCreationDto.Description = "Dummy";
            courseForCreationDto.Title = "Dummy";
            var (post, _) = await PostCourseAsync(courseForCreationDto);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, post.StatusCode);
        }

        [Fact]
        public async Task A1_GetReturnsNotFound_StudentNotExists()
        {
            var get = await GetCoursesAsync(Guid.NewGuid());
            Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
        }

        [Fact]
        public async Task A2_PostReturnsBadRequest_CourseIsNull()
        {
            var (post, _) = await PostCourseAsync(null);
            Assert.Equal(HttpStatusCode.BadRequest, post.StatusCode);
        }

        [Fact]
        public async Task A3_PatchReturnsNotFound_StudentNotExists()
        {
            var patch = await PatchAsync(Guid.NewGuid(), Guid.NewGuid(), new JsonPatchDocument<CourseForUpdateDto>());
            Assert.Equal(HttpStatusCode.NotFound, patch.StatusCode);
        }

        [Fact]
        public async Task A5_DeleteReturnsNotFound_StudentNotExists()
        {
            var delete = await DeleteAsync(Guid.NewGuid(), Guid.NewGuid());
            Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);
        }

        [Fact]
        public async Task B1_GetReturnsCollection_WithoutHateoas()
        {
            var (courses, _) = await GetIEnumerableCoursesAndStudentIdAsync();
            Assert.Equal(10, courses.Count());
        }

        [Fact]
        public async Task B1_GetReturnsNotFound_CourseNotExist()
        {
            var get = await GetCourseAsync(await GetFirstIEnumerableStudentId(), Guid.NewGuid());

            Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
        }

        [Fact]
        public async Task B1_GetReturnsNotNullHref_CoursesWithHateoas()
        {
            var (get, _) = await GetHateoasCoursesAndStudentIdAsync();

            Assert.All(get.Values.First().Links, x => Assert.NotNull(x.Href));
            Assert.All(get.Links, x => Assert.NotNull(x.Href));
        }

        [Fact]
        public async Task B1_GetReturnsOK_GetCourses()
        {
            var get = await GetCoursesAsync(await GetFirstIEnumerableStudentId());
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        }

        [Fact]
        public async Task B1_GetReturnsOK_GetCoursesWithHateoas()
        {
            var getCourse = await GetCoursesAsync(await GetFirstHateoasStudentId());
            Assert.Equal(HttpStatusCode.OK, getCourse.StatusCode);
        }

        [Fact]
        public async Task B4_PutReturnsBadRequest_CourseIsNull()
        {
            var put = await PutAsync(await GetFirstIEnumerableStudentId(), Guid.NewGuid(), null);
            Assert.Equal(HttpStatusCode.BadRequest, put.StatusCode);
        }

        [Fact]
        public async Task B5_DeleteReturnsNotFound_CourseNotExists()
        {
            var delete = await DeleteAsync(await GetFirstIEnumerableStudentId(), Guid.NewGuid());
            Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);
        }

        [Fact]
        public async Task C1_GetReturnsOK_CourseExistsInStudent()
        {
            var (get, studentId) = await GetIEnumerableCoursesAndStudentIdAsync();

            var getCourse = await GetCourseAsync(studentId, get.First().Id);
            Assert.Equal(HttpStatusCode.OK, getCourse.StatusCode);
        }

        [Fact]
        public async Task C1_GetReturnsOK_CourseShapedWithHateoas()
        {
            var (get, studentId) = await GetHateoasCoursesAndStudentIdAsync();

            var getCourse = await GetCourseAsync(studentId, get.Values.First().Id);
            Assert.Equal(HttpStatusCode.OK, getCourse.StatusCode);
        }
    }

    public partial class ApiCoursesControllerShould
    {
        private Task<HttpResponseMessage> GetCoursesAsync(Guid studentId, string fields = "") =>
            _client.GetAsync(Api.Courses.ToApiUrl(studentId, fields));

        private Task<HttpResponseMessage> GetCourseAsync(Guid studentId, Guid courseId, string fields = "") =>
            _client.GetAsync(Api.Course.ToApiUrl(studentId, courseId, fields));

        private async Task<Guid> GetFirstIEnumerableStudentId() =>
            (await GetPeopleAsync<IEnumerable<StudentDto>>()).First().Id;

        private async Task<Guid> GetFirstHateoasStudentId()
        {
            _client.DefaultRequestHeaders.TryAddWithoutValidation(Header.Accept, MediaType.OutputFormatterJson);
            return (await GetPeopleAsync<HateoasDto<StudentValuesDto>>()).Values.First().Id;
        }

        private async Task<TStudent> GetPeopleAsync<TStudent>() =>
            DeserializeObject<TStudent>(await (await _client.GetAsync(Api.People))
                .Content.ReadAsStringAsync());

        private async Task<(HttpResponseMessage Response, Guid StudentId)> PostCourseAsync(
            CourseForManipulationDto courseDto)
        {
            var studentId = await GetFirstIEnumerableStudentId();
            return (await PostAsync(studentId, courseDto), studentId);
        }

        private async Task<(T course, Guid StudentId)> PostCourseAsync<T>(
            CourseForManipulationDto courseDto)
        {
            var studentId = await GetFirstIEnumerableStudentId();
            return (DeserializeObject<T>(await (await PostAsync(studentId, courseDto)).Content.ReadAsStringAsync()),
                studentId);
        }

        private Task<HttpResponseMessage> PostAsync(Guid studentId, CourseForManipulationDto dto) =>
            _client.PostAsync(Api.Courses.ToApiUrl(studentId), dto.Content(MediaType.InputFormatterJson));


        private Task<HttpResponseMessage> PutAsync(Guid studentId, Guid courseId, CourseForManipulationDto dto) =>
            _client.PutAsync(Api.Course.ToApiUrl(studentId, courseId), dto.Content(MediaType.InputFormatterJson));


        private Task<HttpResponseMessage> PatchAsync(Guid studentId, Guid courseId,
            JsonPatchDocument<CourseForUpdateDto> patchDoc) =>
            _client.PatchAsync(Api.Course.ToApiUrl(studentId, courseId),
                patchDoc.Content(MediaType.PatchFormatterJson));


        private Task<HttpResponseMessage> DeleteAsync(Guid studentId, Guid courseId) =>
            _client.DeleteAsync(Api.Course.ToApiUrl(studentId, courseId));

        private async Task<(HateoasDto<CourseValuesDto> Courses, Guid StudentId)> GetHateoasCoursesAndStudentIdAsync()
        {
            var studentId = await GetFirstHateoasStudentId();
            return (
                DeserializeObject<HateoasDto<CourseValuesDto>>(await (await GetCoursesAsync(studentId))
                    .Content.ReadAsStringAsync()), studentId);
        }

        private async Task<(IEnumerable<CourseDto> Courses, Guid StudentId)> GetIEnumerableCoursesAndStudentIdAsync()
        {
            var studentId = await GetFirstIEnumerableStudentId();
            return (
                DeserializeObject<IEnumerable<CourseDto>>(await (await GetCoursesAsync(studentId)).Content
                    .ReadAsStringAsync()), studentId);
        }

        private class StudentValuesDto : StudentDto
        {
            public List<LinkDto> Links { get; } = new List<LinkDto>();
        }

        private class CourseValuesDto : StudentDto
        {
            public List<LinkDto> Links { get; } = new List<LinkDto>();
        }

        private class HateoasDto<T>
        {
            public List<T> Values { get; } = new List<T>();
            public List<LinkDto> Links { get; } = new List<LinkDto>();
        }
    }
}