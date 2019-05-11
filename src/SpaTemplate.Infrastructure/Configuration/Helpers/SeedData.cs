// -----------------------------------------------------------------------
// <copyright file="SeedData.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Infrastructure
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using IdentityServer4.EntityFramework.DbContexts;
	using IdentityServer4.EntityFramework.Mappers;
	using Microsoft.AspNetCore.Identity;
	using Microsoft.EntityFrameworkCore;
	using Microsoft.Extensions.DependencyInjection;
	using SpaTemplate.Core.FacultyContext;
	using Xeinaemm.Identity;

	public static class SeedData
	{
		public static void EnsureSeedTestData(this IServiceProvider provider)
		{
			var dbContext = provider.GetRequiredService<ApplicationDbContext>();
			dbContext.Database.Migrate();
			for (var i = 0; i < 20; i++) AddStudent(dbContext, $"Name{i}", $"Surname{i}", i);
			dbContext.SaveChanges();
		}

		public static async Task EnsureIdentitySeedDataAsync(this IServiceProvider provider, IIdentitySeedData seedData)
		{
			provider.GetRequiredService<IdentityDbContext>().Database.Migrate();
			provider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();
			provider.GetRequiredService<ConfigurationDbContext>().Database.Migrate();

			var userMgr = provider.GetRequiredService<UserManager<IdentityUser>>();
			foreach (var user in seedData.Users)
			{
				var usr = await userMgr.FindByNameAsync(user.Username).ConfigureAwait(false);
				if (usr == null)
				{
					usr = new IdentityUser { UserName = user.Username };
					var result = await userMgr.CreateAsync(usr, user.Password).ConfigureAwait(false);
					if (!result.Succeeded) throw new Exception(result.Errors.First().Description);

					usr = await userMgr.FindByNameAsync(user.Username).ConfigureAwait(false);
					result = await userMgr.AddClaimsAsync(usr, user.Claims).ConfigureAwait(false);
					if (!result.Succeeded) throw new Exception(result.Errors.First().Description);
				}
			}

			var context = provider.GetRequiredService<ConfigurationDbContext>();
			if (!context.Clients.Any())
			{
				foreach (var client in seedData.Clients)
					await context.Clients.AddAsync(client.ToEntity()).ConfigureAwait(false);

				await context.SaveChangesAsync().ConfigureAwait(false);
			}

			if (!context.IdentityResources.Any())
			{
				foreach (var resource in seedData.IdentityResources)
					await context.IdentityResources.AddAsync(resource.ToEntity()).ConfigureAwait(false);

				await context.SaveChangesAsync().ConfigureAwait(false);
			}

			if (!context.ApiResources.Any())
			{
				foreach (var resource in seedData.ApiResources)
					await context.ApiResources.AddAsync(resource.ToEntity()).ConfigureAwait(false);

				await context.SaveChangesAsync().ConfigureAwait(false);
			}
		}

		private static void AddStudent(ApplicationDbContext dbContext, string name, string surname, int age)
		{
			var student = new Student
			{
				Name = name,
				Surname = surname,
				Age = age,
			};
			student.Courses.Clear();
			student.Courses.AddRange(SeedCourses());
			dbContext.People.Add(student);
		}

		private static List<Course> SeedCourses()
		{
			var list = new List<Course>();
			for (var i = 0; i < 20; i++)
			{
				list.Add(new Course
				{
					Title = $"Title{i}",
					Description = $"Description{i}",
				});
			}

			return list;
		}
	}
}