// -----------------------------------------------------------------------
// <copyright file="TypeHelperService.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Core.SharedKernel
{
	using System.Linq;
	using System.Reflection;

	public class TypeHelperService : ITypeHelperService
	{
		public bool TypeHasProperties<T>(string fields)
			where T : IDto => string.IsNullOrWhiteSpace(fields)
				|| fields.Split(',').Select(field => field.Trim())
				.Select(propertyName => typeof(T).GetProperty(
					propertyName,
					PublicInstances()))
				.All(propertyInfo => propertyInfo != null);

		private static BindingFlags PublicInstances() =>
			BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance;
	}
}