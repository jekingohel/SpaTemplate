// -----------------------------------------------------------------------
// <copyright file="PagedList.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Core.SharedKernel
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public class PagedList<T> : List<T>, IPagedList<T>
		where T : BaseEntity
	{
		public PagedList()
		{
		}

		private PagedList(IEnumerable<T> items, int count, int pageNumber, int pageSize)
		{
			this.TotalCount = count;
			this.PageSize = pageSize;
			this.CurrentPage = pageNumber;
			this.TotalPages = (int)Math.Ceiling(count / (double)pageSize);
			this.AddRange(items);
		}

		public int CurrentPage { get; }

		public bool HasNext => this.CurrentPage < this.TotalPages;

		public bool HasPrevious => this.CurrentPage > 1;

		public int PageSize { get; }

		public int TotalCount { get; }

		public int TotalPages { get; }

		public static PagedList<T> Create(List<T> source, int pageNumber, int pageSize)
		{
			var items = source.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize);
			return new PagedList<T>(items, source.Count, pageNumber, pageSize);
		}
	}
}