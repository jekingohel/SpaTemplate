using System;

namespace SpaTemplate.Core.SharedKernel
{
	public abstract class BaseDomainEvent
	{
		public DateTime DateOccurred { get; protected set; } = DateTime.UtcNow;
	}
}