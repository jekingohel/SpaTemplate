namespace SpaTemplate.Core
{
	public interface IHandle<T> where T : BaseDomainEvent
	{
		void Handle(T domainEvent);
	}
}