using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SpaTemplate.Core;
using SpaTemplate.Infrastructure;
using SpaTemplate.Tests.Helpers;
using Xunit;

namespace SpaTemplate.Tests.IntegrationTests
{
	public class EfRepositoryShould
	{
		private AppDbContext _dbContext;

		private static DbContextOptions<AppDbContext> CreateNewContextOptions()
		{
			// Create a fresh service provider, and therefore a fresh
			// InMemory database instance.
			var serviceProvider = new ServiceCollection()
				.AddEntityFrameworkInMemoryDatabase()
				.BuildServiceProvider();

			// Create a new options instance telling the context to use an
			// InMemory database and the new service provider.
			var builder = new DbContextOptionsBuilder<AppDbContext>();
			builder.UseInMemoryDatabase("rest")
				.UseInternalServiceProvider(serviceProvider);

			return builder.Options;
		}

		private EfRepository GetRepository()
		{
			var options = CreateNewContextOptions();
			var mockDispatcher = new Mock<IDomainEventDispatcher>();

			_dbContext = new AppDbContext(options, mockDispatcher.Object);
			return new EfRepository(_dbContext);
		}

		[Theory]
		[AutoMoqData]
		public void AddItemAndSetId(Person item)
		{
			var repository = GetRepository();

			repository.Add(item);
			Assert.True(repository.Commit());
			var newItem = repository.List<Person>().First();

			Assert.Equal(item, newItem);
			Assert.True(newItem.Id != Guid.Empty);
		}

		[Theory]
		[AutoMoqData]
		public void DeleteItemAfterAddingIt(Person item)
		{
			var repository = GetRepository();

			repository.Add(item);
			repository.Delete(item);
			Assert.True(repository.Commit());
		}

		[Theory]
		[AutoMoqData]
		public void ExistsItemAfterAddingIt(Person item)
		{
			var repository = GetRepository();

			repository.Add(item);
			Assert.True(repository.Commit());
			Assert.True(repository.EntityExists<Person>(item.Id));
		}

		[Theory]
		[AutoMoqData]
		public void UpdateItemAfterAddingIt(Person item)
		{
			var repository = GetRepository();
			var initialTitle = Guid.NewGuid().ToString();
			item.Name = initialTitle;

			repository.Add(item);
			Assert.True(repository.Commit());

			_dbContext.Entry(item).State = EntityState.Detached;

			var newItem = repository.List<Person>()
				.FirstOrDefault(i => i.Name == initialTitle);
			Assert.NotNull(newItem);
			Assert.NotSame(item, newItem);
			var newTitle = Guid.NewGuid().ToString();
			newItem.Name = newTitle;

			repository.UpdateEntity(newItem);
			Assert.True(repository.Commit());

			var updatedItem = repository.List<Person>()
				.FirstOrDefault(i => i.Name == newTitle);

			Assert.NotNull(updatedItem);
			Assert.NotEqual(item.Name, updatedItem.Name);
			Assert.Equal(newItem.Id, updatedItem.Id);
		}
	}
}