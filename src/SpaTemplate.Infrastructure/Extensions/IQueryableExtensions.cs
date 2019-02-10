using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using SpaTemplate.Core;

namespace SpaTemplate.Infrastructure
{
	public static class IQueryableExtensions
	{
		public static IQueryable<T> ApplySort<T>(this IQueryable<T> source, string orderBy,
			IDictionary<string, IPropertyMappingValue> mappingDictionary)
		{
			if (source == null)
				throw new NullReferenceException($"Collection {nameof(source)} cannot be null");
			if (mappingDictionary == null)
				throw new NullReferenceException($"Collection {nameof(mappingDictionary)} cannot be null");

			if (string.IsNullOrWhiteSpace(orderBy)) return source;

			foreach (var orderByClause in orderBy.Split(',').Reverse())
			{
				var trimmedOrderByClause = orderByClause.Trim();

				var orderDescending = trimmedOrderByClause.EndsWith(" desc");

				var propertyName = trimmedOrderByClause.RemoveSuffix();
				if (!mappingDictionary.ContainsKey(propertyName))
					throw new ArgumentException($"Key mapping for {propertyName} is missing");

				var propertyMappingValue = mappingDictionary[propertyName];

				if (propertyMappingValue == null)
					throw new NullReferenceException($"Collection {nameof(propertyMappingValue)} cannot be null");

				foreach (var destinationProperty in propertyMappingValue.DestinationProperties.Reverse())
				{
					if (propertyMappingValue.Revert) orderDescending = !orderDescending;
					source = source.OrderBy(destinationProperty + (orderDescending ? " descending" : " ascending"));
				}
			}

			return source;
		}

		private static string RemoveSuffix(this string str) => str.IndexOf(" ", StringComparison.Ordinal) == -1
			? str
			: str.Remove(str.IndexOf(" ", StringComparison.Ordinal));
	}
}