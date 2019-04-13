// -----------------------------------------------------------------------
// <copyright file="PropertyMappingService.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Core.SharedKernel
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public class PropertyMappingService : IPropertyMappingService
	{
		private static readonly Dictionary<string, IPropertyMappingValue> PropertyMappingDictionary =
			new Dictionary<string, IPropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
			{
				{ "Name", new PropertyMappingValue(new List<string> { "Name" }) },
				{ "Surname", new PropertyMappingValue(new List<string> { "Surname" }) },
				{ "Age", new PropertyMappingValue(new List<string> { "Age" }, true) },
				{ "IsDone", new PropertyMappingValue(new List<string> { "IsDone" }, true) },
				{ "FullName", new PropertyMappingValue(new List<string> { "Name", "Surname" }) },
				{ "Dummy", new PropertyMappingValue(new List<string> { "Dummy" }) },
				{ "Title", new PropertyMappingValue(new List<string> { "Title" }) },
				{ "Description", new PropertyMappingValue(new List<string> { "Description" }) },
			};

		private readonly IList<IPropertyMapping> propertyMappings = new List<IPropertyMapping>();

		public PropertyMappingService()
		{
			this.propertyMappings.Add(new PropertyMapping(PropertyMappingDictionary));
		}

		public IDictionary<string, IPropertyMappingValue> GetPropertyMapping
			<TSource, TDestination>()
			where TSource : IDto
			where TDestination : BaseEntity
		{
			if (this.propertyMappings.OfType<PropertyMapping>().Count() == 1)
				return this.propertyMappings.OfType<PropertyMapping>().First().MappingDictionary;

			throw new Exception(
				$"Cannot find exact property mapping instance for <{typeof(TSource)},{typeof(TDestination)}");
		}

		public bool ValidMappingExistsFor<TSource, TDestination>(string fields)
			where TSource : IDto
			where TDestination : BaseEntity
		{
			if (string.IsNullOrWhiteSpace(fields)) return true;

			return (from field in fields.Split(',')
					select field.Trim()
					into trimmedField
					let indexOfFirstSpace = trimmedField.IndexOf(" ", StringComparison.Ordinal)
					select indexOfFirstSpace == -1 ? trimmedField : trimmedField.Remove(indexOfFirstSpace))
				.All(propertyName => this.GetPropertyMapping<TSource, TDestination>().ContainsKey(propertyName));
		}
	}
}