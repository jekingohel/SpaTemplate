using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;

namespace SpaTemplate.Tests.Helpers
{
	public class AutoMoqDataAttribute : AutoDataAttribute
	{
		private static readonly Fixture FixtureInstance = new Fixture();

		public AutoMoqDataAttribute() : base(() => FixtureInstance.Customize(new AutoMoqCustomization()))
		{
		    FixtureInstance.Behaviors.Remove(new ThrowingRecursionBehavior());
		    FixtureInstance.Behaviors.Add(new OmitOnRecursionBehavior());
		}
	}
}