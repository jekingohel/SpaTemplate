// -----------------------------------------------------------------------
// <copyright file="HateoasDto.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Core.SharedKernel
{
	using System.Collections.Generic;

	public sealed class HateoasDto
	{
		private HateoasDto(IEnumerable<IDictionary<string, object>> values, IEnumerable<ILinkDto> links)
		{
			this.Values = values;
			this.Links = links;
		}

		public IEnumerable<ILinkDto> Links { get; }

		public IEnumerable<IDictionary<string, object>> Values { get; }

		public static HateoasDto CreateHateoasDto(
			IEnumerable<IDictionary<string, object>> values,
			IEnumerable<ILinkDto> links) => new HateoasDto(values, links);
	}
}