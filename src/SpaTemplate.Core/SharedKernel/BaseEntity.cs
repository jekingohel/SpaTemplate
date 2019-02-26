using System;
using System.Collections.Generic;
using SpaTemplate.Core.Hateoas;

namespace SpaTemplate.Core.SharedKernel
{
	// This can be modified to BaseEntity<TId> to support multiple key types (e.g. Guid)
	public abstract class BaseEntity : IShapeData
	{
		public List<BaseDomainEvent> Events = new List<BaseDomainEvent>();
		public Guid Id { get; set; }
	}
}