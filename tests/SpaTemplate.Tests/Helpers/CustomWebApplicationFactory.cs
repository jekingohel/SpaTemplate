using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpaTemplate.Core.SharedKernel;
using SpaTemplate.Infrastructure;
using SpaTemplate.Infrastructure.Core;
using SpaTemplate.Web.Core;

namespace SpaTemplate.Tests.Helpers
{
	public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<Startup>
	{
		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
            builder.UseContentRoot(".");
			builder.ConfigureServices(services =>
			{
				var serviceProvider = new ServiceCollection()
					.AddEntityFrameworkInMemoryDatabase()
					.BuildServiceProvider();

				services.AddDbContext<AppDbContext>(options =>
				{
					options.UseInMemoryDatabase("tests-factory");
					options.UseInternalServiceProvider(serviceProvider);
				});

				services.AddScoped<IDomainEventDispatcher, NoOpDomainEventDispatcher>();

				var sp = services.BuildServiceProvider();

				using (var scope = sp.CreateScope())
				{
					var scopedServices = scope.ServiceProvider;
					var db = scopedServices.GetRequiredService<AppDbContext>();

					var logger = scopedServices
						.GetRequiredService<ILogger<CustomWebApplicationFactory<TStartup>>>();

					db.Database.EnsureCreated();

					try
					{
						SeedData.PopulateTestData(db);
					}
					catch (Exception ex)
					{
						logger.LogError(ex, "An error occurred seeding the " +
						                    "database with test messages. Error: {ex.Message}");
					}
				}
			});
		}
	}
}