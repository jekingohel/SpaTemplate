using System.Collections.Generic;

namespace SpaTemplate.Core
{
	public class Person : BaseEntity
	{
		public string Name { get; set; }
		public string Surname { get; set; }
		public int Age { get; set; }
		
		public bool IsDone { get; private set; }

		public ICollection<Course> Courses { get; set; } = new List<Course>();

		public void MarkComplete()
		{
			IsDone = true;
			Events.Add(new PersonCompletedEvent(this));
		}
	}
}