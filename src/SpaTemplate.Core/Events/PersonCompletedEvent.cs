namespace SpaTemplate.Core
{
	public class PersonCompletedEvent : BaseDomainEvent
	{
		public PersonCompletedEvent(Person completedItem) => CompletedItem = completedItem;

		public Person CompletedItem { get; set; }
	}
}