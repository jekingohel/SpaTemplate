using System.Collections.Generic;

namespace SpaTemplate.Core.SharedKernel
{
	public class PropertyMapping : IPropertyMapping
	{
		public PropertyMapping(IDictionary<string, IPropertyMappingValue> mappingDictionary) =>
			MappingDictionary = mappingDictionary;

		public IDictionary<string, IPropertyMappingValue> MappingDictionary { get; }
	}
}