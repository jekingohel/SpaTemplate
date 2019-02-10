namespace SpaTemplate.Core
{
	public interface IDomainEventDispatcher
	{
		void Dispatch(BaseDomainEvent domainEvent);
	}
}