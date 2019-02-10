using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SpaTemplate.Core;
using SpaTemplate.Tests.Helpers;
using SpaTemplate.Web.Core;
using Xunit;

namespace SpaTemplate.Tests.FunctionalTests
{
	public partial class ApiPersonCollectionsControllerShould : IClassFixture<CustomWebApplicationFactory<Startup>>
	{
		public ApiPersonCollectionsControllerShould(CustomWebApplicationFactory<Startup> factory) =>
			_client = factory.CreateClientWithDefaultRequestHeaders();

		private readonly HttpClient _client;

		[Theory]
		[AutoMoqData]
		public async Task CreateCollection_AfterValidPost(IEnumerable<PersonForManipulationDto> dtos)
		{
			var post = await PostAsync(dtos);
			Assert.Equal(HttpStatusCode.Created, post.StatusCode);

			var dtoList =
				JsonConvert.DeserializeObject<IEnumerable<PersonForCreationDto>>(
					await post.Content.ReadAsStringAsync());
			Assert.Equal(3, dtoList.Count());
		}

		[Theory]
		[AutoMoqData]
		public async Task ReturnsCollection_AfterValidGet(IEnumerable<PersonForManipulationDto> dtos)
		{
			var post = await PostAsync(dtos);
			Assert.Equal(HttpStatusCode.Created, post.StatusCode);

			var dtoList =
				JsonConvert.DeserializeObject<IEnumerable<PersonDto>>(await post.Content.ReadAsStringAsync());
			var ids = dtoList.Select(x => x.Id.ToString()).ToList();

			var get = await GetAsync($"{ids[0]},{ids[1]},{ids[2]}");
			Assert.Equal(HttpStatusCode.OK, get.StatusCode);

			var dtoCollection =
				JsonConvert.DeserializeObject<IEnumerable<PersonDto>>(await get.Content.ReadAsStringAsync());

			Assert.Equal(3, dtoCollection.Count());
		}

		[Fact]
		public async Task ReturnsBadRequest_NoIds()
		{
			var get = await GetAsync(" ");

			Assert.Equal(HttpStatusCode.BadRequest, get.StatusCode);
		}

		[Fact]
		public async Task ReturnsNotFound_IdsNotExist()
		{
			var get = await GetAsync("d4ee680c-c29f-4fcd-8264-d5ea46ecadf5, b4ee680c-c29f-4fcd-8264-d5ea46ecadf5");

			Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
		}
	}

	public partial class ApiPersonCollectionsControllerShould
	{
		private Task<HttpResponseMessage> PostAsync(IEnumerable<PersonForManipulationDto> dtos)
		{
			var content = new StringContent(JsonConvert.SerializeObject(dtos),
				Encoding.UTF8,
				MediaType.InputFormatterJson);

			return _client.PostAsync(Route.PersonCollectionsApi, content);
		}

		private Task<HttpResponseMessage> GetAsync(string additionalField) =>
			_client.GetAsync($"{Route.PersonCollectionsApi}/({additionalField})");
	}
}