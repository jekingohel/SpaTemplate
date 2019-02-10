using System.Collections.Generic;

namespace SpaTemplate.Core
{
	public class HateoasDto
	{
		private HateoasDto(IEnumerable<IDictionary<string, object>> values, IEnumerable<ILinkDto> links)
		{
			Values = values;
			Links = links;
		}

		public IEnumerable<IDictionary<string, object>> Values { get; }
		public IEnumerable<ILinkDto> Links { get; }

		public static HateoasDto CreateHateoasDto(IEnumerable<IDictionary<string, object>> values,
			IEnumerable<ILinkDto> links) => new HateoasDto(values, links);
	}
}