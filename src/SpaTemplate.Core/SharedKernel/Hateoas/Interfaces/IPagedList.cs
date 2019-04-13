// -----------------------------------------------------------------------
// <copyright file="IPagedList.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Core.SharedKernel
{
	using System.Collections.Generic;

	public interface IPagedList<T> : IList<T>
		where T : BaseEntity
	{
		int CurrentPage { get; }

		int TotalPages { get; }

		int PageSize { get; }

		int TotalCount { get; }

		bool HasPrevious { get; }

		bool HasNext { get; }
	}
}