using System.Collections.Generic;

namespace SpaTemplate.Core.FacultyContext
{
	public class StudentForCreationDto : StudentForManipulationDto
	{
		public List<CourseForCreationDto> CourseForCreationDtos { get; set; }
			= new List<CourseForCreationDto>();
	}
}