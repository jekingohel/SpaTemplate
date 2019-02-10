using System.Collections.Generic;

namespace SpaTemplate.Core
{
	public interface IPropertyMappingValue
	{
		IEnumerable<string> DestinationProperties { get; }
		bool Revert { get; }
	}
}