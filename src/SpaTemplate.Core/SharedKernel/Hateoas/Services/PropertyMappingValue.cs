// -----------------------------------------------------------------------
// <copyright file="PropertyMappingValue.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Core.SharedKernel
{
	using System.Collections.Generic;

	public class PropertyMappingValue : IPropertyMappingValue
	{
		public PropertyMappingValue(
			IEnumerable<string> destinationProperties,
			bool revert = false)
		{
			this.DestinationProperties = destinationProperties;
			this.Revert = revert;
		}

		public IEnumerable<string> DestinationProperties { get; }

		public bool Revert { get; }
	}
}