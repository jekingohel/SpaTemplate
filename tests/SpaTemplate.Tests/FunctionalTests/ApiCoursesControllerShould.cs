using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;
using SpaTemplate.Core;
using SpaTemplate.Tests.Helpers;
using SpaTemplate.Web.Core;
using Xunit;

namespace SpaTemplate.Tests.FunctionalTests
{
	public partial class ApiCoursesControllerShould : IClassFixture<CustomWebApplicationFactory<Startup>>
	{
		public ApiCoursesControllerShould(CustomWebApplicationFactory<Startup> factory) =>
			_client = factory.CreateClientWithDefaultRequestHeaders();

		private readonly HttpClient _client;

		[Theory]
		[InlineData("?orderBy=unknown")]
		[InlineData("?fields=dummy")]
		public async Task GetReturnsBadRequest_FieldAndMappingNotExist(string field)
		{
			var people = JsonConvert.DeserializeObject<IEnumerable<PersonDto>>(await GetPeopleStringAsync());
			var get = await GetCoursesAsync(people.First().Id, field);

			Assert.Equal(HttpStatusCode.BadRequest, get.StatusCode);
		}

		[Theory]
		[InlineData(Method.Get, Rel.Self, 0)]
		[InlineData(Method.Post, Rel.CreateCourseForPerson, 1)]
		[InlineData(Method.Patch, Rel.PartiallyUpdateCourse, 2)]
		[InlineData(Method.Put, Rel.UpdateCourse, 3)]
		[InlineData(Method.Delete, Rel.DeleteCourse, 4)]
		public async Task GetReturnsCollection_WithHateoasInsideLinks(string method, string rel, int number)
		{
			_client.DefaultRequestHeaders.Add(Header.Accept, MediaType.OutputFormatterJson);
			var (courses, _) = await GetCoursesAndPersonIdAsync<HateoasDto<PersonValuesDto>, HateoasDto<CourseValuesDto>>();

			Assert.Equal(10, courses.Values.Count);
			Assert.Equal(5, courses.Values.First().Links.Count);

			Assert.Equal(method, courses.Values.First().Links[number].Method);
			Assert.Equal(rel, courses.Values.First().Links[number].Rel);
		}


		[Theory]
		[InlineData(Method.Get, Rel.Self, 0)]
		[InlineData(Method.Get, Rel.NextPage, 1)]
		public async Task GetReturnsCollection_WithHateoasOutsideLinks(string method, string rel, int number)
		{
			_client.DefaultRequestHeaders.Add(Header.Accept, MediaType.OutputFormatterJson);

			var (courses, _) = await GetCoursesAndPersonIdAsync<HateoasDto<PersonValuesDto>, HateoasDto<CourseValuesDto>>();

			Assert.Equal(2, courses.Links.Count);
			Assert.Equal(method, courses.Links[number].Method);
			Assert.Equal(rel, courses.Links[number].Rel);
		}

		[Theory]
		[AutoMoqData]
		public async Task PostReturnsCreated(CourseForCreationDto dto)
        {
            var (response, _) = await PostCourseAsync(dto);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

		[Theory]
		[AutoMoqData]
		public async Task PostReturnsNotFound_PostCourseToPersonThatNotExists(CourseForCreationDto dto)
		{
			var post = await PostAsync(Guid.NewGuid(), dto);
			Assert.Equal(HttpStatusCode.NotFound, post.StatusCode);
		}

		[Fact]
		public async Task PostReturnsBadRequest_CourseIsNull()
		{
			var people = JsonConvert.DeserializeObject<IEnumerable<PersonDto>>(await GetPeopleStringAsync());
			var post = await PostAsync(people.First().Id, null);
			Assert.Equal(HttpStatusCode.BadRequest, post.StatusCode);
		}

		[Fact]
		public async Task PutReturnsBadRequest_CourseIsNull()
		{
			var people = JsonConvert.DeserializeObject<IEnumerable<PersonDto>>(await GetPeopleStringAsync());
			var put = await PutAsync(people.First().Id, Guid.NewGuid(), null);
			Assert.Equal(HttpStatusCode.BadRequest, put.StatusCode);
		}

		[Theory]
		[AutoMoqData]
		public async Task PutReturnsCreated_NewCourse(CourseForUpdateDto dto)
		{
			var people = JsonConvert.DeserializeObject<IEnumerable<PersonDto>>(await GetPeopleStringAsync());
			var put = await PutAsync(people.First().Id, Guid.NewGuid(), dto);
			Assert.Equal(HttpStatusCode.Created, put.StatusCode);
		}

		[Theory]
		[AutoMoqData]
		public async Task PutReturnsNoContent_ExistingCourse(CourseForUpdateDto dto)
		{
			var (response, personId) = await PostCourseAsync(dto);
			var course = JsonConvert.DeserializeObject<CourseDto>(await response.Content.ReadAsStringAsync());
			var put = await PutAsync(personId, course.Id, dto);
			Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);
		}

		[Theory]
		[AutoMoqData]
		public async Task PutReturnsUnprocessableEntity_PropertiesEqual(CourseForUpdateDto dto)
		{
			dto.Description = "Dummy";
			dto.Title = "Dummy";
			var (response, personId) = await PostCourseAsync(dto);
			var course = JsonConvert.DeserializeObject<CourseDto>(await response.Content.ReadAsStringAsync());
			var put = await PutAsync(personId, course.Id, dto);
			Assert.Equal(HttpStatusCode.UnprocessableEntity, put.StatusCode);
		}

		[Theory]
		[AutoMoqData]
		public async Task PutReturnsNotFound_PersonNotExists(CourseForUpdateDto dto)
		{
			var put = await PutAsync(Guid.NewGuid(), Guid.NewGuid(), dto);
			Assert.Equal(HttpStatusCode.NotFound, put.StatusCode);
		}

		[Theory]
		[AutoMoqData]
		public async Task DeleteReturnsNoContent_CourseDeleted(CourseForCreationDto dto)
		{
			var (response, personId) = await PostCourseAsync(dto);
			var courseDto = JsonConvert.DeserializeObject<CourseDto>(await response.Content.ReadAsStringAsync());

			var delete = await DeleteCourseAsync(personId, courseDto.Id.ToString());
			Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
		}

		[Fact]
		public async Task DeleteReturnsNotFound_CourseNotExists()
		{
			var people = JsonConvert.DeserializeObject<IEnumerable<PersonDto>>(await GetPeopleStringAsync());
			var delete = await DeleteCourseAsync(people.First().Id, Guid.NewGuid().ToString());
			Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);
		}

		[Theory]
		[AutoMoqData]
		public async Task PostReturnsSameEntity_CourseCreated(CourseForCreationDto dto)
		{
			var (response, personId) = await PostCourseAsync(dto);
			var courseDto = JsonConvert.DeserializeObject<CourseDto>(await response.Content.ReadAsStringAsync());
			var get = await GetCoursesAsync(personId, courseDto.Id.ToString());
			var course = JsonConvert.DeserializeObject<CourseDto>(await get.Content.ReadAsStringAsync());

			Assert.Equal(courseDto.Id, course.Id);
		}

		[Theory]
		[AutoMoqData]
		public async Task GetPaginationNextPageNotNull(CourseForCreationDto dto)
		{
			var (_, personId) = await PostCourseAsync(dto);
			var get = await GetCoursesAsync(personId);
			var header = get.Headers.GetValues(Header.XPagination);
			var pagination = JsonConvert.DeserializeObject<HateoasPagination>(header.First());
			Assert.NotNull(pagination.NextPage);
		}

		[Theory]
		[InlineAutoMoqData("")]
		[InlineAutoMoqData("53782262-e7ad-4d1e-aa2d-b7605f6a38df")]
		public async Task GetReturnsNotFound_PersonNotExists(string field)
		{
			var get = await GetCoursesAsync(Guid.NewGuid(), field);
			Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
		}

		[Theory]
		[AutoMoqData]
		public async Task PatchReturnsBadRequest_PatchDocIsNull(CourseForCreationDto courseForCreationDto)
		{
			var (_, personId) = await PostCourseAsync(courseForCreationDto);
			var patch = await PatchAsync(personId, Guid.NewGuid(), null);
			Assert.Equal(HttpStatusCode.BadRequest, patch.StatusCode);
		}

		[Fact]
		public async Task PatchReturnsNotFound_PersonNotExists()
		{
			var patch = await PatchAsync(Guid.NewGuid(), Guid.NewGuid(), new JsonPatchDocument<CourseForUpdateDto>());
			Assert.Equal(HttpStatusCode.NotFound, patch.StatusCode);
		}

		[Theory]
		[AutoMoqData]
		public async Task PatchReturnsUnprocessableEntity_PropertiesEqualExistingCourse(
			CourseForCreationDto courseForCreationDto)
		{
			var (response, personId) = await PostCourseAsync(courseForCreationDto);
			var course = JsonConvert.DeserializeObject<CourseDto>(await response.Content.ReadAsStringAsync());
			var patchDoc = new JsonPatchDocument<CourseForUpdateDto>();
			patchDoc.Replace(x => x.Description, "Dummy");
			patchDoc.Replace(x => x.Title, "Dummy");

			var patch = await PatchAsync(personId, course.Id, patchDoc);
			Assert.Equal(HttpStatusCode.UnprocessableEntity, patch.StatusCode);
		}

		[Theory]
		[AutoMoqData]
		public async Task PatchReturnsUnprocessableEntity_PropertiesEqualsNewCourse(
			CourseForCreationDto courseForCreationDto)
		{
			var (_, personId) = await PostCourseAsync(courseForCreationDto);
			var patchDoc = new JsonPatchDocument<CourseForUpdateDto>();
			patchDoc.Replace(x => x.Description, "Dummy");
			patchDoc.Replace(x => x.Title, "Dummy");

			var patch = await PatchAsync(personId, Guid.NewGuid(), patchDoc);
			Assert.Equal(HttpStatusCode.UnprocessableEntity, patch.StatusCode);
		}

		[Theory]
		[AutoMoqData]
		public async Task PatchReturnsCreated_NewCourse(CourseForCreationDto courseForCreationDto)
		{
			var (_, personId) = await PostCourseAsync(courseForCreationDto);
			var patchDoc = new JsonPatchDocument<CourseForUpdateDto>();
			patchDoc.Replace(x => x.Description, "Dummy");

			var patch = await PatchAsync(personId, Guid.NewGuid(), patchDoc);
			Assert.Equal(HttpStatusCode.Created, patch.StatusCode);
		}

		[Theory]
		[AutoMoqData]
		public async Task PatchReturnsNoContent_ValidPatch(CourseForCreationDto courseForCreationDto)
		{
			var (response, personId) = await PostCourseAsync(courseForCreationDto);
			var course = JsonConvert.DeserializeObject<CourseDto>(await response.Content.ReadAsStringAsync());
			var patchDoc = new JsonPatchDocument<CourseForUpdateDto>();
			patchDoc.Replace(x => x.Description, "Dummy");

			var patch = await PatchAsync(personId, course.Id, patchDoc);
			Assert.Equal(HttpStatusCode.NoContent, patch.StatusCode);
		}

		[Fact]
		public async Task DeleteReturnsNotFound_PersonNotExists()
		{
			var delete = await DeleteCourseAsync(Guid.NewGuid(), Guid.NewGuid().ToString());
			Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);
		}

		[Fact]
		public async Task GetReturnsCollection_WithoutHateoas()
        {
            var (courses, _) = await GetCoursesAndPersonIdAsync<IEnumerable<PersonDto>, IEnumerable<CourseDto>>();
            Assert.Equal(10, courses.Count());
        }

		[Fact]
		public async Task GetReturnsNotFound_CourseNotExist()
		{
			var people = JsonConvert.DeserializeObject<IEnumerable<PersonDto>>(await GetPeopleStringAsync());
			var get = await GetCoursesAsync(people.First().Id, Guid.NewGuid().ToString());

			Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
		}

		[Fact]
		public async Task GetReturnsNotNullHref_CoursesWithHateoas()
		{
			_client.DefaultRequestHeaders.Add(Header.Accept, MediaType.OutputFormatterJson);

			var (courses, _) = await GetCoursesAndPersonIdAsync<HateoasDto<PersonValuesDto>, HateoasDto<CourseValuesDto>>();

			Assert.All(courses.Values.First().Links, x => Assert.NotNull(x.Href));
			Assert.All(courses.Links, x => Assert.NotNull(x.Href));
		}

		[Fact]
		public async Task GetReturnsOK_CourseExistsInPerson()
		{
			var (courses, personId) = await GetCoursesAndPersonIdAsync<IEnumerable<PersonDto>, IEnumerable<CourseDto>>();

			var get = await GetCoursesAsync(personId, courses.First().Id.ToString());
			Assert.Equal(HttpStatusCode.OK, get.StatusCode);
		}

		[Fact]
		public async Task GetReturnsOK_CourseShapedWithHateoas()
		{
			_client.DefaultRequestHeaders.Add(Header.Accept, MediaType.OutputFormatterJson);

			var (hateoasDto, personId) = await GetCoursesAndPersonIdAsync<HateoasDto<PersonValuesDto>, HateoasDto<CourseValuesDto>>();

			var courses = await GetCoursesAsync(personId, hateoasDto.Values.First().Id.ToString());
			Assert.Equal(HttpStatusCode.OK, courses.StatusCode);
		}

		[Fact]
		public async Task GetReturnsOK_GetCourses()
		{
			var person = JsonConvert.DeserializeObject<IEnumerable<PersonDto>>(await GetPeopleStringAsync());
			var get = await GetCoursesAsync(person.First().Id);
			Assert.Equal(HttpStatusCode.OK, get.StatusCode);
		}

		[Fact]
		public async Task GetReturnsOK_GetCoursesWithHateoas()
		{
			_client.DefaultRequestHeaders.Add(Header.Accept, MediaType.OutputFormatterJson);

			var person = JsonConvert.DeserializeObject<HateoasDto<PersonValuesDto>>(await GetPeopleStringAsync());
			var get = await GetCoursesAsync(person.Values.First().Id);
			Assert.Equal(HttpStatusCode.OK, get.StatusCode);
		}

		[Fact]
		public async Task PostReturnsBadRequest_UnprocessableEntity()
        {
            var (response, _) = await PostCourseAsync(new CourseForCreationDto());
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }
	}

	public partial class ApiCoursesControllerShould
	{
		private Task<HttpResponseMessage> DeleteCourseAsync(Guid personId, string field) =>
			_client.DeleteAsync($"{PersonLink(personId)}{field}");

		private async Task<(HttpResponseMessage Response, Guid PersonId)> PostCourseAsync(
			CourseForManipulationDto courseDto)
		{
			var people = JsonConvert.DeserializeObject<IEnumerable<PersonDto>>(await GetPeopleStringAsync());
			var personId = people.First().Id;
			return (await _client.PostAsync(PersonLink(personId), Content(courseDto, MediaType.InputFormatterJson)), personId);
		}

		private async Task<string> GetCoursesStringAsync(Guid personId) =>
			await (await GetCoursesAsync(personId)).Content.ReadAsStringAsync();

		private Task<HttpResponseMessage> GetCoursesAsync(Guid personId, string additionalField = null) =>
			_client.GetAsync($"{PersonLink(personId)}{additionalField}");

		private async Task<string> GetPeopleStringAsync() =>
			await (await _client.GetAsync(Route.PeopleApi)).Content.ReadAsStringAsync();

		private static string PersonLink(Guid personId) => $"{Route.PeopleApi}/{personId}/courses/";

		private static string CourseLink(Guid personId, Guid id) => $"{PersonLink(personId)}{id}";

		private static StringContent Content(object courseDto, string mediaType) => new StringContent(
			JsonConvert.SerializeObject(courseDto),
			Encoding.UTF8,
			mediaType);

		private Task<HttpResponseMessage> PostAsync(Guid personId, CourseForManipulationDto dto) =>
			_client.PostAsync(PersonLink(personId), Content(dto, MediaType.InputFormatterJson));

		private Task<HttpResponseMessage> PutAsync(Guid personId, Guid id, CourseForManipulationDto dto) =>
			_client.PutAsync(CourseLink(personId, id), Content(dto, MediaType.InputFormatterJson));

		private Task<HttpResponseMessage> PatchAsync(Guid personId, Guid id,
			JsonPatchDocument<CourseForUpdateDto> patchDoc) =>
			_client.PatchAsync(CourseLink(personId, id), Content(patchDoc, MediaType.PatchFormatterJson));

		private async Task<(TCourse Courses, Guid PersonId)> GetCoursesAndPersonIdAsync<TPerson, TCourse>()
		{
			var people = JsonConvert.DeserializeObject<TPerson>(await GetPeopleStringAsync());
			var personId = Guid.Empty;
			switch (people)
			{
				case IEnumerable<PersonDto> dto:
					personId = dto.First().Id;
					break;
				case HateoasDto<PersonValuesDto> hateoasDto:
					personId = hateoasDto.Values.First().Id;
					break;
			}

			var courses = JsonConvert.DeserializeObject<TCourse>(await GetCoursesStringAsync(personId));
			return (courses, personId);
		}

		private class PersonValuesDto : PersonDto
		{
			public List<LinkDto> Links { get; } = new List<LinkDto>();
		}

		private class CourseValuesDto : PersonDto
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