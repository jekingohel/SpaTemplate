using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SpaTemplate.Core.FacultyContext;
using SpaTemplate.Core.SharedKernel;
using SpaTemplate.Tests.Helpers;
using SpaTemplate.Web.Core;
using Xunit;

namespace SpaTemplate.Tests.FunctionalTests
{
	public partial class ApiPeopleCollectionsControllerShould : IClassFixture<CustomWebApplicationFactory<Startup>>
	{
		public ApiPeopleCollectionsControllerShould(CustomWebApplicationFactory<Startup> factory) =>
			_client = factory.CreateClientWithDefaultRequestHeaders();

		private readonly HttpClient _client;

		[Theory]
		[AutoMoqData]
		public async Task CreateCollection_AfterValidPost(IEnumerable<StudentForManipulationDto> dtos)
		{
			var post = await PostAsync(dtos);
			Assert.Equal(HttpStatusCode.Created, post.StatusCode);

			var dtoList =
				JsonConvert.DeserializeObject<IEnumerable<StudentForCreationDto>>(
					await post.Content.ReadAsStringAsync());
			Assert.Equal(3, dtoList.Count());
		}

		[Theory]
		[AutoMoqData]
		public async Task ReturnsCollection_AfterValidGet(IEnumerable<StudentForManipulationDto> dtos)
		{
			var post = await PostAsync(dtos);
			Assert.Equal(HttpStatusCode.Created, post.StatusCode);

			var dtoList =
				JsonConvert.DeserializeObject<IEnumerable<StudentDto>>(await post.Content.ReadAsStringAsync());

			var get = await GetAsync(dtoList.Select(x => x.Id).Take(3));

			Assert.Equal(HttpStatusCode.OK, get.StatusCode);

			var dtoCollection =
				JsonConvert.DeserializeObject<List<StudentDto>>(await get.Content.ReadAsStringAsync());

			Assert.Equal(3, dtoCollection.Count);
		}

		[Fact]
		public async Task ReturnsBadRequest_NoIds()
		{
			var get = await GetAsync();

			Assert.Equal(HttpStatusCode.BadRequest, get.StatusCode);
		}

		[Fact]
		public async Task ReturnsNotFound_IdsNotExist()
		{
            var ids = new List<Guid>
            {
                Guid.NewGuid(),
                Guid.NewGuid()
            };
			var get = await GetAsync(ids);

			Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
		}
	}

	public partial class ApiPeopleCollectionsControllerShould
	{
		private Task<HttpResponseMessage> PostAsync(IEnumerable<StudentForManipulationDto> dtos) => 
            _client.PostAsync(Api.StudentCollections, dtos.Content(MediaType.InputFormatterJson));

        private Task<HttpResponseMessage> GetAsync(IEnumerable<Guid> ids = null) =>
			_client.GetAsync(Api.StudentCollectionsIds.ToApiUrl(ids));
	}
}