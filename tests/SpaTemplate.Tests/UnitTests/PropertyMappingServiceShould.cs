using SpaTemplate.Core.Hateoas;
using SpaTemplate.Core.SharedKernel;
using SpaTemplate.Tests.Helpers;
using Xunit;

namespace SpaTemplate.Tests.UnitTests
{
	public class PropertyMappingServiceShould
	{
		[Theory]
		[InlineAutoMoqData("Dummy")]
		public void ReturnsTrue_ValidFields(string field, PropertyMappingService property)
		{
			Assert.True(property.ValidMappingExistsFor<DummyEntityDto, DummyEntity>(field));
		}

		[Fact]
		public void BeAssignableFromInterface()
		{
			Assert.IsAssignableFrom<IPropertyMappingService>(new PropertyMappingService());
		}

		private class DummyEntity : BaseEntity
		{
		}

		private class DummyEntityDto : IDto
		{
		}
	}
}