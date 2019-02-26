using System;
using System.Linq;
using SpaTemplate.Core.FacultyContext;
using SpaTemplate.Core.Hateoas;
using SpaTemplate.Tests.Helpers;
using Xunit;

namespace SpaTemplate.Tests.IntegrationTests
{
	public class StudentServiceShould
	{
		[Theory]
		[AutoMoqData]
		public void AssignableFromIPagedList(StudentService sut)
		{
			Assert.IsAssignableFrom<IStudentService>(sut);
		}

		[Theory]
		[AutoMoqData]
		public void ReturnsChangedPaginationWithSpecifiedPageNumber(PropertyMappingService property)
		{
			var parameters = new StudentParameters {PageSize = 2, PageNumber = 2};
            var sut = new StudentService(DbContextHelper.SeedRepo(property), new TypeHelperService(), new PropertyMappingService());

			var actual = sut.GetPagedList(parameters);

			Assert.Equal(2, actual.CurrentPage);
		}

		[Theory]
		[InlineAutoMoqData(10, 1, 1)]
		[InlineAutoMoqData(2, 1, 3)]
		public void ReturnsCorrectTotalPages(int pageSize, int pageNumber, int totalPages,
			PropertyMappingService property)
		{
			var parameters = new StudentParameters
			{
				PageSize = pageSize,
				PageNumber = pageNumber
			};
            var sut = new StudentService(DbContextHelper.SeedRepo(property), new TypeHelperService(), new PropertyMappingService());

			var actual = sut.GetPagedList(parameters);

			Assert.Equal(totalPages, actual.TotalPages);
		}

		[Theory]
		[InlineData("dummy", 2)]
		[InlineData("Test", 3)]
		public void ReturnsCorrectQueryList(string searchQuery, int totalCount)
		{
			var parameters = new StudentParameters
			{
				SearchQuery = searchQuery
			};
            var sut = new StudentService(DbContextHelper.SeedRepo(new PropertyMappingService()), new TypeHelperService(), new PropertyMappingService());

			var actual = sut.GetPagedList(parameters);

			Assert.Equal(totalCount, actual.TotalCount);
		}

		[Theory]
		[AutoMoqData]
		public void ReturnsCorrectOrderBy_Title(PropertyMappingService property)
		{
			var parameters = new StudentParameters
			{
				OrderBy = "Name"
			};
            var sut = new StudentService(DbContextHelper.SeedRepo(property), new TypeHelperService(), new PropertyMappingService());

			var actual = sut.GetPagedList(parameters);

			Assert.Equal("Dummy", actual[0].Name);
		}

		[Theory]
		[AutoMoqData]
		public void ReturnsCorrectOrderBy_Description(PropertyMappingService property)
		{
			var parameters = new StudentParameters
			{
				OrderBy = "surname"
			};
            var sut = new StudentService(DbContextHelper.SeedRepo(property), new TypeHelperService(), new PropertyMappingService());

			var actual = sut.GetPagedList(parameters);

			Assert.Equal("Buzz", actual[0].Surname);
		}

		[Theory]
		[AutoMoqData]
		public void ThrowsExceptionOrderBy_Id(PropertyMappingService property)
		{
			var parameters = new StudentParameters
			{
				OrderBy = "Id"
			};
            var sut = new StudentService(DbContextHelper.SeedRepo(property), new TypeHelperService(), new PropertyMappingService());

			Assert.Throws<ArgumentException>(() => sut.GetPagedList(parameters));
		}

		[Fact]
		public void ReturnsList_EqualIds()
		{
            var sut = new StudentService(DbContextHelper.SeedRepo(new PropertyMappingService()), new TypeHelperService(), new PropertyMappingService());

			var ids = DbContextHelper.DummyGuidsArray();
			var actual = sut.GetCollection(ids);

			Assert.Equal(ids, actual.Select(x => x.Id));
		}
	}
}