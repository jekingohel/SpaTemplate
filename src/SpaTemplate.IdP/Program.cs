// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.IdP
{
	using System;
	using System.Diagnostics;
	using System.Threading.Tasks;
	using Microsoft.AspNetCore;
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.DependencyInjection;
	using SpaTemplate.Infrastructure;
	using Xeinaemm.AspNetCore.Identity.IdentityServer;

	public static class Program
	{
#pragma warning disable IDISP004 // Don't ignore return value of type IDisposable.
#pragma warning disable IDISP001 // Dispose created.
		public static async Task Main(string[] args)
		{
			var host = CreateWebHostBuilder(args).Build();

			try
			{
				using (var scope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
				{
					var services = scope.ServiceProvider;
					var config = services.GetRequiredService<IConfiguration>();
					await services.EnsureIdentitySeedDataAsync<CustomIdentityDbContext>(new IdentitySeedData(config)).ConfigureAwait(false);
				}
			}
			catch (Exception e)
			{
				Debug.WriteLine(e);
			}

			host.Run();
		}
#pragma warning restore IDISP001 // Dispose created.
#pragma warning restore IDISP004 // Don't ignore return value of type IDisposable.

		public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.UseStartup<Startup>();
	}
}
