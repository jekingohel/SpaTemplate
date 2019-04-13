// -----------------------------------------------------------------------
// <copyright file="BaseEntity.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Core.SharedKernel
{
	using System;
	using System.Collections.Generic;

	// This can be modified to BaseEntity<TId> to support multiple key types (e.g. Guid)
	public abstract class BaseEntity : IShapeData
	{
		public List<BaseDomainEvent> Events = new List<BaseDomainEvent>();

		public Guid Id { get; set; }
	}
}