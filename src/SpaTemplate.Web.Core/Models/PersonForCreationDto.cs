using System.Collections.Generic;

namespace SpaTemplate.Web.Core
{
	public class PersonForCreationDto : PersonForManipulationDto
	{
		public ICollection<CourseForCreationDto> CourseForCreationDtos { get; set; }
			= new List<CourseForCreationDto>();
	}
}