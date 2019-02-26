namespace SpaTemplate.Core.Hateoas
{
	public class HateoasPagination : BasePagination
	{
		public string PreviousPage { get; set; }
		public string NextPage { get; set; }
	}
}