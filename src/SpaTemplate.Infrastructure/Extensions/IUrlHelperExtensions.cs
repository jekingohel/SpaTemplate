// -----------------------------------------------------------------------
// <copyright file="IUrlHelperExtensions.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Infrastructure
{
	using System;
	using System.Collections.Generic;
	using Microsoft.AspNetCore.Mvc;
	using SpaTemplate.Core.SharedKernel;

	public static class IUrlHelperExtensions
	{
		public static HateoasPagination CreateHateoasPagination<T>(
			this IUrlHelper urlHelper,
			string routeName,
			IPagedList<T> pagedList,
			IParameters parameter)
			where T : BaseEntity => new HateoasPagination
			{
				TotalCount = pagedList.TotalCount,
				CurrentPage = pagedList.CurrentPage,
				TotalPages = pagedList.TotalPages,
				PageSize = pagedList.PageSize,
				NextPage = urlHelper.NextPage(routeName, parameter, pagedList),
				PreviousPage = urlHelper.PreviousPage(routeName, parameter, pagedList),
			};

		public static IEnumerable<ILinkDto> CreateLinks<TEntity>(
			this IUrlHelper urlHelper,
			string routeName,
			IParameters parameter,
			IPagedList<TEntity> pagedList)
			where TEntity : BaseEntity
		{
			var links = new List<ILinkDto>
			{
				urlHelper.CreateResourceUri(routeName, parameter, ResourceUriType.Current)
					.AddRelAndMethod(Rel.Self, Method.Get),
			};
			if (pagedList.HasNext)
				links.Add(urlHelper.CreateResourceUri(routeName, parameter, ResourceUriType.NextPage)
					.AddRelAndMethod(Rel.NextPage, Method.Get));

			if (pagedList.HasPrevious)
				links.Add(urlHelper.CreateResourceUri(routeName, parameter, ResourceUriType.PreviousPage)
					.AddRelAndMethod(Rel.PreviousPage, Method.Get));

			return links;
		}

		private static string CreatePage(
			this IUrlHelper urlHelper,
			string routeName,
			IParameters resourceParameters,
			int pageNumber = 0) => urlHelper.Link(
			routeName,
			new
			{
				resourceParameters.Fields,
				resourceParameters.OrderBy,
				resourceParameters.SearchQuery,
				pageNumber = resourceParameters.PageNumber + pageNumber,
				resourceParameters.PageSize,
			});

		private static string CreateResourceUri(
			this IUrlHelper urlHelper,
			string routeName,
			IParameters parameters,
			ResourceUriType type)
		{
			switch (type)
			{
				case ResourceUriType.PreviousPage:
					return urlHelper.CreatePage(routeName, parameters, -1);
				case ResourceUriType.NextPage:
					return urlHelper.CreatePage(routeName, parameters, 1);
				case ResourceUriType.Current:
					return urlHelper.CreatePage(routeName, parameters);
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
		}

		private static string NextPage<TEntity>(
			this IUrlHelper urlHelper,
			string routeName,
			IParameters resourceParameters,
			IPagedList<TEntity> entities)
			where TEntity : BaseEntity
			=>
			entities.HasNext
				? urlHelper.CreateResourceUri(routeName, resourceParameters, ResourceUriType.NextPage)
				: null;

		private static string PreviousPage<TEntity>(
			this IUrlHelper urlHelper,
			string routeName,
			IParameters parameter,
			IPagedList<TEntity> entities)
			where TEntity : BaseEntity
			=>
			entities.HasPrevious
				? urlHelper.CreateResourceUri(routeName, parameter, ResourceUriType.PreviousPage)
				: null;
	}
}