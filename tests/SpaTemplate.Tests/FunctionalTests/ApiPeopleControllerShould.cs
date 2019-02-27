using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;
using SpaTemplate.Tests.Helpers;
using SpaTemplate.Web.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using SpaTemplate.Core.FacultyContext;
using SpaTemplate.Core.SharedKernel;
using Xunit;

namespace SpaTemplate.Tests.FunctionalTests
{
    /// <inheritdoc />
    /// <summary>
    ///     A - Standard calls to API without combinations(POST + GET, POST + DELETE etc.)
    ///     B - Extended calls that test API in isolation without prepared data in the database to remove hardcoded values.
    ///         Firstly we call POST to prepare object and then exercise another action(ie. DELETE).
    ///     C - If you exercise more actions (ie. 3) then use next letter to distinct number of calls, ie. C = 3 D = 4, E = 5 etc.
    ///     Number defines tests for particular action(ie. GET)
    ///     1 - GET,
    ///     2 - POST,
    ///     3 - PATCH,
    ///     4 - PUT,
    ///     5 - DELETE
    ///
    ///     The functional test needs to be tested in particular order due to domino's effect of API, ie. if POST will fail, others too.
    /// </summary>
    public partial class ApiPeopleControllerShould : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        public ApiPeopleControllerShould(CustomWebApplicationFactory<Startup> factory) =>
            _client = factory.CreateClientWithDefaultRequestHeaders();

        private readonly HttpClient _client;

        [Theory]
        [InlineData("?orderBy=unknown")]
        [InlineData("?fields=dummy")]
        public async Task A1_GetReturnsBadRequest_FieldAndMappingNotExist(string field)
        {
            var get = await GetAsync(field);
            Assert.Equal(HttpStatusCode.BadRequest, get.StatusCode);
        }

        [Fact]
        public async Task A1_GetPaginationNextPageNotNull()
        {
            var get = await GetAsync();
            var header = get.Headers.GetValues(Header.XPagination);
            var pagination = JsonConvert.DeserializeObject<HateoasPagination>(header.First());
            Assert.NotNull(pagination.NextPage);
        }

        [Fact]
        public async Task A1_GetPaginationPreviousPageNotNull()
        {
            var get = await GetAsync("?pageNumber=2");
            var header = get.Headers.GetValues(Header.XPagination);
            var pagination = JsonConvert.DeserializeObject<HateoasPagination>(header.First());
            Assert.NotNull(pagination.PreviousPage);
        }

        [Fact]
        public async Task A1_GetReturnsNotFound_EntityNotExist()
        {
            var get = await GetAsync(Guid.NewGuid());
            Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
        }

        [Fact]
        public async Task A1_GetReturnsOk_ValidGet()
        {
            var get = await GetAsync();
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        }

        [Theory]
        [InlineData(Method.Get, Rel.Self, 0)]
        [InlineData(Method.Get, Rel.NextPage, 1)]
        public async Task A1_GetReturnsCollection_ValidMethodAndRelOutsideLinks(string method, string rel, int number)
        {
            var get = await GetHateoasReadAsStringAsync();

            Assert.Equal(2, get.Links.Count);

            Assert.Equal(method, get.Links[number].Method);
            Assert.Equal(rel, get.Links[number].Rel);
        }

        [Fact]
        public async Task A1_GetReturnsCollection()
        {
            var get = await GetReadAsStringAsync<IEnumerable<StudentDto>>();
            Assert.Equal(10, get.Count());
        }
               
        [Theory]
        [InlineData(Method.Get, Rel.Self, 0)]
        [InlineData(Method.Post, Rel.CreateStudent, 1)]
        [InlineData(Method.Patch, Rel.PatchStudent, 2)]
        [InlineData(Method.Delete, Rel.DeleteStudent, 3)]
        public async Task A1_GetReturnsCollection_ValidMethodAndRelInsideLinks(string method, string rel, int number)
        {
            var get = await GetHateoasReadAsStringAsync();

            Assert.Equal(10, get.Values.Count);
            Assert.Equal(4, get.Values[0].Links.Count);

            var linkDto = get.Values[0].Links[number];
            Assert.Equal(method, linkDto.Method);
            Assert.Equal(rel, linkDto.Rel);
        }
        
        [Theory]
        [InlineData("", Rel.NextPage)]
        [InlineData("?pageNumber=2", Rel.PreviousPage)]
        public async Task A1_GetReturnsNotNullHref(string fields, string rel)
        {
            var get = await GetHateoasReadAsStringAsync(fields);
            Assert.All(get.Values[0].Links, x => Assert.NotNull(x.Href));
            Assert.All(get.Links, x => Assert.NotNull(x.Href));
            Assert.Contains(get.Links, x => x.Rel == rel);
        }

        [Theory]
        [AutoMoqData]
        public async Task A2_PostReturnsCreated(StudentForUpdateDto studentForUpdateDto)
        {
            var post = await PostAsync(studentForUpdateDto);
            Assert.Equal(HttpStatusCode.Created, post.StatusCode);
        }

        [Fact]
        public async Task A3_PatchReturnsNotFound_EntityForPatchNotExist()
        {
            var patch = await PatchAsync(Guid.NewGuid(), new JsonPatchDocument<StudentDto>());
            Assert.Equal(HttpStatusCode.NotFound, patch.StatusCode);
        }

        [Fact]
        public async Task A5_DeleteReturnsNotFoundEntity()
        {
            var delete = await DeleteAsync(Guid.NewGuid());
            Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);
        }

        [Theory]
        [InlineAutoMoqData("?fields=whichDoesntExist", HttpStatusCode.BadRequest)]
        [InlineAutoMoqData("", HttpStatusCode.OK)]
        public async Task B1_GetReturnsCorrectStatusCode(string field, HttpStatusCode statusCode,
            StudentForCreationDto studentForCreationDto)
        {
            var post = await PostReadAsStringAsync<StudentDto>(studentForCreationDto);

            var get = await GetAsync(post.Id, field);
            Assert.Equal(statusCode, get.StatusCode);
        }

        [Theory]
        [AutoMoqData]
        public async Task B3_PatchReturnsUnprocessableEntity_EqualFields(StudentForUpdateDto studentForUpdateDto)
        {
            var post = await PostReadAsStringAsync<StudentDto>(studentForUpdateDto);

            var patchDoc = new JsonPatchDocument<StudentDto>();
            patchDoc.Replace(x => x.Name, post.Surname);

            var patch = await PatchAsync(post.Id, patchDoc);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, patch.StatusCode);
        }

        [Theory]
        [AutoMoqData]
        public async Task B3_PatchReturnsNoContent_ValidPatch(StudentForUpdateDto studentForUpdateDto)
        {
            var post = await PostReadAsStringAsync<StudentDto>(studentForUpdateDto);

            var patchDoc = new JsonPatchDocument<StudentDto>();
            patchDoc.Replace(x => x.Surname, "Dummy");

            var patch = await PatchAsync(post.Id, patchDoc);
            Assert.Equal(HttpStatusCode.NoContent, patch.StatusCode);
        }

        [Theory]
        [AutoMoqData]
        public async Task C5_DeleteRemoveStudent(StudentForCreationDto studentForCreationDto)
        {
            var post = await PostReadAsStringAsync<StudentDto>(studentForCreationDto);

            var delete = await DeleteAsync(post.Id);
            Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

            var get = await GetAsync(post.Id);
            Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
        }
    }

    public partial class ApiPeopleControllerShould
    {
        //<--------------------------------------------------------GET-------------------------------------------------------->
        private Task<HttpResponseMessage> GetAsync(Guid id, string fields = "") =>
            _client.GetAsync(Api.Student.ToApiUrl(id, fields));

        private Task<HttpResponseMessage> GetAsync(string fields = "") =>
            _client.GetAsync(Api.People.ToApiUrl(fields));

        private async Task<T> GetReadAsStringAsync<T>(string fields = "") => 
            JsonConvert.DeserializeObject<T>(await (await GetAsync(fields)).Content.ReadAsStringAsync());

        private async Task<HateoasDto> GetHateoasReadAsStringAsync(string fields = "")
        {
            _client.DefaultRequestHeaders.TryAddWithoutValidation(Header.Accept, MediaType.OutputFormatterJson);
            return await GetReadAsStringAsync<HateoasDto>(fields);
        }
        //<--------------------------------------------------------GET-------------------------------------------------------->


        //<--------------------------------------------------------POST-------------------------------------------------------->
        private Task<HttpResponseMessage> PostAsync(StudentForManipulationDto studentDto) =>
            _client.PostAsync(Api.People, studentDto.Content(MediaType.PatchFormatterJson));

        private async Task<T> PostReadAsStringAsync<T>(StudentForManipulationDto student) =>
            JsonConvert.DeserializeObject<T>(await (await PostAsync(student)).Content.ReadAsStringAsync());
        //<--------------------------------------------------------POST-------------------------------------------------------->


        //<--------------------------------------------------------PATCH-------------------------------------------------------->
        private Task<HttpResponseMessage> PatchAsync(Guid studentId, JsonPatchDocument<StudentDto> patchDoc) =>
            _client.PatchAsync(Api.Student.ToApiUrl(studentId), patchDoc.Content(MediaType.PatchFormatterJson));
        //<--------------------------------------------------------PATCH-------------------------------------------------------->


        //<--------------------------------------------------------DELETE-------------------------------------------------------->
        private Task<HttpResponseMessage> DeleteAsync(Guid studentId) =>
            _client.DeleteAsync(Api.Student.ToApiUrl(studentId));
        //<--------------------------------------------------------DELETE-------------------------------------------------------->


        private class StudentValuesDto : StudentDto
        {
            public List<LinkDto> Links { get; } = new List<LinkDto>();
        }

        private class HateoasDto
        {
            public List<StudentValuesDto> Values { get; } = new List<StudentValuesDto>();
            public List<LinkDto> Links { get; } = new List<LinkDto>();
        }

        private class DummyName
        {
            public List<string> Name { get; } = new List<string>();
        }
    }
}