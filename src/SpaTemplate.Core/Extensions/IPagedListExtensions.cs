namespace SpaTemplate.Core
{
	public static class IPagedListExtensions
	{
		public static BasePagination CreateBasePagination<T>(this IPagedList<T> pagedList)
			where T : BaseEntity => new BasePagination
		{
			TotalCount = pagedList.TotalCount,
			PageSize = pagedList.PageSize,
			CurrentPage = pagedList.CurrentPage,
			TotalPages = pagedList.TotalPages
		};
	}
}