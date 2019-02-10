using System;
using SpaTemplate.Core;

namespace SpaTemplate.Web.Core
{
	public class CourseDto : IDto
	{
		public Guid Id { get; set; }

		public string Title { get; set; }

		public string Description { get; set; }
	}
}