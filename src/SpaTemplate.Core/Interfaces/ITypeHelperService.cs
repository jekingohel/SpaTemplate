namespace SpaTemplate.Core
{
	public interface ITypeHelperService
	{
		bool TypeHasProperties<T>(string fields) where T : IDto;
	}
}