using System;

namespace SpaTemplate.Core
{
	public abstract class BaseDomainEvent
	{
		public DateTime DateOccurred { get; protected set; } = DateTime.UtcNow;
	}
}