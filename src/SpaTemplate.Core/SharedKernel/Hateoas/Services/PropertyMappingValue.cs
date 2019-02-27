using System.Collections.Generic;

namespace SpaTemplate.Core.SharedKernel
{
	public class PropertyMappingValue : IPropertyMappingValue
	{
		public PropertyMappingValue(IEnumerable<string> destinationProperties,
			bool revert = false)
		{
			DestinationProperties = destinationProperties;
			Revert = revert;
		}

		public IEnumerable<string> DestinationProperties { get; }
		public bool Revert { get; }
	}
}