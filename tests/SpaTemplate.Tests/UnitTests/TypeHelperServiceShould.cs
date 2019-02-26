using AutoFixture.Xunit2;
using SpaTemplate.Core.Hateoas;
using Xunit;

namespace SpaTemplate.Tests.UnitTests
{
	public class TypeHelperServiceShould
	{
		[Theory]
		[InlineAutoData(null)]
		[InlineAutoData("id")]
		[InlineAutoData("Fizz")]
		[InlineAutoData("id,Fizz, buzz")]
		[InlineAutoData("buzz     ")]
		public void ReturnsTrue_NullOrCorrectFields(string fields)
		{
            var sut = new TypeHelperService();
			Assert.True(sut.TypeHasProperties<DummyEntity>(fields));
		}

		[Theory]
		[InlineAutoData("FizzBuzz!")]
		public void ReturnsFalse_FieldNotExist(string fields)
        {
            var sut = new TypeHelperService();
			Assert.False(sut.TypeHasProperties<DummyEntity>(fields));
		}

		[Fact]
		public void BeAssignableFromInterface()
		{
			Assert.IsAssignableFrom<ITypeHelperService>(new TypeHelperService());
		}

		private class DummyEntity : IDto
		{
			public int Id { get; set; }
			public string Fizz { get; set; }
			public string Buzz { get; set; }
		}
	}
}