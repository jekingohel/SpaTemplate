using System.Collections.Generic;

namespace SpaTemplate.Core.FacultyContext
{
	public class StudentForUpdateDto : StudentForManipulationDto
	{
		public List<CourseForUpdateDto> CourseForUpdateDtos { get; set; }
			= new List<CourseForUpdateDto>();
	}
}