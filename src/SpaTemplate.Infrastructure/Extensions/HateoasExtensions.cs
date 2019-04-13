// -----------------------------------------------------------------------
// <copyright file="HateoasExtensions.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Infrastructure
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Linq.Dynamic.Core;
	using AutoMapper;
	using SpaTemplate.Core.SharedKernel;

	public static class HateoasExtensions
	{
		public static IEnumerable<T> ApplySort<T>(
			this IEnumerable<T> source,
			string orderBy,
			IDictionary<string,
			IPropertyMappingValue> mappingDictionary)
		{
			if (source == null)
				throw new NullReferenceException($"Collection {nameof(source)} cannot be null");
			if (mappingDictionary == null)
				throw new NullReferenceException($"Collection {nameof(mappingDictionary)} cannot be null");

			if (string.IsNullOrWhiteSpace(orderBy)) return source;

			foreach (var orderByClause in orderBy.Split(',').Reverse())
			{
				var trimmedOrderByClause = orderByClause.Trim();

				var orderDescending = trimmedOrderByClause.EndsWith(" desc", StringComparison.Ordinal);

				var propertyName = trimmedOrderByClause.RemoveSuffix();
				if (!mappingDictionary.ContainsKey(propertyName))
					throw new ArgumentException($"Key mapping for {propertyName} is missing");

				var propertyMappingValue = mappingDictionary[propertyName];

				if (propertyMappingValue == null)
					throw new NullReferenceException($"Collection {nameof(propertyMappingValue)} cannot be null");

				foreach (var destinationProperty in propertyMappingValue.DestinationProperties.Reverse())
				{
					if (propertyMappingValue.Revert) orderDescending = !orderDescending;
					source = source.AsQueryable().OrderBy(destinationProperty + (orderDescending ? " descending" : " ascending"));
				}
			}

			return source;
		}

		public static IDictionary<string, object> ShapeDataWithoutParameters<TDto, TEntity>(
			this TEntity entity,
			Func<Guid, string, IEnumerable<ILinkDto>> function,
			string fields = "")
			where TDto : IDto
			where TEntity : BaseEntity
		{
			var dictionary = (IDictionary<string, object>)Mapper.Map<TDto>(entity).ShapeDataObject(fields);
			dictionary.Add(Constants.KeyLink, function?.Invoke(entity.Id, fields));
			return dictionary;
		}

		private static string RemoveSuffix(this string str) => str.IndexOf(" ", StringComparison.Ordinal) == -1
			? str
			: str.Remove(str.IndexOf(" ", StringComparison.Ordinal));
	}
}