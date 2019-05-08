// -----------------------------------------------------------------------
// <copyright file="ApiPeopleControllerShould.cs" company="Piotr Xeinaemm Czech">
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
	public sealed partial class ApiPeopleControllerShould : IClassFixture<CustomWebApplicationFactory<Startup>>
	{
		private readonly HttpClient client;

		public ApiPeopleControllerShould(CustomWebApplicationFactory<Startup> factory)
		{
			this.client = factory.CreateClientWithDefaultRequestHeaders();
		}

		[Fact]
		public async Task A1GetPaginationNextPageNotNullAsync()
		{
			var get = await this.GetAsync().ConfigureAwait(false);
			var header = get.Headers.GetValues(Header.XPagination);
			var pagination = JsonConvert.DeserializeObject<HateoasPagination>(header.First());
			Assert.NotNull(pagination.NextPage);
		}

		[Fact]
		public async Task A1GetPaginationPreviousPageNotNullAsync()
		{
			var get = await this.GetAsync("?pageNumber=2").ConfigureAwait(false);
			var header = get.Headers.GetValues(Header.XPagination);
			var pagination = JsonConvert.DeserializeObject<HateoasPagination>(header.First());
			Assert.NotNull(pagination.PreviousPage);
		}

		[Theory]
		[InlineData("?orderBy=unknown")]
		[InlineData("?fields=dummy")]
		public async Task A1GetReturnsBadRequestFieldAndMappingNotExistAsync(string field)
		{
			var get = await this.GetAsync(field).ConfigureAwait(false);
			Assert.Equal(HttpStatusCode.BadRequest, get.StatusCode);
		}

		[Fact]
		public async Task A1GetReturnsCollectionAsync()
		{
			var get = await this.GetReadAsStringAsync<IEnumerable<StudentDto>>().ConfigureAwait(false);
			Assert.Equal(10, get.Count());
		}

		[Theory]
		[InlineData(Method.Get, Rel.Self, 0)]
		[InlineData(Method.Post, SpaTemplateRel.CreateStudent, 1)]
		[InlineData(Method.Patch, SpaTemplateRel.PatchStudent, 2)]
		[InlineData(Method.Delete, SpaTemplateRel.DeleteStudent, 3)]
		public async Task A1GetReturnsCollectionValidMethodAndRelInsideLinksAsync(string method, string rel, int number)
		{
			var get = await this.GetHateoasReadAsStringAsync().ConfigureAwait(false);

			Assert.Equal(10, get.Values.Count);
			Assert.Equal(4, get.Values[0].Links.Count);

			var linkDto = get.Values[0].Links[number];
			Assert.Equal(method, linkDto.Method);
			Assert.Equal(rel, linkDto.Rel);
		}

		[Theory]
		[InlineData(Method.Get, Rel.Self, 0)]
		[InlineData(Method.Get, Rel.NextPage, 1)]
		public async Task A1GetReturnsCollectionValidMethodAndRelOutsideLinksAsync(string method, string rel, int number)
		{
			var get = await this.GetHateoasReadAsStringAsync().ConfigureAwait(false);

			Assert.Equal(2, get.Links.Count);

			Assert.Equal(method, get.Links[number].Method);
			Assert.Equal(rel, get.Links[number].Rel);
		}

		[Fact]
		public async Task A1GetReturnsNotFoundEntityNotExistAsync()
		{
			var get = await this.GetAsync(Guid.NewGuid()).ConfigureAwait(false);
			Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
		}

		[Theory]
		[InlineData("", Rel.NextPage)]
		[InlineData("?pageNumber=2", Rel.PreviousPage)]
		public async Task A1GetReturnsNotNullHrefAsync(string fields, string rel)
		{
			var get = await this.GetHateoasReadAsStringAsync(fields).ConfigureAwait(false);
			Assert.All(get.Values[0].Links, x => Assert.NotNull(x.Href));
			Assert.All(get.Links, x => Assert.NotNull(x.Href));
			Assert.Contains(get.Links, x => x.Rel == rel);
		}

		[Fact]
		public async Task A1GetReturnsOkValidGetAsync()
		{
			var get = await this.GetAsync().ConfigureAwait(false);
			Assert.Equal(HttpStatusCode.OK, get.StatusCode);
		}

		[Theory]
		[AutoMoqData]
		public async Task A2PostReturnsCreatedAsync(StudentForUpdateDto studentForUpdateDto)
		{
			var post = await this.PostAsync(studentForUpdateDto).ConfigureAwait(false);
			Assert.Equal(HttpStatusCode.Created, post.StatusCode);
		}

		[Fact]
		public async Task A3PatchReturnsNotFoundEntityForPatchNotExistAsync()
		{
			var patch = await this.PatchAsync(Guid.NewGuid(), new JsonPatchDocument<StudentDto>()).ConfigureAwait(false);
			Assert.Equal(HttpStatusCode.NotFound, patch.StatusCode);
		}

		[Fact]
		public async Task A5DeleteReturnsNotFoundEntityAsync()
		{
			var delete = await this.DeleteAsync(Guid.NewGuid()).ConfigureAwait(false);
			Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);
		}

		[Theory]
		[InlineAutoMoqData("?fields=whichDoesntExist", HttpStatusCode.BadRequest)]
		[InlineAutoMoqData("", HttpStatusCode.OK)]
		public async Task B1GetReturnsCorrectStatusCodeAsync(
			string field,
			HttpStatusCode statusCode,
			StudentForCreationDto studentForCreationDto)
		{
			var post = await this.PostReadAsStringAsync<StudentDto>(studentForCreationDto).ConfigureAwait(false);

			var get = await this.GetAsync(post.Id, field).ConfigureAwait(false);
			Assert.Equal(statusCode, get.StatusCode);
		}

		[Theory]
		[AutoMoqData]
		public async Task B3PatchReturnsNoContentValidPatchAsync(StudentForUpdateDto studentForUpdateDto)
		{
			var post = await this.PostReadAsStringAsync<StudentDto>(studentForUpdateDto).ConfigureAwait(false);

			var patchDoc = new JsonPatchDocument<StudentDto>();
			patchDoc.Replace(x => x.Surname, "Dummy");

			var patch = await this.PatchAsync(post.Id, patchDoc).ConfigureAwait(false);
			Assert.Equal(HttpStatusCode.NoContent, patch.StatusCode);
		}

		[Theory]
		[AutoMoqData]
		public async Task B3PatchReturnsUnprocessableEntityEqualFieldsAsync(StudentForUpdateDto studentForUpdateDto)
		{
			var post = await this.PostReadAsStringAsync<StudentDto>(studentForUpdateDto).ConfigureAwait(false);

			var patchDoc = new JsonPatchDocument<StudentDto>();
			patchDoc.Replace(x => x.Name, post.Surname);

			var patch = await this.PatchAsync(post.Id, patchDoc).ConfigureAwait(false);
			Assert.Equal(HttpStatusCode.UnprocessableEntity, patch.StatusCode);
		}

		[Theory]
		[AutoMoqData]
		public async Task C5DeleteRemoveStudentAsync(StudentForCreationDto studentForCreationDto)
		{
			var post = await this.PostReadAsStringAsync<StudentDto>(studentForCreationDto).ConfigureAwait(false);

			var delete = await this.DeleteAsync(post.Id).ConfigureAwait(false);
			Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

			var get = await this.GetAsync(post.Id).ConfigureAwait(false);
			Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
		}
	}

	public partial class ApiPeopleControllerShould
	{
		// <--------------------------------------------------------GET-------------------------------------------------------->
		private Task<HttpResponseMessage> GetAsync(Guid id, string fields = "") =>
			this.client.GetAsync(new Uri(Api.Student).ToApiUrl(id, fields));

		private Task<HttpResponseMessage> GetAsync(string fields = "") =>
			this.client.GetAsync(new Uri(Api.People).ToApiUrl(fields));

		private async Task<T> GetReadAsStringAsync<T>(string fields = "") =>
			JsonConvert.DeserializeObject<T>(await (await this.GetAsync(fields).ConfigureAwait(false)).Content.ReadAsStringAsync().ConfigureAwait(false));

		private Task<HateoasDto> GetHateoasReadAsStringAsync(string fields = "")
		{
			this.client.DefaultRequestHeaders.TryAddWithoutValidation(Header.Accept, MediaType.OutputFormatterJson);
			return this.GetReadAsStringAsync<HateoasDto>(fields);
		}

		// <--------------------------------------------------------GET-------------------------------------------------------->

		// <--------------------------------------------------------POST-------------------------------------------------------->
		private Task<HttpResponseMessage> PostAsync(StudentForManipulationDto studentDto) =>
			this.client.PostAsync(new Uri(Api.People), studentDto.Content(MediaType.PatchFormatterJson));

		private async Task<T> PostReadAsStringAsync<T>(StudentForManipulationDto student) =>
			JsonConvert.DeserializeObject<T>(await (await this.PostAsync(student).ConfigureAwait(false)).Content.ReadAsStringAsync().ConfigureAwait(false));

		// <--------------------------------------------------------POST-------------------------------------------------------->

		// <--------------------------------------------------------PATCH-------------------------------------------------------->
		private Task<HttpResponseMessage> PatchAsync(Guid studentId, JsonPatchDocument<StudentDto> patchDoc) =>
			this.client.PatchAsync(new Uri(Api.Student).ToApiUrl(studentId), patchDoc.Content(MediaType.PatchFormatterJson));

		// <--------------------------------------------------------PATCH-------------------------------------------------------->

		// <--------------------------------------------------------DELETE-------------------------------------------------------->
		private Task<HttpResponseMessage> DeleteAsync(Guid studentId) =>
			this.client.DeleteAsync(new Uri(Api.Student).ToApiUrl(studentId));

		// <--------------------------------------------------------DELETE-------------------------------------------------------->
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
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
#pragma warning restore CA1812 // Avoid uninstantiated internal classes
	}
}