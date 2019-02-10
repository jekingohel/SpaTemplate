using System;
using System.Collections.Generic;

namespace SpaTemplate.Core
{
	// This can be modified to BaseEntity<TId> to support multiple key types (e.g. Guid)
	public abstract class BaseEntity
	{
		public List<BaseDomainEvent> Events = new List<BaseDomainEvent>();
		public Guid Id { get; set; }
	}
}