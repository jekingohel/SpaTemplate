using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SpaTemplate.Core;
using SpaTemplate.Infrastructure;
using SpaTemplate.Tests.Helpers;
using SpaTemplate.Web.Core;
using Xunit;

namespace SpaTemplate.Tests.IntegrationTests
{
	public class PersonRepositoryShould
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

		private RepositoryPerson GetRepository(IPropertyMappingService propertyMapping)
		{
			var options = CreateNewContextOptions();
			var mockDispatcher = new Mock<IDomainEventDispatcher>();

			_dbContext = new AppDbContext(options, mockDispatcher.Object);
			return new RepositoryPerson(_dbContext, propertyMapping);
		}

		[Theory]
		[AutoMoqData]
		public void AssignableFromIPagedList(RepositoryPerson sut)
		{
			Assert.IsAssignableFrom<IRepositoryPerson<Person>>(sut);
		}

		private static Guid[] DummyGuidsArray() => new[]
		{
			new Guid("cf23dadb-8997-41b2-8851-d429be769391"),
			new Guid("a2398e93-cceb-4027-8e3c-ba56b9bd6e10"),
			new Guid("015ed34c-f678-48ae-abbe-1be8a79d8f0f"),
			new Guid("e0195020-ab04-4691-b6b7-a6b35fe58874"),
			new Guid("b9b39223-dc15-4dd5-bc16-e7aec7a0cdf4")
		};

		private RepositoryPerson SeedRepo(IPropertyMappingService property)
		{
			var repo = GetRepository(property);
			var ids = DummyGuidsArray();
			repo.Add(new Person {Id = ids[0], Name = "Dummy", Surname = "FizzBuzz"});
			repo.Add(new Person {Id = ids[1], Name = "Dummy", Surname = "FizzBuzz"});
			repo.Add(new Person {Id = ids[2], Name = "Test", Surname = "Buzz"});
			repo.Add(new Person {Id = ids[3], Name = "Test", Surname = "FizzBuzz"});
			repo.Add(new Person {Id = ids[4], Name = "Test", Surname = "FizzBuzz"});
			repo.Commit();
			return repo;
		}

		[Theory]
		[AutoMoqData]
		public void ReturnsChangedPaginationWithSpecifiedPageNumber(PropertyMappingService property)
		{
			var parameters = new PersonParameters {PageSize = 2, PageNumber = 2};
			var repo = SeedRepo(property);

			var actual = repo.GetPagedList<PersonDto>(parameters);

			Assert.Equal(2, actual.CurrentPage);
		}

		[Theory]
		[InlineAutoMoqData(10, 1, 1)]
		[InlineAutoMoqData(2, 1, 3)]
		public void ReturnsCorrectTotalPages(int pageSize, int pageNumber, int totalPages,
			PropertyMappingService property)
		{
			var parameters = new PersonParameters
			{
				PageSize = pageSize,
				PageNumber = pageNumber
			};
			var repo = SeedRepo(property);

			var actual = repo.GetPagedList<PersonDto>(parameters);

			Assert.Equal(totalPages, actual.TotalPages);
		}

		[Theory]
		[InlineAutoMoqData("dummy", 2)]
		[InlineAutoMoqData("Test", 3)]
		public void ReturnsCorrectQueryList(string searchQuery, int totalCount, PropertyMappingService property)
		{
			var parameters = new PersonParameters
			{
				SearchQuery = searchQuery
			};
			var repo = SeedRepo(property);

			var actual = repo.GetPagedList<PersonDto>(parameters);

			Assert.Equal(totalCount, actual.TotalCount);
		}

		[Theory]
		[AutoMoqData]
		public void ReturnsCorrectOrderBy_Title(PropertyMappingService property)
		{
			var parameters = new PersonParameters
			{
				OrderBy = "Name"
			};
			var repo = SeedRepo(property);

			var actual = repo.GetPagedList<PersonDto>(parameters);

			Assert.Equal("Dummy", actual[0].Name);
		}

		[Theory]
		[AutoMoqData]
		public void ReturnsCorrectOrderBy_Description(PropertyMappingService property)
		{
			var parameters = new PersonParameters
			{
				OrderBy = "surname"
			};
			var repo = SeedRepo(property);

			var actual = repo.GetPagedList<PersonDto>(parameters);

			Assert.Equal("Buzz", actual[0].Surname);
		}

		[Theory]
		[AutoMoqData]
		public void ThrowsExceptionOrderBy_Id(PropertyMappingService property)
		{
			var parameters = new PersonParameters
			{
				OrderBy = "Id"
			};
			var repo = SeedRepo(property);
			Assert.Throws<ArgumentException>(() => repo.GetPagedList<PersonDto>(parameters));
		}

		[Fact]
		public void ReturnsList_EqualIds()
		{
			var repo = SeedRepo(Mock.Of<IPropertyMappingService>());

			var ids = DummyGuidsArray();
			var actual = repo.GetEntities(ids);

			Assert.Equal(ids, actual.Select(x => x.Id));
		}
	}
}