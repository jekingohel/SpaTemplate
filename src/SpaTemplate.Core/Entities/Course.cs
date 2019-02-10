namespace SpaTemplate.Core
{
	public class Course : BaseEntity
	{
		public string Title { get; set; }
		public string Description { get; set; }

		public Person Person { get; set; }
	}
}