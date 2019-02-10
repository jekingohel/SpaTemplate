namespace SpaTemplate.Core
{
	public interface IParameters
	{
		int PageNumber { get; set; }

		int PageSize { get; set; }

		string SearchQuery { get; set; }

		string OrderBy { get; set; }

		string Fields { get; set; }
	}
}