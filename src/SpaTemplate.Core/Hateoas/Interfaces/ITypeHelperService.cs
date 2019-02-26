namespace SpaTemplate.Core.Hateoas
{
	public interface ITypeHelperService
	{
		bool TypeHasProperties<T>(string fields) where T : IDto;
	}
}