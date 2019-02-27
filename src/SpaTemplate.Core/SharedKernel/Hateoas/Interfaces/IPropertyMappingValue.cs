using System.Collections.Generic;

namespace SpaTemplate.Core.SharedKernel
{
	public interface IPropertyMappingValue
	{
		IEnumerable<string> DestinationProperties { get; }
		bool Revert { get; }
	}
}