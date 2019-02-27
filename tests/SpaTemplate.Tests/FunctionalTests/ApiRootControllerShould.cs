﻿using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SpaTemplate.Core.SharedKernel;
using SpaTemplate.Infrastructure.Api;
using SpaTemplate.Tests.Helpers;
using Xunit;

namespace SpaTemplate.Tests.FunctionalTests
{
	public class ApiRootControllerShould : IClassFixture<CustomWebApplicationFactory<Startup>>
	{
		private readonly HttpClient _client;

		public ApiRootControllerShould(CustomWebApplicationFactory<Startup> factory) =>
			_client = factory.CreateClientWithDefaultRequestHeaders();

		[Theory]
		[InlineData(Rel.Self, Method.Get, 0)]
		[InlineData(Rel.People, Method.Get, 1)]
		[InlineData(Rel.CreateStudent, Method.Post, 2)]
		public async Task ReturnsHateoasLinks_Root(string rel, string method, int number)
		{
            _client.DefaultRequestHeaders.TryAddWithoutValidation(Header.Accept, MediaType.OutputFormatterJson);
            var response = await _client.GetAsync(Route.RootApi);
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);

			var result = JsonConvert.DeserializeObject<List<LinkDto>>(await response.Content.ReadAsStringAsync());

			Assert.Equal(3, result.Count);

			Assert.Equal(rel, result[number].Rel);
			Assert.Equal(method, result[number].Method);
		}
	}
}