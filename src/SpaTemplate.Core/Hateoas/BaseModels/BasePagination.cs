﻿namespace SpaTemplate.Core.Hateoas
{
	public class BasePagination
	{
		public int TotalCount { get; set; }
		public int PageSize { get; set; }
		public int CurrentPage { get; set; }
		public int TotalPages { get; set; }
	}
}