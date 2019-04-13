// -----------------------------------------------------------------------
// <copyright file="IPropertyMappingValue.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Core.SharedKernel
{
	using System.Collections.Generic;

	public interface IPropertyMappingValue
	{
		IEnumerable<string> DestinationProperties { get; }

		bool Revert { get; }
	}
}