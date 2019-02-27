namespace SpaTemplate.Core.SharedKernel
{
	public interface ITypeHelperService
	{
		bool TypeHasProperties<T>(string fields) where T : IDto;
	}
}