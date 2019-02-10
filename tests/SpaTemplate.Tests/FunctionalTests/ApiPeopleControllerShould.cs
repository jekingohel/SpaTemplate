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
	public partial class ApiPeopleControllerShould : IClassFixture<CustomWebApplicationFactory<Startup>>
	{
		public ApiPeopleControllerShould(CustomWebApplicationFactory<Startup> factory) =>
			_client = factory.CreateClientWithDefaultRequestHeaders();

		private readonly HttpClient _client;

		[Theory]
		[InlineAutoMoqData("?fields=whichDoesntExist", HttpStatusCode.BadRequest)]
		[InlineAutoMoqData("", HttpStatusCode.OK)]
		public async Task GetReturnsCorrectStatusCode(string field, HttpStatusCode statusCode,
			PersonForCreationDto personForCreationDto)
		{
			var personDto = await GetPersonAsync(personForCreationDto);

			var get = await GetAsync($"{personDto.Id}{field}");
			Assert.Equal(statusCode, get.StatusCode);
		}

		[Theory]
		[InlineData("?orderBy=unknown")]
		[InlineData("?fields=dummy")]
		public async Task GetReturnsBadRequest_FieldAndMappingNotExist(string field)
		{
			var get = await GetAsync(field);
			Assert.Equal(HttpStatusCode.BadRequest, get.StatusCode);
		}

		[Theory]
		[AutoMoqData]
		public async Task DeleteRemoveEntity(PersonForCreationDto personForCreationDto)
		{
			var personDto = await GetPersonAsync(personForCreationDto);

			var delete = await DeleteAsync(personDto.Id);
			Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

			var get = await GetAsync(personDto.Id.ToString());
			Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
		}

		[Fact]
		public async Task DeleteReturnsNotFoundEntity()
		{
			var delete = await DeleteAsync(Guid.NewGuid());
			Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);
		}

		[Theory]
		[AutoMoqData]
		public async Task PatchReturnsUnprocessableEntity_EqualFields(PersonForUpdateDto personForUpdateDto)
		{
			var personDto = await GetPersonAsync(personForUpdateDto);

			var patchDoc = new JsonPatchDocument<PersonDto>();
			patchDoc.Replace(x => x.Name, personDto.Surname);

			var patch = await PatchAsync(personDto.Id, patchDoc);
			Assert.Equal(HttpStatusCode.UnprocessableEntity, patch.StatusCode);
		}

		[Theory]
		[AutoMoqData]
		public async Task PostReturnsCreated(PersonForUpdateDto personForUpdateDto)
		{
			var post = await PostAsync(personForUpdateDto);
			Assert.Equal(HttpStatusCode.Created, post.StatusCode);
		}

		[Theory]
		[AutoMoqData]
		public async Task PatchReturnsNoContent_ValidPatch(PersonForUpdateDto personForUpdateDto)
		{
			var personDto = await GetPersonAsync(personForUpdateDto);

			var patchDoc = new JsonPatchDocument<PersonDto>();
			patchDoc.Replace(x => x.Surname, "Dummy");

			var patch = await PatchAsync(personDto.Id, patchDoc);
			Assert.Equal(HttpStatusCode.NoContent, patch.StatusCode);
		}

		[Theory]
		[InlineData(Method.Get, Rel.Self, 0)]
		[InlineData(Method.Post, Rel.CreatePerson, 1)]
		[InlineData(Method.Patch, Rel.PatchPerson, 2)]
		[InlineData(Method.Delete, Rel.DeletePerson, 3)]
		public async Task GetReturnsCollection_ValidMethodAndRelInsideLinks(string method, string rel, int number)
		{
			_client.DefaultRequestHeaders.Add(Header.Accept, MediaType.OutputFormatterJson);
			var hateoasDto = await GetCollectionAsync<HateoasDto>();

			Assert.Equal(10, hateoasDto.Values.Count);
			Assert.Equal(4, hateoasDto.Values[0].Links.Count);

			var linkDto = hateoasDto.Values[0].Links[number];
			Assert.Equal(method, linkDto.Method);
			Assert.Equal(rel, linkDto.Rel);
		}

		[Theory]
		[InlineData(Method.Get, Rel.Self, 0)]
		[InlineData(Method.Get, Rel.NextPage, 1)]
		public async Task GetReturnsCollection_ValidMethodAndRelOutsideLinks(string method, string rel, int number)
		{
			_client.DefaultRequestHeaders.Add(Header.Accept, MediaType.OutputFormatterJson);
			var hateoasDto = await GetCollectionAsync<HateoasDto>();

			Assert.Equal(2, hateoasDto.Links.Count);

			var linkDto = hateoasDto.Links[number];
			Assert.Equal(method, linkDto.Method);
			Assert.Equal(rel, linkDto.Rel);
		}

		[Theory]
		[InlineData("", Rel.NextPage)]
		[InlineData("?pageNumber=2", Rel.PreviousPage)]
		public async Task ReturnsNotNullHref(string fields, string rel)
		{
			_client.DefaultRequestHeaders.Add(Header.Accept, MediaType.OutputFormatterJson);

			var hateoasDto = await GetCollectionAsync<HateoasDto>(fields);
			Assert.All(hateoasDto.Values[0].Links, x => Assert.NotNull(x.Href));
			Assert.All(hateoasDto.Links, x => Assert.NotNull(x.Href));
			Assert.Contains(hateoasDto.Links, x => x.Rel == rel);
		}

		[Fact]
		public async Task GetPaginationNextPageNotNull()
		{
			var people = await GetAsync();
			var header = people.Headers.GetValues(Header.XPagination);
			var pagination = JsonConvert.DeserializeObject<HateoasPagination>(header.First());
			Assert.NotNull(pagination.NextPage);
		}

		[Fact]
		public async Task GetPaginationPreviousPageNotNull()
		{
			var people = await GetAsync("?pageNumber=2");
			var header = people.Headers.GetValues(Header.XPagination);
			var pagination = JsonConvert.DeserializeObject<HateoasPagination>(header.First());
			Assert.NotNull(pagination.PreviousPage);
		}

		[Fact]
		public async Task GetReturnsCollection()
		{
			var people = await GetCollectionAsync<IEnumerable<PersonDto>>();
			Assert.Equal(10, people.Count());
		}

		[Fact]
		public async Task GetReturnsNotFound_EntityNotExist()
		{
			var get = await GetAsync(Guid.NewGuid().ToString());
			Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
		}

		[Fact]
		public async Task PatchReturnsNotFound_EntityForPatchNotExist()
		{
			var patch = await PatchAsync(Guid.NewGuid(), new JsonPatchDocument<PersonDto>());
			Assert.Equal(HttpStatusCode.NotFound, patch.StatusCode);
		}

		[Fact]
		public async Task PostReturnsBadRequest_RequiredPropertyWithResponse()
		{
			var post = await PostAsync(new PersonForCreationDto());
			Assert.Equal(HttpStatusCode.BadRequest, post.StatusCode);

			var stringResponse = await post.Content.ReadAsStringAsync();
			var result = JsonConvert.DeserializeObject<DummyName>(stringResponse);
			Assert.Equal("The Name field is required.", result.Name.First());
		}

		[Fact]
		public async Task ReturnsOk_ValidGet()
		{
			var get = await GetAsync();
			Assert.Equal(HttpStatusCode.OK, get.StatusCode);
		}
	}

	public partial class ApiPeopleControllerShould
	{
		private Task<HttpResponseMessage> DeleteAsync(Guid personId) =>
			_client.DeleteAsync($"{Route.PeopleApi}/{personId}");

		private Task<HttpResponseMessage> GetAsync(string additionalField = null) =>
			_client.GetAsync($"{Route.PeopleApi}/{additionalField}");

		private Task<HttpResponseMessage> PostAsync(PersonForManipulationDto personDto) =>
			_client.PostAsync(Route.PeopleApi, Content(personDto, MediaType.InputFormatterJson));

		private Task<HttpResponseMessage> PatchAsync(Guid personId, JsonPatchDocument<PersonDto> patchDoc) =>
			_client.PatchAsync($"{Route.PeopleApi}/{personId}", Content(patchDoc, MediaType.PatchFormatterJson));

		private static StringContent Content(object obj, string mediaType) => new StringContent(
			JsonConvert.SerializeObject(obj),
			Encoding.UTF8,
			mediaType);

		private async Task<PersonDto> GetPersonAsync(PersonForManipulationDto person) =>
			JsonConvert.DeserializeObject<PersonDto>(await (await PostAsync(person)).Content.ReadAsStringAsync());

		private async Task<T> GetCollectionAsync<T>(string fields = null) =>
			JsonConvert.DeserializeObject<T>(await (await GetAsync(fields)).Content.ReadAsStringAsync());

		private class PersonValuesDto : PersonDto
		{
			public List<LinkDto> Links { get; } = new List<LinkDto>();
		}

		private class HateoasDto
		{
			public List<PersonValuesDto> Values { get; } = new List<PersonValuesDto>();
			public List<LinkDto> Links { get; } = new List<LinkDto>();
		}

		private class DummyName
		{
			public List<string> Name { get; } = new List<string>();
		}
	}
}