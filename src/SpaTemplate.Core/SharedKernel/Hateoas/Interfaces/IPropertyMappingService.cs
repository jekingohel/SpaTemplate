// -----------------------------------------------------------------------
// <copyright file="IPropertyMappingService.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Core.SharedKernel
{
	using System.Collections.Generic;

	public interface IPropertyMappingService
	{
		bool ValidMappingExistsFor<TSource, TDestination>(string fields)
			where TSource : IDto
			where TDestination : BaseEntity;

		IDictionary<string, IPropertyMappingValue> GetPropertyMapping<TSource, TDestination>()
			where TSource : IDto
			where TDestination : BaseEntity;
	}
}