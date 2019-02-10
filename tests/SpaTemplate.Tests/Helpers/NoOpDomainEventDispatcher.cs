using SpaTemplate.Core;

namespace SpaTemplate.Tests.Helpers
{
	public class NoOpDomainEventDispatcher : IDomainEventDispatcher
	{
		public void Dispatch(BaseDomainEvent domainEvent)
		{
		}
	}
}