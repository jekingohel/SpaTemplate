using System.Net.Http;
using SpaTemplate.Core;
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
			_client.DefaultRequestHeaders.Add(Header.XRealIp, Constants.LocalhostIp);
			return _client;
		}
	}
}