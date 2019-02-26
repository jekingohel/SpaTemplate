using System.Collections.Generic;
using SpaTemplate.Core.SharedKernel;

namespace SpaTemplate.Core.FacultyContext
{
	public class Student : Person
	{
		public bool IsDone { get; private set; }

		public ICollection<Course> Courses { get; set; } = new List<Course>();

		public void MarkComplete()
		{
			IsDone = true;
			Events.Add(new StudentCompletedEvent(this));
		}
	}
}