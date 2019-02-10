using System.ComponentModel.DataAnnotations;

namespace SpaTemplate.Web.Core
{
	public class PersonForManipulationDto
	{
		[Required]
		public string Name { get; set; }
		public string Surname { get; set; }
		public int Age { get; set; }
		public bool IsDone { get; set; }
	}
}