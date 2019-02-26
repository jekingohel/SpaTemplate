using System;
using SpaTemplate.Core.Hateoas;

namespace SpaTemplate.Core.FacultyContext
{
	// Note: doesn't expose events or behavior
	public class StudentDto : IDto
	{
		public Guid Id { get; set; }

		public string Name { get; set; }
		public string Surname { get; set; }
		public int Age { get; set; }
		public bool IsDone { get; }
	}
}