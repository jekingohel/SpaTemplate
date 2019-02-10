using SpaTemplate.Core;
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

		[Theory]
		[AutoMoqData]
		public void BeAssignableFromInterface(PropertyMappingService sut)
		{
			Assert.IsAssignableFrom<IPropertyMappingService>(sut);
		}

		private class DummyEntity : BaseEntity
		{
		}

		private class DummyEntityDto : IDto
		{
		}
	}
}