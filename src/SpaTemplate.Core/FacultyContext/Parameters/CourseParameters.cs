using SpaTemplate.Core.Hateoas;

namespace SpaTemplate.Core.FacultyContext
{
	public class CourseParameters : IParameters
	{
		private const int MaxPageSize = 50;

		private int _pageSize = 10;
		public int PageNumber { get; set; } = 1;

		public int PageSize
		{
			get => _pageSize;
			set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
		}

		public string SearchQuery { get; set; }

		public string OrderBy { get; set; } = "Title";

		public string Fields { get; set; }
	}
}