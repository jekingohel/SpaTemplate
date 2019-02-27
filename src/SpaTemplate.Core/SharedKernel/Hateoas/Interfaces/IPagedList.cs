using System.Collections.Generic;

namespace SpaTemplate.Core.SharedKernel
{
	public interface IPagedList<T> : IList<T> where T : BaseEntity
	{
		int CurrentPage { get; }
		int TotalPages { get; }
		int PageSize { get; }
		int TotalCount { get; }
		bool HasPrevious { get; }
		bool HasNext { get; }
	}
}