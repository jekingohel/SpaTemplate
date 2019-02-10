using System;
using SpaTemplate.Core;

namespace SpaTemplate.Web.Core
{
	// Note: doesn't expose events or behavior
	public class PersonDto : IDto
	{
		public Guid Id { get; set; }

		public string Name { get; set; }
		public string Surname { get; set; }
		public int Age { get; set; }
		public bool IsDone { get; private set; }
	}
}