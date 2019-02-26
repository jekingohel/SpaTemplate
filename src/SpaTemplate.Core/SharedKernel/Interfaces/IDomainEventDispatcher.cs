namespace SpaTemplate.Core.SharedKernel
{
	public interface IDomainEventDispatcher
	{
		void Dispatch(BaseDomainEvent domainEvent);
	}
}