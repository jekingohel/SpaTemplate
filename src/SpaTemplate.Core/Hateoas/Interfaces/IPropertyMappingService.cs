using System.Collections.Generic;
using SpaTemplate.Core.SharedKernel;

namespace SpaTemplate.Core.Hateoas
{
	public interface IPropertyMappingService
	{
		bool ValidMappingExistsFor<TSource, TDestination>(string fields)
			where TSource : IDto where TDestination : BaseEntity;

		IDictionary<string, IPropertyMappingValue> GetPropertyMapping<TSource, TDestination>()
			where TSource : IDto where TDestination : BaseEntity;
	}
}