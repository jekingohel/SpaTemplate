using System.Collections.Generic;

namespace SpaTemplate.Web.Core
{
	public class PersonForUpdateDto : PersonForManipulationDto
	{
		public ICollection<CourseForUpdateDto> CourseForUpdateDtos { get; set; }
			= new List<CourseForUpdateDto>();
	}
}