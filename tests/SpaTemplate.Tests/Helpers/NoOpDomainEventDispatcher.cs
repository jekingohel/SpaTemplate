using SpaTemplate.Core.SharedKernel;

namespace SpaTemplate.Tests.Helpers
{
	public class NoOpDomainEventDispatcher : IDomainEventDispatcher
	{
		public void Dispatch(BaseDomainEvent domainEvent)
		{
		}
	}
}