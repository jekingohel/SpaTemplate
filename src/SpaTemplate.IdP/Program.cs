// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.IdP
{
	using System.Threading.Tasks;
	using Microsoft.AspNetCore;
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.Extensions.DependencyInjection;
	using SpaTemplate.Infrastructure;

	public static class Program
	{
#pragma warning disable IDISP004 // Don't ignore return value of type IDisposable.
		public static async Task Main(string[] args)
		{
			using (var host = CreateWebHostBuilder(args).Build())
			{
				using (var scope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
				{
					await scope.ServiceProvider.EnsureIdentitySeedDataAsync(new IdentitySeedData()).ConfigureAwait(false);
				}

				host.Run();
			}
		}
#pragma warning restore IDISP004 // Don't ignore return value of type IDisposable.

		public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.UseStartup<Startup>();
	}
}
