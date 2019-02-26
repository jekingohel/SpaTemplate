using System;
using SpaTemplate.Core.SharedKernel;

namespace SpaTemplate.Core.FacultyContext
{
	public class Course : BaseEntity
	{
		public string Title { get; set; }
		public string Description { get; set; }

		public Student Student { get; set; }
		public Guid StudentId { get; set; }
    }
}