using AutoFixture.Xunit2;
using Xunit;

namespace SpaTemplate.Tests.Helpers
{
	public class InlineAutoMoqDataAttribute : CompositeDataAttribute
	{
		public InlineAutoMoqDataAttribute(params object[] values)
			: base(new InlineDataAttribute(values), new AutoMoqDataAttribute())
		{
		}
	}
}