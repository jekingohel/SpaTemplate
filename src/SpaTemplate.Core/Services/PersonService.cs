using System;

namespace SpaTemplate.Core
{
	public class PersonService : IHandle<PersonCompletedEvent>
	{
		public void Handle(PersonCompletedEvent domainEvent)
		{
			if (domainEvent == null)
				throw new NullReferenceException($"Collection {nameof(domainEvent)} cannot be null");

			// Do Nothing
		}
	}
}