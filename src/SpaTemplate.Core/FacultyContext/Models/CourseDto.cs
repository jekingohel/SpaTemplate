using System;
using SpaTemplate.Core.Hateoas;

namespace SpaTemplate.Core.FacultyContext
{
	public class CourseDto : IDto
	{
		public Guid Id { get; set; }

		public string Title { get; set; }

		public string Description { get; set; }
	}
}