using System.Collections.Generic;
using SpaTemplate.Core.SharedKernel;
using SpaTemplate.Tests.Helpers;
using Xunit;

namespace SpaTemplate.Tests.UnitTests
{
	public class PagedListShould
	{
		[Theory]
		[InlineData(1, 1, 5, true, false, 5)]
		[InlineData(1, 2, 5, true, false, 3)]
		[InlineData(2, 1, 5, true, true, 5)]
		[InlineData(2, 2, 5, true, true, 3)]
		[InlineData(3, 2, 5, false, true, 3)]
		[InlineData(5, 1, 5, false, true, 5)]
		public void ReturnsPagedList_CorrectPagination(int pageNumber, int pageSize, int totalCount, bool hasNext,
			bool hasPrevious, int totalPages)
		{
			var pagedList = PagedList<DummyEntity>.Create(DummyList(), pageNumber, pageSize);

			Assert.Equal(pageSize, pagedList.PageSize);
			Assert.Equal(pageNumber, pagedList.CurrentPage);
			Assert.Equal(totalCount, pagedList.TotalCount);
			Assert.Equal(hasNext, pagedList.HasNext);
			Assert.Equal(hasPrevious, pagedList.HasPrevious);
			Assert.Equal(totalPages, pagedList.TotalPages);
        }

		[Theory]
		[AutoMoqData]
		public void AssignableFromIPagedList(PagedList<DummyEntity> sut)
		{
			Assert.IsAssignableFrom<IPagedList<DummyEntity>>(sut);
		}

		private static List<DummyEntity> DummyList() => new List<DummyEntity>
		{
			new DummyEntity(),
			new DummyEntity(),
			new DummyEntity(),
			new DummyEntity(),
			new DummyEntity()
		};

		public class DummyEntity : BaseEntity
		{
		}
	}
}