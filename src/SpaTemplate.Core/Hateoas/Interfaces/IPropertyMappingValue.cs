using System.Collections.Generic;

namespace SpaTemplate.Core.Hateoas
{
	public interface IPropertyMappingValue
	{
		IEnumerable<string> DestinationProperties { get; }
		bool Revert { get; }
	}
}