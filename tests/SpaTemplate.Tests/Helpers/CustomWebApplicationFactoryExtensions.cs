using System.Net.Http;
using SpaTemplate.Core.Hateoas;
using SpaTemplate.Web.Core;

namespace SpaTemplate.Tests.Helpers
{
	public static class CustomWebApplicationFactoryExtensions
	{
		private static HttpClient _client;

		public static HttpClient CreateClientWithDefaultRequestHeaders(
			this CustomWebApplicationFactory<Startup> factory)
		{
			_client = factory.CreateClient();
            _client.DefaultRequestHeaders.TryAddWithoutValidation(Header.XRealIp, Constants.LocalhostIp);

			return _client;
		}
	}
}